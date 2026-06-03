using UnityEngine;
using UnityEngine.UI; // Necessário para gerir os componentes de Imagem da UI
using System.Collections.Generic;
using System.Linq; // Necessário para podermos usar o .Any() e o .FirstOrDefault()

// Cápsula de dados para associar o nome do jogador ao ID do seu avatar
public class ConnectedPlayer
{
    public string playerName;
    public int avatarId;
}

public class MenuManager : MonoBehaviour
{
    [Header("Os Nossos Painéis")]
    public GameObject mainMenuPanel;
    public GameObject miniGamesPanel;
    public GameObject lobbyPanel;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI roomCodeText;

    [Header("Lobby UI - Grelha de Jogadores")]
    public TMPro.TextMeshProUGUI[] playerSlotTexts; // Array com os 8 textos dos slots
    public Image[] playerSlotImages;                // Array com as 8 imagens dos slots

    [Header("A Tua Coleção de Avatares")]
    public Sprite[] availableAvatars;               // Array com as 10 imagens reais (Sprites) no Unity
    public Sprite placeholderAvatar;                // Nova variável para a imagem predefinida (ex: silhueta)

    // Lista dinâmica que guarda os jogadores atualmente conectados na sala
    private List<ConnectedPlayer> activePlayers = new List<ConnectedPlayer>();

    public void AddPlayerToUI(string playerName, int avatarId)
    {
        // Só adiciona o jogador se ele ainda não estiver na lista (evita duplicados)
        if (!activePlayers.Any(p => p.playerName == playerName))
        {
            activePlayers.Add(new ConnectedPlayer { playerName = playerName, avatarId = avatarId });
            RefreshPlayersUI();
        }
    }

    public void RemovePlayerFromUI(string playerName)
    {
        // Procura o jogador pelo nome e remove-o da lista
        var playerToRemove = activePlayers.FirstOrDefault(p => p.playerName == playerName);
        if (playerToRemove != null)
        {
            activePlayers.Remove(playerToRemove);
            RefreshPlayersUI();
        }
    }

    // Função que limpa a UI e redesenha todos os slots de acordo com a lista ativa
    private void RefreshPlayersUI()
    {
        // 1. Reset completo: Coloca todos os 8 slots em modo de espera e define a imagem predefinida
        for (int i = 0; i < playerSlotTexts.Length; i++)
        {
            playerSlotTexts[i].text = "Waiting for user...";
            playerSlotTexts[i].color = new Color(0.6f, 0.6f, 0.6f); // Cor cinzenta
            playerSlotTexts[i].fontStyle = TMPro.FontStyles.Italic;

            if (playerSlotImages.Length > i && playerSlotImages[i] != null)
            {
                playerSlotImages[i].sprite = placeholderAvatar; // Define o avatar predefinido
                playerSlotImages[i].color = new Color(1f, 1f, 1f, 0.4f); // Deixa a imagem semi-transparente (efeito de vazio)
                playerSlotImages[i].gameObject.SetActive(true); // Garante que a imagem fica visível
            }
        }

        // 2. Preenchimento: Atualiza os slots com os dados dos jogadores que estão na sala
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (i >= playerSlotTexts.Length) break; // Proteção para não estourar o limite de 8 slots

            // Atualiza o texto do nome
            playerSlotTexts[i].text = activePlayers[i].playerName;
            playerSlotTexts[i].color = Color.white; // Cor branca para jogador ativo
            playerSlotTexts[i].fontStyle = TMPro.FontStyles.Bold;

            // Atualiza a imagem do Avatar
            if (playerSlotImages.Length > i && playerSlotImages[i] != null)
            {
                int selecionadoId = activePlayers[i].avatarId;

                // Garante que o ID enviado pelo telemóvel existe no array de Sprites do Unity
                if (selecionadoId >= 0 && selecionadoId < availableAvatars.Length)
                {
                    playerSlotImages[i].sprite = availableAvatars[selecionadoId];
                    playerSlotImages[i].color = Color.white; // Restaura a opacidade total (100% visível)
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
    }

    public void ShowMiniGamesPanel()
    {
        mainMenuPanel.SetActive(false);
        miniGamesPanel.SetActive(true);
        lobbyPanel.SetActive(false);
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

        roomCodeText.text = roomCode;

        // Garante que o Lobby começa limpo e no estado correto de espera
        RefreshPlayersUI();
    }
}