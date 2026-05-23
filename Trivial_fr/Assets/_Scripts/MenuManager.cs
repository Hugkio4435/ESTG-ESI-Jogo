using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [Header("Os Nossos Painéis")]
    public GameObject mainMenuPanel;
    public GameObject miniGamesPanel;
    public GameObject lobbyPanel;


    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI roomCodeText; 

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
        // Enviamos esse ID específico para o NetworkManager
        NetworkManager.Instance.RequestRoomCreation(gameID);
    }

    // 4. Função chamada pelo NetworkManager QUANDO o servidor responde
    public void StartMiniGame(string roomCode)
    {
        mainMenuPanel.SetActive(false);
        miniGamesPanel.SetActive(false);


        lobbyPanel.SetActive(true);
        // Atualiza o texto na UI para mostrar o código real
        roomCodeText.text =  roomCode;

    }









}