using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    
    public static NetworkManager Instance;

    [Header("Dependencies")]
    public MenuManager menuManager; 

    
    private bool roomCodeReceived = false;
    private string newRoomCode = "";

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
       
    }

    private void ConnectToServer()
    {
        Debug.Log("Connecting to Node.js Server...");

        // AQUI ENTRA O CÓDIGO DO TEU PLUGIN DE SOCKET.IO
        // Exemplo genérico de como costuma ser:
        // socket.Connect("ws://localhost:3000");
        //
        // socket.On("ROOM_CREATED", (response) => {
        //      // We CANNOT change the UI here directly! We just save the data.
        //      newRoomCode = response.roomCode;
        //      roomCodeReceived = true;
        // });
    }

    /// <summary>
    /// Called by MenuManager when a minigame button is clicked
    /// </summary>
    public void RequestRoomCreation(string minigameType)
    {

        ConnectToServer();

        Debug.Log("Asking Server to create a room for: " + minigameType);

        // AQUI ENVIAS O PEDIDO PARA O SERVIDOR:
        // socket.Emit("CREATE_ROOM", "{ \"gameType\": \"" + minigameType + "\" }");

        // --- PARA TESTAR HOJE (SEM O SERVIDOR PRONTO) ---
        // Simular que o servidor respondeu passado 1 segundo:
        Invoke(nameof(SimulateServerResponse), 1f);
    }

    // Função falsa para teste
    private void SimulateServerResponse()
    {
        newRoomCode = "ABCD";
        roomCodeReceived = true;
    }

    private void Update()
    {
        // The Update loop runs on the Main Thread. It checks if the background thread received a code.
        if (roomCodeReceived)
        {
            roomCodeReceived = false;

            // Now it's safe to tell the MenuManager to change the screen!
            menuManager.StartMiniGame(newRoomCode);
        }
    }
}