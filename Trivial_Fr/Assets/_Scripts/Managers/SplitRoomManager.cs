using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioData
{
    public string authorName;
    public string originalPrompt;
    public string completedPrompt;

    public List<ConnectedPlayer> yesVoters = new List<ConnectedPlayer>();
    public List<ConnectedPlayer> noVoters = new List<ConnectedPlayer>();

    public float timeTakenForVoting = 0f;
}

public class SplitRoomManager : MonoBehaviour, IMinigame
{
    public enum GamePhase
    {
        WaitingToStart,
        TutorialPhase,
        GlobalWritingPhase,
        VotingPhase,
        RevealPhase,
        LeaderboardPhase
    }

    [Header("Estado do Jogo")]
    public GamePhase currentPhase = GamePhase.WaitingToStart;
    private List<ConnectedPlayer> playersInGame;
    private MenuManager menuManager;

    [Header("Definicoes de Rondas")]
    public int totalRounds = 3;
    private int currentRound = 1;

    [Header("Temporizador e Pontuacao")]
    public TextMeshProUGUI timerText;
    public float writingTimeLimit = 60f;
    public float votingTimeLimit = 20f;
    public float maxTimeBonus = 500f;

    private float currentTime;
    private bool isTimerRunning = false;

    private List<ScenarioData> currentScenarios = new List<ScenarioData>();
    private int currentScenarioIndex = 0;

    private Dictionary<string, int> localRoundScores = new Dictionary<string, int>();

    [Header("Elementos de UI (Textos)")]
    public TextMeshProUGUI mainDisplayText;
    public TextMeshProUGUI statusText;

    [Header("UI dos Avatares")]
    public Transform yesAvatarContainer;
    public Transform noAvatarContainer;
    public GameObject avatarPrefab;
    public Sprite[] avatarSprites;

    [Header("Tutorial e Graficos")]
    public GameObject tutorialPanel;
    public GameObject revealUIPanel;
    private bool tutorialSkipped = false;

    [Header("Leaderboard Final")]
    public GameObject leaderboardPanel;
    public Transform leaderboardContainer;
    public GameObject leaderboardRowPrefab;

    [Header("Base de Dados de Frases")]
    private List<string> prompts = new List<string>()
    {
        "Tu recebes 2.000€ em dinheiro todas as semanas para o resto da tua vida, mas sob uma condição, tu precisas de fazer uma tatuagem permanente na tua cara onde diz [BLANK]. Ela tem de cobrir a tua testa. Tu fazes a tatuagem?",
        "Recebes um milhao de euros, mas tens de viver o resto da vida com [BLANK]. Aceitas?",
        "Podes ter o superpoder de voar, mas sempre que aterrares tens de [BLANK]. Aceitas?",
        "Tu estás num pequeno estabelecimento com uma jukebox mágica. A música que está a tocar prevê o futuro imediato na área ao redor. Ela começa a tocar [BLANK]. Tu sais a correr o mais rápido possível?",
        "A tua comida favorita passa a ter o sabor de [BLANK]. Aceitas?",
        "Um candidato bilionário ao governo faz uma promessa audaz na sua campanha. Ele pagará todos os seus impostos, desde que renuncies ao direito de [BLANK].Votas nele?",
        "Adormeces-te acidentalmente durante 100 anos. Quando acordas, descobres que a sociedade é radicalmente diferente. Em vez de um sistema de governo, somos governados por [BLANK]. Voltas a adormecer e vês o que o futuro te reserva?",
        "Foste acusado de um crime que não cometeste. Um advogado profissional entra em contacto contigo para te representar. Esta pessoa é inteligente e tem muita experiência, no entanto, por acaso, é [BLANK]. Permites que esta pessoa te represente?",
        "Estás a lutar numa luta de boxe com um prémio de 10.000€. Estás a um golpe decisivo da vitória quando ouves o seu adversário a sussurrar-te [BLANK] ao ouvido. Vais deixá-lo ganhar?",
        "Vives num mundo onde as pessoas que completam 30 anos são obrigadas a [BLANK] ... ou passar um ano de prisão domiciliária. É o seu último dia com 29 anos. Vais realizar a tarefa amanhã?",
        "Descobres que um símbolo japonês tatuado na parte de trás da tua mão significa [BLANK]. Estás prestes a passar um ano no Japão. Não tens dinheiro para o remover. Cancelas a tua viagem?",
        "Avistas um chupacabra. Queres tirar uma fotografia, mas o seu telemóvel está cheio. Para libertar espaço, precisas de apagar a única foto que tens de [BLANK]. Apagas a imagem?",
        "Um fantasma na tua casa assombra-te, dando-te spoilers dos seus programas de TV e filmes. Para o fazer desaparecer, precisas de esculpir [BLANK] na tua porta da frente. Tu fazes isso?",
        "Enquanto caminhas na praia, encontras uma mensagem numa garrafa. A carta é dirigida a ti e contem apenas uma breve linha: [BLANK]. Tentas encontrar essa pessoa?",
        "Estás sob um feitiço de Halloween que faz com que fiques preso no teu disfarce durante 48 horas. Estás vestido de [BLANK]. Tens uma palestra marcada para amanhã para um grupo de veteranos de guerra. Ainda fazes isto?",
        "Tens 100 anos e estás no teu leito de morte. Graças à nova tecnologia, podes ser recordado como um holograma. Infelizmente, o holograma irá retratar-te a [BLANK]. Ainda aceitas um?",
        "Estás prestes a submeter-te a uma pequena cirurgia cardíaca. Enquanto te levam para a sala de operações, apercebes-te que o teu cirurgião está a [BLANK]. Infelizmente, este é o único médico que aceita o teu seguro de saúde. Interrompes a operação?"
    };

    private List<string> availablePrompts = new List<string>();

    private void Update()
    {
        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(currentTime).ToString();
                if (currentTime <= 10f) timerText.color = Color.red;
                else timerText.color = Color.white;
            }

            if (currentTime <= 0)
            {
                currentTime = 0;
                isTimerRunning = false;
                HandleTimeUp();
            }
        }
    }

    private void StartTimer(float timeLimit)
    {
        currentTime = timeLimit;
        isTimerRunning = true;
        if (timerText != null) timerText.gameObject.SetActive(true);
    }

    private void StopTimer()
    {
        isTimerRunning = false;
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    private void HandleTimeUp()
    {
        if (currentPhase == GamePhase.GlobalWritingPhase)
        {
            foreach (var scenario in currentScenarios)
            {
                if (string.IsNullOrEmpty(scenario.completedPrompt))
                {
                    scenario.completedPrompt = scenario.originalPrompt.Replace("[BLANK]", "<color=#E74C3C>SALSICHAS</color>");
                }
            }
            StartVotingPhase();
        }
        else if (currentPhase == GamePhase.VotingPhase)
        {
            currentScenarios[currentScenarioIndex].timeTakenForVoting = votingTimeLimit;
            StartRevealPhase();
        }
    }

    public Dictionary<string, int> CalculateRoundScores()
    {
        return localRoundScores;
    }

    public void InitializeGame(List<ConnectedPlayer> playersFromLobby, MenuManager manager)
    {
        if (leaderboardContainer != null)
        {
            foreach (Transform child in leaderboardContainer)
            {
                Destroy(child.gameObject);
            }
        }

        AudioManager.Instance.PlayMusic("SplitRoomTheme");

        menuManager = manager;
        playersInGame = playersFromLobby;
        currentScenarios.Clear();
        currentScenarioIndex = 0;
        currentRound = 1;
        localRoundScores.Clear();
        availablePrompts = new List<string>(prompts);

        ClearAvatarsUI();

        foreach (var p in playersInGame)
        {
            localRoundScores.Add(p.playerName, 0);
        }

        StartCoroutine(ShowTutorialCoroutine());
    }

    private IEnumerator ShowTutorialCoroutine()
    {
        currentPhase = GamePhase.TutorialPhase;
        tutorialSkipped = false;

        if (mainDisplayText != null) mainDisplayText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (revealUIPanel != null) revealUIPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        NetworkManager.Instance.BroadcastToPhones("GAME_STARTED", null);

        yield return new WaitUntil(() => tutorialSkipped);

        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (mainDisplayText != null) mainDisplayText.gameObject.SetActive(true);
        if (statusText != null) statusText.gameObject.SetActive(true);

        StartGlobalWritingPhase();
    }

    public void OnTutorialContinueButtonPressed()
    {
        if (currentPhase == GamePhase.TutorialPhase)
        {
            tutorialSkipped = true;
        }
    }

    private void StartGlobalWritingPhase()
    {
        currentPhase = GamePhase.GlobalWritingPhase;
        ClearAvatarsUI();

        if (revealUIPanel != null) revealUIPanel.SetActive(false);

        Dictionary<string, string> playerPrompts = new Dictionary<string, string>();

        foreach (var player in playersInGame)
        {
            if (availablePrompts.Count == 0) availablePrompts = new List<string>(prompts);

            int randomIndex = Random.Range(0, availablePrompts.Count);
            string assignedPrompt = availablePrompts[randomIndex];
            availablePrompts.RemoveAt(randomIndex);

            playerPrompts.Add(player.playerName, assignedPrompt);

            currentScenarios.Add(new ScenarioData
            {
                authorName = player.playerName,
                originalPrompt = assignedPrompt
            });
        }

        mainDisplayText.text = "FASE DE ESCRITA";
        statusText.text = $"Cenarios recebidos: 0 / {playersInGame.Count}";

        var payload = new { prompts = playerPrompts };
        NetworkManager.Instance.BroadcastToPhones("START_GLOBAL_WRITING", payload);

        StartTimer(writingTimeLimit);
    }

    public void ProcessAction(ActionResponse data)
    {
        if (currentPhase == GamePhase.GlobalWritingPhase && data.actionType == "SUBMIT_WORD")
        {
            RegisterCompletedWord(data.playerName, data.word);
        }
        else if (currentPhase == GamePhase.VotingPhase && data.actionType == "SUBMIT_VOTE")
        {
            RegisterVote(data.playerName, data.voteValue);
        }
    }

    private void RegisterCompletedWord(string author, string word)
    {
        var scenario = currentScenarios.Find(s => s.authorName == author);
        if (scenario != null && string.IsNullOrEmpty(scenario.completedPrompt))
        {
            scenario.completedPrompt = scenario.originalPrompt.Replace("[BLANK]", $"<color=#F1C40F>{word.ToUpper()}</color>");
        }

        int receivedCount = currentScenarios.FindAll(s => !string.IsNullOrEmpty(s.completedPrompt)).Count;
        statusText.text = $"Cenarios recebidos: {receivedCount} / {playersInGame.Count}";

        if (receivedCount >= playersInGame.Count)
        {
            StopTimer();
            StartVotingPhase();
        }
    }

    private void StartVotingPhase()
    {
        currentPhase = GamePhase.VotingPhase;
        ClearAvatarsUI();

        if (revealUIPanel != null) revealUIPanel.SetActive(true);

        ScenarioData currentScenario = currentScenarios[currentScenarioIndex];

        mainDisplayText.text = currentScenario.completedPrompt;
        int expectedVotes = playersInGame.Count - 1;
        statusText.text = $"VOTOS REGISTADOS: 0 / {expectedVotes}";

        var payload = new { authorToIgnore = currentScenario.authorName };
        NetworkManager.Instance.BroadcastToPhones("START_VOTING_PHASE", payload);

        StartTimer(votingTimeLimit);
    }

    private void RegisterVote(string voterName, bool votedYes)
    {
        ScenarioData currentScenario = currentScenarios[currentScenarioIndex];
        ConnectedPlayer voter = playersInGame.Find(p => p.playerName == voterName);

        if (voter != null)
        {
            if (votedYes) currentScenario.yesVoters.Add(voter);
            else currentScenario.noVoters.Add(voter);
        }

        int totalVotesCast = currentScenario.yesVoters.Count + currentScenario.noVoters.Count;
        int expectedVotes = playersInGame.Count - 1;

        statusText.text = $"VOTOS REGISTADOS: {totalVotesCast} / {expectedVotes}";

        if (totalVotesCast >= expectedVotes)
        {
            currentScenario.timeTakenForVoting = votingTimeLimit - currentTime;
            StopTimer();
            StartRevealPhase();
        }
    }

    private void StartRevealPhase()
    {
        StopTimer();
        currentPhase = GamePhase.RevealPhase;
        StartCoroutine(SimpleRevealCoroutine());
    }

    private IEnumerator SimpleRevealCoroutine()
    {
        ScenarioData currentScenario = currentScenarios[currentScenarioIndex];

        int yesVotes = currentScenario.yesVoters.Count;
        int noVotes = currentScenario.noVoters.Count;

        mainDisplayText.text = $"Autor: <color=#2ECC71>{currentScenario.authorName}</color>\n\n{currentScenario.completedPrompt}";
        statusText.text = $"RESULTADOS DA DIVISAO\nSim: {yesVotes} | Nao: {noVotes}";

        if (revealUIPanel != null) revealUIPanel.SetActive(true);

        SpawnVoterAvatars(currentScenario);

        int pointsEarned = 0;
        int totalVotes = yesVotes + noVotes;

        if (totalVotes > 0)
        {
            int voteDifference = Mathf.Abs(yesVotes - noVotes);
            int minPossibleDifference = totalVotes % 2;
            int maxError = totalVotes - minPossibleDifference;
            int currentError = voteDifference - minPossibleDifference;

            if (maxError == 0)
            {
                pointsEarned = 1000;
            }
            else
            {
                float successRatio = 1f - ((float)currentError / maxError);

                if (successRatio >= 0.99f) pointsEarned = 1000;
                else if (successRatio >= 0.66f) pointsEarned = 750;
                else if (successRatio >= 0.33f) pointsEarned = 500;
                else if (successRatio > 0.01f) pointsEarned = 250;
                else pointsEarned = 100;
            }
        }

        float timeRatio = Mathf.Clamp01(currentScenario.timeTakenForVoting / votingTimeLimit);
        int timeBonus = Mathf.RoundToInt(timeRatio * maxTimeBonus);

        localRoundScores[currentScenario.authorName] += (pointsEarned + timeBonus);

        int minorityBonus = 666;
        if (yesVotes != noVotes)
        {
            if (yesVotes < noVotes)
            {
                foreach (var voter in currentScenario.yesVoters)
                {
                    localRoundScores[voter.playerName] += minorityBonus;
                }
            }
            else if (noVotes < yesVotes)
            {
                foreach (var voter in currentScenario.noVoters)
                {
                    localRoundScores[voter.playerName] += minorityBonus;
                }
            }
        }

        yield return new WaitForSeconds(5f);

        currentScenarioIndex++;

        if (currentScenarioIndex < currentScenarios.Count)
        {
            StartVotingPhase();
        }
        else
        {
            StartCoroutine(RoundEndCoroutine());
        }
    }

    private void SpawnVoterAvatars(ScenarioData scenario)
    {
        ClearAvatarsUI();

        foreach (var player in scenario.yesVoters)
        {
            if (yesAvatarContainer != null && avatarPrefab != null)
            {
                GameObject newAvatar = Instantiate(avatarPrefab, yesAvatarContainer);
                Image img = newAvatar.GetComponent<Image>();
                if (img != null && player.avatarId >= 0 && player.avatarId < avatarSprites.Length)
                {
                    img.sprite = avatarSprites[player.avatarId];
                }
            }
        }

        foreach (var player in scenario.noVoters)
        {
            if (noAvatarContainer != null && avatarPrefab != null)
            {
                GameObject newAvatar = Instantiate(avatarPrefab, noAvatarContainer);
                Image img = newAvatar.GetComponent<Image>();
                if (img != null && player.avatarId >= 0 && player.avatarId < avatarSprites.Length)
                {
                    img.sprite = avatarSprites[player.avatarId];
                }
            }
        }
    }

    private void ClearAvatarsUI()
    {
        if (yesAvatarContainer != null)
        {
            foreach (Transform child in yesAvatarContainer) Destroy(child.gameObject);
        }
        if (noAvatarContainer != null)
        {
            foreach (Transform child in noAvatarContainer) Destroy(child.gameObject);
        }
    }

    private IEnumerator RoundEndCoroutine()
    {
        if (currentRound < totalRounds)
        {
            ClearAvatarsUI();
            if (revealUIPanel != null) revealUIPanel.SetActive(false);

            mainDisplayText.text = $"FIM DA RONDA {currentRound}";
            statusText.text = "Preparem-se para a proxima ronda...";

            yield return new WaitForSeconds(4f);

            currentRound++;
            currentScenarios.Clear();
            currentScenarioIndex = 0;

            StartGlobalWritingPhase();
        }
        else
        {
            StartCoroutine(ShowLeaderboardCoroutine());
        }
    }

    private IEnumerator ShowLeaderboardCoroutine()
    {
        currentPhase = GamePhase.LeaderboardPhase;

        ClearAvatarsUI();
        if (revealUIPanel != null) revealUIPanel.SetActive(false);
        if (mainDisplayText != null) mainDisplayText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);

        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);

        if (leaderboardContainer != null)
        {
            foreach (Transform child in leaderboardContainer) Destroy(child.gameObject);
        }

        List<KeyValuePair<string, int>> sortedScores = new List<KeyValuePair<string, int>>(localRoundScores);
        sortedScores.Sort((x, y) => y.Value.CompareTo(x.Value));

        for (int i = 0; i < sortedScores.Count; i++)
        {
            if (leaderboardRowPrefab != null && leaderboardContainer != null)
            {
                GameObject newRow = Instantiate(leaderboardRowPrefab, leaderboardContainer);

                TextMeshProUGUI txt = newRow.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                Image avatarImg = newRow.transform.Find("Avatar").GetComponent<Image>();

                txt.text = $"<b>{i + 1}º lugar:</b> {sortedScores[i].Key} - {sortedScores[i].Value}";

                Color textColor = Color.black;
                if (i == 0) ColorUtility.TryParseHtmlString("#F1C40F", out textColor);
                else if (i == 1) ColorUtility.TryParseHtmlString("#E0E0E0", out textColor);
                else if (i == 2) ColorUtility.TryParseHtmlString("#E67E22", out textColor);
                else textColor = Color.black;

                txt.color = textColor;

                ConnectedPlayer cp = playersInGame.Find(p => p.playerName == sortedScores[i].Key);
                if (cp != null && cp.avatarId >= 0 && cp.avatarId < avatarSprites.Length)
                {
                    avatarImg.sprite = avatarSprites[cp.avatarId];
                }
            }
        }

        yield return null;
    }

    public void OnBackToLobbyButtonPressed()
    {
        if (currentPhase == GamePhase.LeaderboardPhase)
        {
            if (leaderboardContainer != null)
            {
                foreach (Transform child in leaderboardContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            if (menuManager != null)
            {
                menuManager.EndMinigame(this);
                menuManager.ReturnToLobbyFromLeaderboard();
            }

            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        }
    }
}