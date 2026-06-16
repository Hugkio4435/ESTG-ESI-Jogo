using UnityEngine;

using UnityEngine.UI;

using System.Collections.Generic;

using System.Linq;



public class ConnectedPlayer

{

    public string playerName;

    public int avatarId;

    public int globalScore = 0;

}



public class MenuManager : MonoBehaviour

{

    [Header("Os Nossos Paineis")]

    public GameObject mainMenuPanel;

    public GameObject miniGamesPanel;

    public GameObject lobbyPanel;

    public GameObject splitRoomPanel;

    public GameObject jogoCerteiroPanel; // NOVO: O painel do novo jogo!



    [Header("Ecrã de Resultados (Leaderboard)")]

    public GameObject leaderboardPanel;

    public TMPro.TextMeshProUGUI leaderboardListText;



    [Header("Gestores de Minijogos")]

    public SplitRoomManager splitRoomManager;

    public JogoCerteiroManager jogoCerteiroManager; // NOVO: O gestor do novo jogo!



    [Header("UI Elements")]

    public TMPro.TextMeshProUGUI roomCodeText;



    [Header("Lobby UI - Grelha de Jogadores")]

    public TMPro.TextMeshProUGUI[] playerSlotTexts;

    public Image[] playerSlotImages;



    [Header("A Tua Colecao de Avatares")]

    public Sprite[] availableAvatars;

    public Sprite placeholderAvatar;



    private List<ConnectedPlayer> activePlayers = new List<ConnectedPlayer>();



    // NOVO: Variável para o sistema saber qual foi o jogo escolhido

    private string currentSelectedGame = "";



    public void AddPlayerToUI(string playerName, int avatarId)

    {

        if (!activePlayers.Any(p => p.playerName == playerName))

        {

            activePlayers.Add(new ConnectedPlayer { playerName = playerName, avatarId = avatarId });

            RefreshPlayersUI();

        }

    }



    public void RemovePlayerFromUI(string playerName)

    {

        var playerToRemove = activePlayers.FirstOrDefault(p => p.playerName == playerName);

        if (playerToRemove != null)

        {

            activePlayers.Remove(playerToRemove);

            RefreshPlayersUI();

        }

    }



    private void RefreshPlayersUI()

    {

        for (int i = 0; i < playerSlotTexts.Length; i++)

        {

            playerSlotTexts[i].text = "Espaço Vazio";

            playerSlotTexts[i].color = new Color(0f, 0f, 0f, 0.5f);

            playerSlotTexts[i].fontStyle = TMPro.FontStyles.Italic;



            if (playerSlotImages.Length > i && playerSlotImages[i] != null)

            {

                playerSlotImages[i].sprite = placeholderAvatar;

                playerSlotImages[i].color = new Color(1f, 1f, 1f, 0.4f);

                playerSlotImages[i].gameObject.SetActive(true);

            }

        }



        for (int i = 0; i < activePlayers.Count; i++)

        {

            if (i >= playerSlotTexts.Length) break;



            playerSlotTexts[i].text = activePlayers[i].playerName;

            playerSlotTexts[i].color = Color.white;

            playerSlotTexts[i].fontStyle = TMPro.FontStyles.Bold;



            if (playerSlotImages.Length > i && playerSlotImages[i] != null)

            {

                int selecionadoId = activePlayers[i].avatarId;



                if (selecionadoId >= 0 && selecionadoId < availableAvatars.Length)

                {

                    playerSlotImages[i].sprite = availableAvatars[selecionadoId];

                    playerSlotImages[i].color = Color.white;

                    playerSlotImages[i].gameObject.SetActive(true);

                }

            }

        }

    }



    private void Start()

    {

        AudioManager.Instance.PlayMusic("IntroMusic");

        ShowMainMenu();

    }



    public void ShowMainMenu()

    {

        mainMenuPanel.SetActive(true);

        miniGamesPanel.SetActive(false);

        lobbyPanel.SetActive(false);

        if (splitRoomPanel != null) splitRoomPanel.SetActive(false);

        if (jogoCerteiroPanel != null) jogoCerteiroPanel.SetActive(false); // NOVO

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

    }



    public void ShowMiniGamesPanel()

    {

        mainMenuPanel.SetActive(false);

        miniGamesPanel.SetActive(true);

        lobbyPanel.SetActive(false);

        if (splitRoomPanel != null) splitRoomPanel.SetActive(false);

        if (jogoCerteiroPanel != null) jogoCerteiroPanel.SetActive(false); // NOVO

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

    }



    public void QuitGame()

    {

        Debug.Log("A fechar o jogo...");

#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;

#else

        Application.Quit();

#endif

    }



    // ATUALIZADO: Agora guardamos qual é o jogo que o Host escolheu!

    public void RequestGameStart(string gameID)

    {

        currentSelectedGame = gameID; // Guarda o ID do jogo (ex: "SplitRoom" ou "JogoCerteiro")

        NetworkManager.Instance.RequestRoomCreation(gameID);

    }



    public void StartMiniGame(string roomCode)

    {

        mainMenuPanel.SetActive(false);

        miniGamesPanel.SetActive(false);

        lobbyPanel.SetActive(true);

        if (splitRoomPanel != null) splitRoomPanel.SetActive(false);

        if (jogoCerteiroPanel != null) jogoCerteiroPanel.SetActive(false); // NOVO

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);



        roomCodeText.text = roomCode;

        RefreshPlayersUI();

    }



    // ATUALIZADO: O controlador de tráfego que envia os jogadores para o painel certo!

    public void StartTheGame()

    {

        if (activePlayers.Count < 2)

        {

            Debug.LogWarning("Precisas de pelo menos 2 jogadores para jogar!");

            return;

        }



        Debug.Log($"A transitar do Lobby para o Minijogo: {currentSelectedGame}...");



        NetworkManager.Instance.StartGameLoop();

        lobbyPanel.SetActive(false);



        // Verifica qual foi o jogo selecionado e abre o caminho certo

        if (currentSelectedGame == "JogoCerteiro") // Usa exatamente o nome que passas no botão!

        {

            if (jogoCerteiroPanel != null) jogoCerteiroPanel.SetActive(true);

            jogoCerteiroManager.InitializeGame(activePlayers, this);

        }

        else // O comportamento padrão é ir para o Split the Room

        {

            if (splitRoomPanel != null) splitRoomPanel.SetActive(true);

            splitRoomManager.InitializeGame(activePlayers, this);

        }

    }



    // ATUALIZADO: Entrega as respostas dos telemóveis ao painel que estiver ativo

    public void HandlePlayerAction(ActionResponse actionData)

    {

        if (splitRoomPanel != null && splitRoomPanel.activeInHierarchy)

        {

            splitRoomManager.ProcessAction(actionData);

        }

        else if (jogoCerteiroPanel != null && jogoCerteiroPanel.activeInHierarchy) // NOVO

        {

            jogoCerteiroManager.ProcessAction(actionData);

        }

    }



    public void EndMinigame(IMinigame finishedMinigame)

    {

        Debug.Log("Minigame finished! Processing global scores...");



        Dictionary<string, int> earnedPoints = finishedMinigame.CalculateRoundScores();



        foreach (var player in activePlayers)

        {

            if (earnedPoints.ContainsKey(player.playerName))

            {

                player.globalScore += earnedPoints[player.playerName];

            }

        }



        var sortedPlayers = activePlayers.OrderByDescending(p => p.globalScore).ToList();



        string scoreText = "";

        for (int i = 0; i < sortedPlayers.Count; i++)

        {

            scoreText += $"{i + 1}º - {sortedPlayers[i].playerName}: {sortedPlayers[i].globalScore} pts\n\n";

        }



        leaderboardListText.text = scoreText;



        if (splitRoomPanel != null) splitRoomPanel.SetActive(false);

        if (jogoCerteiroPanel != null) jogoCerteiroPanel.SetActive(false); // NOVO



        leaderboardPanel.SetActive(true);

        NetworkManager.Instance.BroadcastToPhones("SHOW_LEADERBOARD", null);

    }



    public void ReturnToLobbyFromLeaderboard()

    {

        AudioManager.Instance.PlayMusic("IntroMusic");

        leaderboardPanel.SetActive(false);

        lobbyPanel.SetActive(true);

        NetworkManager.Instance.BroadcastToPhones("RETURN_TO_LOBBY", null);

    }



    public void LeaveLobby()

    {

        Debug.Log("A sair do Lobby e a fechar a sala...");

        AudioManager.Instance.PlayMusic("IntroMusic");

        NetworkManager.Instance.CloseRoom();



        activePlayers.Clear();

        RefreshPlayersUI();



        lobbyPanel.SetActive(false);

        miniGamesPanel.SetActive(true);

    }

}

