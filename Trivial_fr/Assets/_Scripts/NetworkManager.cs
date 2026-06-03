using UnityEngine;
using System;
using SocketIOClient;

public class NetworkManager : MonoBehaviour
{
    // ATUALIZADO: Agora a fila guarda o pacote inteiro (PlayerResponse) e não apenas uma string
    private System.Collections.Generic.Queue<PlayerResponse> playersToSpawn = new System.Collections.Generic.Queue<PlayerResponse>();
    private System.Collections.Generic.Queue<string> playersToRemove = new System.Collections.Generic.Queue<string>();

    public static NetworkManager Instance;

    [Header("Dependencies")]
    public MenuManager menuManager;

    [Header("Server Connection")]
    public string serverURL = "http://localhost:3000";

    private SocketIOUnity socket;

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

        Debug.Log("A iniciar ligação ao Servidor em background...");
        socket.Connect();
    }

    private void ConnectToServer()
    {
        if (!socket.Connected)
        {
            Debug.Log("A ligar ao Servidor Node.js em: " + serverURL);
            socket.Connect();
        }
    }

    private void SetupSocketListeners()
    {
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Ligação estabelecida com o servidor!");
        };

        socket.On("ROOM_CREATED", (response) =>
        {
            RoomCreatedResponse data = response.GetValue<RoomCreatedResponse>();
            Debug.Log("O Unity recebeu do servidor o código: " + data.roomCode);
            newRoomCode = data.roomCode;
            roomCodeReceived = true;
        });

        socket.On("PLAYER_JOINED", (response) =>
        {
            PlayerResponse data = response.GetValue<PlayerResponse>();
            Debug.Log($"O jogador [{data.name}] entrou com o Avatar {data.avatarId}!");

            // Coloca o PACOTE INTEIRO na fila
            lock (playersToSpawn)
            {
                playersToSpawn.Enqueue(data);
            }
        });

        socket.On("ACTION_RECEIVED", (response) =>
        {
            ActionResponse data = response.GetValue<ActionResponse>();
            Debug.Log($"Ação recebida de {data.playerName}: {data.action}");
        });

        socket.On("PLAYER_LEFT", (response) =>
        {
            PlayerResponse data = response.GetValue<PlayerResponse>();
            Debug.Log($"O jogador [{data.name}] saiu do jogo.");

            lock (playersToRemove)
            {
                playersToRemove.Enqueue(data.name);
            }
        });
    }

    public void RequestRoomCreation(string minigameType)
    {
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
        if (roomCodeReceived)
        {
            roomCodeReceived = false;
            menuManager.StartMiniGame(newRoomCode);
        }

        // CORRIGIDO: Retira o pacote inteiro da fila e passa o Nome e o AvatarID para o MenuManager
        if (playersToSpawn.Count > 0)
        {
            lock (playersToSpawn)
            {
                PlayerResponse newPlayer = playersToSpawn.Dequeue();
                menuManager.AddPlayerToUI(newPlayer.name, newPlayer.avatarId);
            }
        }

        if (playersToRemove.Count > 0)
        {
            lock (playersToRemove)
            {
                string playerLeft = playersToRemove.Dequeue();
                menuManager.RemovePlayerFromUI(playerLeft);
            }
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
            socket.Dispose();
            socket = null;
        }
    }
}

// ============================================================================
// CLASSES AUXILIARES (DTOs)
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
    public int avatarId { get; set; } // ATUALIZADO PARA LER O AVATAR ID
}

[Serializable]
public class ActionResponse
{
    public string playerName { get; set; }
    public string action { get; set; }
}