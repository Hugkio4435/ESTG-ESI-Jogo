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
        // Configura a ligação com o servidor
        var uri = new Uri(serverURL);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        // Configurar as funções que vão ouvir o servidor
        SetupSocketListeners();
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
        // Garante que o socket está ligado antes de pedir a sala
        ConnectToServer();

        Debug.Log("A pedir ao servidor para criar uma sala para: " + minigameType);

        // Criar o objeto anónimo exatamente com o formato que o Node.js espera
        var payload = new { gameType = minigameType };

        // Usa 'Emit' síncrono para enviar o evento "CREATE_ROOM"
        socket.Emit("CREATE_ROOM", payload);
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
        if (socket != null)
        {
            // Usa 'Disconnect' síncrono quando o jogo é fechado
            socket.Disconnect();
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
    public string roomCode;
    public string gameType;
}

[Serializable]
public class PlayerResponse
{
    public string name;
}

[Serializable]
public class ActionResponse
{
    public string playerName;
    public string action;
}