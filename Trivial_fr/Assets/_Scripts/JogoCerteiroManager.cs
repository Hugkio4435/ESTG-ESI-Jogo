using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class TriviaQuestion
{
    public string questionText;
    public string[] answers = new string[4];
    public int correctAnswerIndex; // 0=A, 1=B, 2=C, 3=D
}

public class JogoCerteiroManager : MonoBehaviour, IMinigame
{
    public enum GamePhase
    {
        WaitingToStart,
        TutorialPhase,
        ReadingPhase,
        VotingPhase,
        RevealPhase,
        LeaderboardPhase
    }

    [Header("Estado do Jogo")]
    public GamePhase currentPhase = GamePhase.WaitingToStart;
    private List<ConnectedPlayer> playersInGame;
    private MenuManager menuManager;

    [Header("Temporizador")]
    public TextMeshProUGUI timerText;
    public float readingTimeLimit = 5f;
    public float votingTimeLimit = 15f;
    private float currentTime;
    private bool isTimerRunning = false;

    [Header("Pontuações (Estilo Kahoot)")]
    public int basePointsForCorrect = 500;
    public int maxTimeBonus = 500;
    private Dictionary<string, int> localRoundScores = new Dictionary<string, int>();

    [Header("UI Principal")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI RoundText;
    public TextMeshProUGUI[] answerTexts;
    public Image[] answerBackgrounds;
    public TextMeshProUGUI statusText;

    [Header("Paineis")]
    public GameObject tutorialPanel;
    public GameObject gameUIPanel;
    public GameObject leaderboardPanel;
    private bool tutorialSkipped = false;

    [Header("Leaderboard Final (Reaproveitado)")]
    public Transform leaderboardContainer;
    public GameObject leaderboardRowPrefab;
    public Sprite[] avatarSprites;

    [Header("Base de Dados de Perguntas")]
    public List<TriviaQuestion> questionsDatabase = new List<TriviaQuestion>();
    private List<TriviaQuestion> availableQuestions = new List<TriviaQuestion>();
    private TriviaQuestion currentQuestion;
    private int currentRound = 1;
    public int totalRounds = 5;

    private Dictionary<string, PlayerAnswerData> playerAnswers = new Dictionary<string, PlayerAnswerData>();

    private class PlayerAnswerData
    {
        public int answerIndex;
        public float timeTaken;
    }

    private void Awake()
    {
        if (questionsDatabase.Count == 0)
        {
            questionsDatabase.Add(new TriviaQuestion { questionText = "Qual é o nome do criador deste jogo brutal?", answers = new string[] { "Hugo", "Miguel", "João", "Diogo" }, correctAnswerIndex = 0 });
            questionsDatabase.Add(new TriviaQuestion { questionText = "Em que linguagem de programação foi feito este projeto?", answers = new string[] { "Java", "C++", "C#", "Python" }, correctAnswerIndex = 2 });
            questionsDatabase.Add(new TriviaQuestion { questionText = "Qual é a capital de Portugal?", answers = new string[] { "Porto", "Coimbra", "Faro", "Lisboa" }, correctAnswerIndex = 3 });
            questionsDatabase.Add(new TriviaQuestion { questionText = "Quantos bits tem um byte?", answers = new string[] { "4", "8", "16", "32" }, correctAnswerIndex = 1 });
            questionsDatabase.Add(new TriviaQuestion { questionText = "Qual destes não é um motor de jogo?", answers = new string[] { "Unity", "Unreal Engine", "Godot", "Photoshop" }, correctAnswerIndex = 3 });
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(currentTime).ToString();
                timerText.color = (currentTime <= 5f) ? Color.red : Color.white;
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
        if (currentPhase == GamePhase.ReadingPhase)
        {
            StartVotingPhase();
        }
        else if (currentPhase == GamePhase.VotingPhase)
        {
            StartRevealPhase();
        }
    }

    public Dictionary<string, int> CalculateRoundScores()
    {
        return localRoundScores;
    }

    public void InitializeGame(List<ConnectedPlayer> playersFromLobby, MenuManager manager)
    {
        AudioManager.Instance.PlayMusic("TriviaTheme");

        menuManager = manager;
        playersInGame = playersFromLobby;
        currentRound = 1;
        localRoundScores.Clear();
        availableQuestions = new List<TriviaQuestion>(questionsDatabase);

        availableQuestions = availableQuestions.OrderBy(x => Random.value).ToList();

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

        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        NetworkManager.Instance.BroadcastToPhones("GAME_STARTED_TRIVIA", null);

        yield return new WaitUntil(() => tutorialSkipped);

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (gameUIPanel != null) gameUIPanel.SetActive(true);

        StartRound();
    }

    public void OnTutorialContinueButtonPressed()
    {
        if (currentPhase == GamePhase.TutorialPhase) tutorialSkipped = true;
    }

    private void StartRound()
    {
        if (availableQuestions.Count == 0) availableQuestions = new List<TriviaQuestion>(questionsDatabase);

        currentQuestion = availableQuestions[0];
        availableQuestions.RemoveAt(0);
        playerAnswers.Clear();

        ResetColors();

        currentPhase = GamePhase.ReadingPhase;
        // questionText.text = $"Ronda {currentRound}: {currentQuestion.questionText}";
        questionText.text = $"{currentQuestion.questionText}";
        RoundText.text = $"Ronda {currentRound}";

        for (int i = 0; i < 4; i++)
        {
            if (i < answerTexts.Length) answerTexts[i].text = currentQuestion.answers[i];
        }

        statusText.text = "A preparar pergunta...";

        NetworkManager.Instance.BroadcastToPhones("PREPARE_TRIVIA", null);
        StartTimer(readingTimeLimit);
    }

    private void StartVotingPhase()
    {
        currentPhase = GamePhase.VotingPhase;
        statusText.text = $"Respostas: 0 / {playersInGame.Count}";

        NetworkManager.Instance.BroadcastToPhones("START_TRIVIA_VOTE", null);
        StartTimer(votingTimeLimit);
    }

    public void ProcessAction(ActionResponse data)
    {
        if (currentPhase == GamePhase.VotingPhase && data.actionType == "SUBMIT_TRIVIA_ANSWER")
        {
            if (!playerAnswers.ContainsKey(data.playerName))
            {
                playerAnswers.Add(data.playerName, new PlayerAnswerData
                {
                    answerIndex = data.triviaAnswerIndex,
                    timeTaken = votingTimeLimit - currentTime
                });

                statusText.text = $"Respostas: {playerAnswers.Count} / {playersInGame.Count}";

                if (playerAnswers.Count >= playersInGame.Count)
                {
                    StopTimer();
                    StartRevealPhase();
                }
            }
        }
    }

    private void StartRevealPhase()
    {
        StopTimer();
        currentPhase = GamePhase.RevealPhase;
        StartCoroutine(RevealCoroutine());
    }

    private IEnumerator RevealCoroutine()
    {
        // Revelar as cores (Fundo e Texto perdem opacidade nas erradas)
        for (int i = 0; i < 4; i++)
        {
            // 1. Alterar a opacidade do Fundo
            if (i < answerBackgrounds.Length && answerBackgrounds[i] != null)
            {
                Color corFundo = answerBackgrounds[i].color;
                corFundo.a = (i == currentQuestion.correctAnswerIndex) ? 1f : 0.3f;
                answerBackgrounds[i].color = corFundo;
            }

            // 2. Alterar a opacidade do Texto
            if (i < answerTexts.Length && answerTexts[i] != null)
            {
                Color corTexto = answerTexts[i].color;
                corTexto.a = (i == currentQuestion.correctAnswerIndex) ? 1f : 0.3f;
                answerTexts[i].color = corTexto;
            }
        }

        int correctCount = 0;

        foreach (var answer in playerAnswers)
        {
            if (answer.Value.answerIndex == currentQuestion.correctAnswerIndex)
            {
                correctCount++;
                float timeRatio = Mathf.Clamp01(1f - (answer.Value.timeTaken / votingTimeLimit));
                int roundPoints = basePointsForCorrect + Mathf.RoundToInt(timeRatio * maxTimeBonus);

                localRoundScores[answer.Key] += roundPoints;
            }
        }

        statusText.text = $"{correctCount} jogadores acertaram!";
        NetworkManager.Instance.BroadcastToPhones("SHOW_TRIVIA_RESULTS", null);

        yield return new WaitForSeconds(5f);

        if (currentRound < totalRounds)
        {
            currentRound++;
            StartRound();
        }
        else
        {
            StartCoroutine(ShowLeaderboardCoroutine());
        }
    }

    private void ResetColors()
    {
        // Volta a colocar os botões com opacidade a 100% para a próxima ronda
        foreach (var bg in answerBackgrounds)
        {
            if (bg != null)
            {
                Color corAtual = bg.color;
                corAtual.a = 1f;
                bg.color = corAtual;
            }
        }
    }

    private IEnumerator ShowLeaderboardCoroutine()
    {
        currentPhase = GamePhase.LeaderboardPhase;

        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
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
            if (menuManager != null)
            {
                menuManager.EndMinigame(this);
                menuManager.ReturnToLobbyFromLeaderboard();
            }

            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

            //this.gameObject.SetActive(false);
        }
    }
}