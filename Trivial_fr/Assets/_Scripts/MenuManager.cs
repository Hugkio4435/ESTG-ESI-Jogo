using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Os Nossos Painéis")]
    public GameObject mainMenuPanel;
    public GameObject miniGamesPanel;
    public GameObject lobbyPanel; 

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

    
    public void StartMiniGame1()
    {
        // Mais tarde, é AQUI que vamos dizer ao Node.js: "Cria a sala para o Jogo 1!"

        mainMenuPanel.SetActive(false);
        miniGamesPanel.SetActive(false);
        lobbyPanel.SetActive(true);
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




}