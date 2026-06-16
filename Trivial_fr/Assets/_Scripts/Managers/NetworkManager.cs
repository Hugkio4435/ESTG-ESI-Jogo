using UnityEngine;

using System;

using System.Collections.Generic;

using SocketIOClient;



public class NetworkManager : MonoBehaviour

{

    // Filas para garantir que as atualizações de rede correm na Main Thread do Unity

    private Queue<PlayerResponse> playersToSpawn = new Queue<PlayerResponse>();

    private Queue<string> playersToRemove = new Queue<string>();

    private Queue<ActionResponse> actionsToProcess = new Queue<ActionResponse>();



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



        Debug.Log("A iniciar ligacao ao Servidor em background...");

        socket.Connect();

    }



    private void SetupSocketListeners()

    {

        socket.OnConnected += (sender, e) =>

        {

            Debug.Log("Ligacao estabelecida com o servidor!");

        };



        socket.On("ROOM_CREATED", (response) =>

        {

            Debug.Log("O Unity recebeu uma resposta do servidor!");

            try

            {

                // 1. Lemos os dados em bruto para a consola para vermos o que chegou

                Debug.Log("Pacote bruto recebido: " + response.ToString());



                // 2. Descodificamos de forma segura

                RoomCreatedResponse data = response.GetValue<RoomCreatedResponse>();



                if (data != null && !string.IsNullOrEmpty(data.roomCode))

                {

                    Debug.Log("Codigo da sala lido com sucesso: " + data.roomCode);

                    newRoomCode = data.roomCode;

                    roomCodeReceived = true;

                }

                else

                {

                    Debug.LogError("O Unity recebeu a mensagem, mas a variável roomCode veio vazia. Problema no JSON!");

                }

            }

            catch (System.Exception ex)

            {

                Debug.LogError("Erro critico ao ler o pacote do servidor: " + ex.Message);

            }

        });



        socket.On("PLAYER_JOINED", (response) =>

        {

            PlayerResponse data = response.GetValue<PlayerResponse>();

            Debug.Log($"O jogador [{data.name}] entrou com o Avatar {data.avatarId}!");



            lock (playersToSpawn)

            {

                playersToSpawn.Enqueue(data);

            }

        });



        // ATUALIZADO: Agora lê a nova estrutura de ações (texto e votos)

        socket.On("ACTION_RECEIVED", (response) =>

        {

            ActionResponse data = response.GetValue<ActionResponse>();



            lock (actionsToProcess)

            {

                actionsToProcess.Enqueue(data);

            }

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



            // Dicionário garante um JSON perfeitamente formatado

            var payload = new Dictionary<string, string> { { "gameType", minigameType } };

            socket.Emit("CREATE_ROOM", payload);

        }

    }



    public void CloseRoom()

    {

        if (socket != null && socket.Connected)

        {

            socket.Emit("CLOSE_ROOM");

            roomCodeReceived = false;

        }

    }



    // NOVA FUNÇÃO: Ordena aos telemóveis para saírem do Lobby

    public void StartGameLoop()

    {

        if (socket != null && socket.Connected)

        {

            socket.Emit("START_GAME");

        }

    }



    // NOVA FUNÇÃO: O "Megafone" do Unity para mudar interfaces nos telemóveis

    public void BroadcastToPhones(string eventName, object payloadData)

    {

        if (socket != null && socket.Connected)

        {

            var envelope = new { @event = eventName, payload = payloadData };

            socket.Emit("HOST_BROADCAST", envelope);

        }

    }



    private void Update()

    {

        if (roomCodeReceived)

        {

            roomCodeReceived = false;

            menuManager.StartMiniGame(newRoomCode);

        }



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



        // NOVO: Processa as ações recebidas e envia-as para o MenuManager

        if (actionsToProcess.Count > 0)

        {

            lock (actionsToProcess)

            {

                ActionResponse newAction = actionsToProcess.Dequeue();

                menuManager.HandlePlayerAction(newAction);

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

// CLASSES AUXILIARES (DTOs) - Sem { get; set; } para evitar erros de leitura

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

    public int avatarId { get; set; }

}



[Serializable]

public class ActionResponse

{

    public string playerName { get; set; }

    public string actionType { get; set; }

    public string word { get; set; }

    public bool voteValue { get; set; }

    public int triviaAnswerIndex { get; set; }

}

