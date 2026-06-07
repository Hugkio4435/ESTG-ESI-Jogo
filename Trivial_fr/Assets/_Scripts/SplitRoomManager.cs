using UnityEngine;
using System.Collections.Generic;
using TMPro;

// Estrutura para guardar os dados de cada ronda individual
public class ScenarioData
{
    public string authorName;
    public string originalPrompt;
    public string completedPrompt;
    public int yesVotes = 0;
    public int noVotes = 0;
}

public class SplitRoomManager : MonoBehaviour
{
    public enum GamePhase
    {
        WaitingToStart,
        GlobalWritingPhase,
        VotingPhase,
        RevealPhase
    }

    [Header("Estado do Jogo")]
    public GamePhase currentPhase = GamePhase.WaitingToStart;
    private List<ConnectedPlayer> playersInGame;

    // Lista de cenários gerados nesta partida
    private List<ScenarioData> currentScenarios = new List<ScenarioData>();
    private int currentScenarioIndex = 0;

    [Header("Elementos de UI (Ecra da TV)")]
    public TextMeshProUGUI mainDisplayText;
    public TextMeshProUGUI statusText;

    [Header("Base de Dados de Frases")]
    private List<string> prompts = new List<string>()
    {
        "Recebes um milhao de euros, mas tens de viver o resto da vida com [BLANK]. Aceitas?",
        "Podes ter o superpoder de voar, mas sempre que aterrares tens de [BLANK]. Aceitas?",
        "Es o melhor programador do mundo, mas o teu teclado so tem a tecla [BLANK]. Aceitas?",
        "A tua comida favorita passa a ter o sabor de [BLANK]. Aceitas?"
    };

    public void InitializeGame(List<ConnectedPlayer> playersFromLobby)
    {
        playersInGame = playersFromLobby;
        currentScenarios.Clear();
        currentScenarioIndex = 0;

        StartGlobalWritingPhase();
    }

    private void StartGlobalWritingPhase()
    {
        currentPhase = GamePhase.GlobalWritingPhase;

        // 1. Criar um dicionário para enviar a cada telemóvel a sua frase específica
        Dictionary<string, string> playerPrompts = new Dictionary<string, string>();

        foreach (var player in playersInGame)
        {
            int randomIndex = Random.Range(0, prompts.Count);
            string assignedPrompt = prompts[randomIndex];

            playerPrompts.Add(player.playerName, assignedPrompt);

            // 2. Preparamos logo as cápsulas em branco para preencher mais tarde
            currentScenarios.Add(new ScenarioData
            {
                authorName = player.playerName,
                originalPrompt = assignedPrompt
            });
        }

        // 3. UI do Unity
        mainDisplayText.text = "FASE DE ESCRITA";
        statusText.text = $"Cenarios recebidos: 0 / {playersInGame.Count}";

        // 4. Envia as frases em massa para os telemóveis
        var payload = new { prompts = playerPrompts };
        NetworkManager.Instance.BroadcastToPhones("START_GLOBAL_WRITING", payload);
    }

    // A porta de entrada das ações vinda do MenuManager
    public void ProcessAction(ActionResponse data)
    {
        if (currentPhase == GamePhase.GlobalWritingPhase && data.actionType == "SUBMIT_WORD")
        {
            RegisterCompletedWord(data.playerName, data.word);
        }
        else if (currentPhase == GamePhase.VotingPhase && data.actionType == "SUBMIT_VOTE")
        {
            RegisterVote(data.voteValue);
        }
    }

    private void RegisterCompletedWord(string author, string word)
    {
        // Encontra o cenário correspondente a quem enviou e regista a frase completa
        var scenario = currentScenarios.Find(s => s.authorName == author);
        if (scenario != null && string.IsNullOrEmpty(scenario.completedPrompt))
        {
            scenario.completedPrompt = scenario.originalPrompt.Replace("[BLANK]", $"<color=#F1C40F>{word.ToUpper()}</color>");
        }

        // Conta quantos já enviaram
        int receivedCount = currentScenarios.FindAll(s => !string.IsNullOrEmpty(s.completedPrompt)).Count;
        statusText.text = $"Cenarios recebidos: {receivedCount} / {playersInGame.Count}";

        // Se todos já escreveram, avança para a votação
        if (receivedCount >= playersInGame.Count)
        {
            StartVotingPhase();
        }
    }

    private void StartVotingPhase()
    {
        currentPhase = GamePhase.VotingPhase;

        ScenarioData currentScenario = currentScenarios[currentScenarioIndex];

        // UI do Unity: Mostra o cenário ANONIMAMENTE
        mainDisplayText.text = currentScenario.completedPrompt;
        int expectedVotes = playersInGame.Count - 1; // O autor não vota
        statusText.text = $"VOTOS REGISTADOS: 0 / {expectedVotes}";

        // Envia ordem de votação. Dizemos quem é o autor para o telemóvel dele se bloquear.
        var payload = new { authorToIgnore = currentScenario.authorName };
        NetworkManager.Instance.BroadcastToPhones("START_VOTING_PHASE", payload);
    }

    private void RegisterVote(bool votedYes)
    {
        ScenarioData currentScenario = currentScenarios[currentScenarioIndex];

        if (votedYes) currentScenario.yesVotes++;
        else currentScenario.noVotes++;

        int totalVotesCast = currentScenario.yesVotes + currentScenario.noVotes;
        int expectedVotes = playersInGame.Count - 1;

        statusText.text = $"VOTOS REGISTADOS: {totalVotesCast} / {expectedVotes}";

        if (totalVotesCast >= expectedVotes)
        {
            StartRevealPhase();
        }
    }

    private void StartRevealPhase()
    {
        currentPhase = GamePhase.RevealPhase;
        ScenarioData currentScenario = currentScenarios[currentScenarioIndex];

        // Revela o autor e os resultados
        mainDisplayText.text = $"Autor: <color=#2ECC71>{currentScenario.authorName}</color>\n\n{currentScenario.completedPrompt}";
        statusText.text = $"RESULTADOS DA DIVISAO\nSim: {currentScenario.yesVotes} | Nao: {currentScenario.noVotes}";

        currentScenarioIndex++;

        // Espera 7 segundos no ecrã de revelação antes de avançar
        if (currentScenarioIndex < currentScenarios.Count)
        {
            Invoke("StartVotingPhase", 7f);
        }
        else
        {
            Invoke("EndGame", 7f);
        }
    }

    private void EndGame()
    {
        mainDisplayText.text = "FIM DO JOGO!";
        statusText.text = "Todos os cenarios foram avaliados.";
    }
}