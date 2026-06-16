using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ConnectedPlayer
{
    public string playerName;
    public int avatarId;
}

public class MenuManager : MonoBehaviour
{
    [Header("Os Nossos Paineis")]
    public GameObject mainMenuPanel;
    public GameObject miniGamesPanel;
    public GameObject lobbyPanel;
    public GameObject splitRoomPanel; // NOVO: O painel do minijogo

    [Header("Gestores de Minijogos")]
    public SplitRoomManager splitRoomManager;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI roomCodeText;

    [Header("Lobby UI - Grelha de Jogadores")]
    public TMPro.TextMeshProUGUI[] playerSlotTexts;
    public Image[] playerSlotImages;

    [Header("A Tua Colecao de Avatares")]
    public Sprite[] availableAvatars;
    public Sprite placeholderAvatar;

    private List<ConnectedPlayer> activePlayers = new List<ConnectedPlayer>();

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
            playerSlotTexts[i].text = "Lugar vazio";
            playerSlotTexts[i].color = Color.black;
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
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        miniGamesPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        if (splitRoomPanel != null) splitRoomPanel.SetActive(false);
    }

    public void ShowMiniGamesPanel()
    {
        mainMenuPanel.SetActive(false);
        miniGamesPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        if (splitRoomPanel != null) splitRoomPanel.SetActive(false);
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

    public void RequestGameStart(string gameID)
    {
        NetworkManager.Instance.RequestRoomCreation(gameID);
    }

    public void StartMiniGame(string roomCode)
    {
        mainMenuPanel.SetActive(false);
        miniGamesPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        if (splitRoomPanel != null) splitRoomPanel.SetActive(false);

        roomCodeText.text = roomCode;
        RefreshPlayersUI();
    }

    public void StartTheGame()
    {
        if (activePlayers.Count < 2)
        {
            Debug.LogWarning("Precisas de pelo menos 2 jogadores para jogar!");
            return;
        }

        Debug.Log("A transitar do Lobby para o Minijogo...");

        // 1. Avisa os telemóveis para saírem do modo Lobby
        NetworkManager.Instance.StartGameLoop();

        // 2. Transita a interface no Unity
        lobbyPanel.SetActive(false);
        if (splitRoomPanel != null) splitRoomPanel.SetActive(true);

        // 3. Inicia a lógica no motor do jogo
        splitRoomManager.InitializeGame(activePlayers);
    }

    // NOVA FUNÇÃO: O NetworkManager entrega aqui as ações, e nós passamos para o jogo ativo
    public void HandlePlayerAction(ActionResponse actionData)
    {
        // Se o painel do minijogo estiver ligado, enviamos a ação para o respetivo manager
        if (splitRoomPanel != null && splitRoomPanel.activeInHierarchy)
        {
            splitRoomManager.ProcessAction(actionData);
        }
    }



    public void LeaveLobby()
    {
        Debug.Log("A sair do Lobby e a fechar a sala...");

        // 1. Avisa o servidor para fechar a sala
        NetworkManager.Instance.CloseRoom();

        // 2. Limpa os jogadores antigos para que o próximo lobby comece do zero
        activePlayers.Clear();
        RefreshPlayersUI();

        // 3. Troca a interface visual
        lobbyPanel.SetActive(false);
        miniGamesPanel.SetActive(true);
    }





}