using UnityEngine;
using System;
using SocketIOClient; // O namespace correto que o Unity agora reconhece

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [Header("Dependencies")]
    public MenuManager menuManager;

    [Header("Server Connection")]
    // Quando fores testar com o teu colega noutra máquina, mudas o localhost para o IP dele
    public string serverURL = "http://localhost:3000";

    private SocketIOUnity socket;

    // Variáveis de controlo para a Thread Principal (Main Thread) do Unity
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
        var uri = new Uri(serverURL);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        SetupSocketListeners();

        // NOVA LINHA: Liga ao servidor assim que o jogo arranca!
        Debug.Log("A iniciar ligação ao Servidor em background...");
        socket.Connect();
    }

    private void ConnectToServer()
    {
        // Usa a propriedade 'Connected' e a função síncrona 'Connect()'
        if (!socket.Connected)
        {
            Debug.Log("A ligar ao Servidor Node.js em: " + serverURL);
            socket.Connect();
        }
    }

    private void SetupSocketListeners()
    {
        // Evento nativo: Dispara quando o Unity consegue estabelecer ligação
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Ligação estabelecida com o servidor!");
        };

        // 1. Escutar o evento "ROOM_CREATED" enviado pelo servidor
        socket.On("ROOM_CREATED", (response) =>
        {
            RoomCreatedResponse data = response.GetValue<RoomCreatedResponse>();

            Debug.Log("O Unity recebeu do servidor o código: " + data.roomCode);

            newRoomCode = data.roomCode;
            roomCodeReceived = true; // Avisa o Update que já temos o código
        });

        // 2. Escutar o evento "PLAYER_JOINED" (Quando um telemóvel entra)
        socket.On("PLAYER_JOINED", (response) =>
        {
            PlayerResponse data = response.GetValue<PlayerResponse>();
            Debug.Log($"O jogador [{data.name}] acabou de entrar no Lobby!");
        });

        // 3. Escutar o evento "ACTION_RECEIVED" (Quando um telemóvel joga)
        socket.On("ACTION_RECEIVED", (response) =>
        {
            ActionResponse data = response.GetValue<ActionResponse>();
            Debug.Log($"Ação recebida de {data.playerName}: {data.action}");
        });

        // 4. Escutar o evento "PLAYER_LEFT" (Quando um jogador sai ou perde a rede)
        socket.On("PLAYER_LEFT", (response) =>
        {
            PlayerResponse data = response.GetValue<PlayerResponse>();
            Debug.Log($"O jogador [{data.name}] saiu do jogo.");
        });
    }

    /// <summary>
    /// Função chamada pelo MenuManager quando clicas no botão do minijogo
    /// </summary>
    public void RequestRoomCreation(string minigameType)
    {
        // Verifica se o tubo está realmente aberto antes de enviar
        if (socket != null && socket.Connected)
        {
            Debug.Log("A pedir ao servidor para criar uma sala para: " + minigameType);
            var payload = new { gameType = minigameType };
            socket.Emit("CREATE_ROOM", payload);
        }
        else
        {
            Debug.LogError("O Unity ainda não conectou ao servidor! Espera um segundo.");
        }
    }

    private void Update()
    {
        // Como o Socket.IO corre em background, precisamos do Update para mudar de ecrã na Main Thread
        if (roomCodeReceived)
        {
            roomCodeReceived = false;
            menuManager.StartMiniGame(newRoomCode);
        }
    }

    private void OnDestroy()
    {
        CleanupSocket();
    }

    private void OnApplicationQuit()
    {
        CleanupSocket();
    }

    private void CleanupSocket()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket.Dispose(); // Isto assassina a thread de background definitivamente
            socket = null;
        }
    }
}

// ============================================================================
// CLASSES AUXILIARES (DTOs)
// Estas classes servem de molde para o Unity conseguir ler os JSONs do Node.js
// Os nomes das variáveis têm de ser IGUAIS aos que o servidor usa.
// ============================================================================

[Serializable]
public class RoomCreatedResponse
{
    public string roomCode { get; set; }
    public string gameType { get; set; }
}

[Serializable]
public class PlayerResponse
{
    public string name { get; set; }
}

[Serializable]
public class ActionResponse
{
    public string playerName { get; set; }
    public string action { get; set; }
}