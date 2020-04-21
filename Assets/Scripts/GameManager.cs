using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private int threshold = 3;

    public enum State
    {
        WaitingForStart,
        ValidateStart,
        PickQuestion,
        WaitingForAnswer,
        ValidateAnswer,
        CheckForFinalAnswer,
        ValidateFinalAnswer,
        GiveVerdictOnAnswer,
        WaitingForLifeline,
        ValidateLifeline,
        Null
    }

    private object threadLocker = new object();
    private bool waitingForReco;
    private string currentInput = "ceva";
    public QuestionDisplay questionDisplay;
    private const string Yes = "Yes.";
    private const string No = "No.";
    private const string Help = "Help.";
    private const string Nevermind = "Never mind.";
    private const string UnintelligibleInput = "Didn't get that, please try again.";

    private const string Fail = "You lose.";
    private const string Win = "You win.";
    private const string LifelineWaitingText = "Pick one!";
    private const string LifelineDefaultText = "Say help to activate";
    private const string LifelineUsedOneText = "Only one lifeline per question allowed.";
    private const string LifelinesUsedBothText = "You're out of lifelines :(";
    private int LifelinesUsed;
    
    private Color DisabledColor = Color.gray;
    private Color EnabledColor = Color.white;

    private State _currentState = State.WaitingForStart;
    private string currentAnswer;
    private int IncorrectAnswersCount = 0;
    private const int Lives = 3;
    public Light Light;
    private const int ColorFlashTime = 3;
    private Color DefaultColor;
    public Canvas GameScreen;
    public Canvas StartMenu;
    private List<String> PrizeStrings = new List<String> {
        "100",
        "200",
        "300",
        "500",
        "1.000",
        "2.000",
        "4.000",
        "8.000",
        "16.000",
        "32.000",
        "64.000",
        "125.000",
        "250.000",
        "500.000",
        "1.000.000"
    };

    public List<Text> PrizeTexts;
    private bool UsedALifeline;

    public State currentState
    {
        get => _currentState;
        set
        {
            switch (value)
            {
                case State.WaitingForStart:
                    EndGameMessage.gameObject.SetActive(false);
                    LifelinesHelp.gameObject.SetActive(false);
                    StartMenu.gameObject.SetActive(true);
                    GameScreen.gameObject.SetActive(false);
                    _currentState = value;
                    waitingForReco = true;
                    GetInput();
                    break;
                case State.ValidateStart:
                    if (currentInput == "Start.")
                    {
                        Reset();
                        currentState = State.PickQuestion;
                        StartMenu.gameObject.SetActive(false);
                        GameScreen.gameObject.SetActive(true);
                        LifelinesHelp.gameObject.SetActive(true);
                    }
                    else
                    {
                        currentState = State.WaitingForStart;
                    }
                    break;
                case State.PickQuestion:
                    Debug.Log("pickQuestion");
                    if (LifelinesUsed == 2)
                    {
                        LifelinesHelp.text = LifelinesUsedBothText;
                    }
                    else
                    {
                        LifelinesHelp.text = LifelineDefaultText;
                    }
                    UsedALifeline = false;
                    foreach (var percentage in percentages)
                    {
                        percentage.gameObject.SetActive(false);
                    }
                    if (IncorrectAnswersCount == Lives)
                    {
                        EndGameMessage.gameObject.SetActive(true);
                        EndGameMessage.color = Color.red;
                        EndGameMessage.text = Fail;
                        StartCoroutine(WaitUserRead(State.WaitingForStart));
                        // _currentState = State.Null;
                    }
                    else if (currentQuestion == getQuestions.Questions.Count)
                    {
                        EndGameMessage.gameObject.SetActive(true);
                        EndGameMessage.text = Win;
                        StartCoroutine(WaitUserRead(State.WaitingForStart));
                        // _currentState = State.Null;
                    }
                    else
                    {
                        if (currentQuestion != 0)
                        {
                            PrizeTexts[currentQuestion - 1].color = Color.white;
                        }
                        PrizeTexts[currentQuestion].color = Color.green;
                        SetThreshold();
                        _currentState = value;
                    }
                    break;
                case State.WaitingForAnswer:
                    Debug.Log("waitingforanswer");
                    waitingForReco = true;
                    GetInput();
                    _currentState = value;
                    break;
                case State.ValidateAnswer:
                    Debug.Log("validateAnswer");
                    Debug.Log(currentInput);
                    
                    errorText.gameObject.SetActive(false);
                    if (currentInput == Help && !UsedALifeline && !(LifelinesUsed == 2))
                    {
                        currentState = State.WaitingForLifeline;
                        break;
                    }
                    var closestAnswer= GetClosestAnswer(out var closestDistance);
                    Debug.Log(closestAnswer);
                    currentAnswer = closestAnswer;
                    if (closestDistance > threshold)
                    {
                        errorText.gameObject.SetActive(true);
                        errorText.text = UnintelligibleInput;
                        // StartCoroutine(WaitUserRead(State.WaitingForAnswer));
                        currentState = State.WaitingForAnswer;
                    }
                    else
                    {
                        for (var index = 0; index < answers.Count; index++)
                        {
                            if (answers[index].text == closestAnswer)
                            {
                                answers[index].color = Color.yellow;
                                Debug.Log(answers[index].text);
                                // break;
                            }
                            else
                            {
                                answers[index].color = Color.white;
                            }
                        }

                        currentState = State.CheckForFinalAnswer;
                    }
                    break;
                case State.CheckForFinalAnswer:
                    Debug.Log("check final");
                    errorText.gameObject.SetActive(false);
                    FinalAnswerCheck.gameObject.SetActive(true);
                    waitingForReco = true;
                    GetInput();
                    _currentState = value;
                    break;
                case State.ValidateFinalAnswer:
                    Debug.Log("validate final answer");
                    FinalAnswerCheck.gameObject.SetActive(false);
                    var finalAnswer = GetYesNo(out var minDistance);
                    if (minDistance > 1)
                    {
                        errorText.text = UnintelligibleInput;
                        currentState = State.CheckForFinalAnswer;
                        // StartCoroutine(WaitUserRead(State.CheckForFinalAnswer));
                    }
                    else
                    {
                        if (finalAnswer == No)
                        {
                            foreach (var answer in answers)
                            {
                                answer.color = Color.white;
                            }
                            currentState = State.WaitingForAnswer;
                        }
                        else
                        {
                            currentState = State.GiveVerdictOnAnswer;
                        }
                    }
                    break;
                case State.GiveVerdictOnAnswer:
                    Debug.Log("give verdict");
                    var correctAnswer = getQuestions.Questions[currentQuestion].correct_answer;
                    int correctAnswerIndex=0, currentAnswerIndex=1;
                    for (int i = 0; i < answers.Count; i++)
                    {
                        if (answers[i].text == correctAnswer)
                        {
                            correctAnswerIndex = i;
                        }
                        if (answers[i].text == currentAnswer)
                        {
                            currentAnswerIndex= i;
                        }
                    }
                    answers[currentAnswerIndex].color = Color.red;
                    answers[correctAnswerIndex].color = Color.green;
                    currentQuestion++;
                    if (correctAnswerIndex != currentAnswerIndex)
                    {
                        IncorrectAnswersCount++;
                        LivesLeft.text = "Lives left: " + (Lives - IncorrectAnswersCount);
                        StartCoroutine(ChangeLightColor(Color.red));
                    }
                    else
                    {
                        StartCoroutine(ChangeLightColor(Color.green));

                    }

                    _currentState = value;
                    currentState = State.Null;
                    break;
                case State.WaitingForLifeline:
                    Debug.Log("waitingforlifeline");
                    LifelinesHelp.text = LifelineWaitingText;
                    waitingForReco = true;
                    GetInput();
                    _currentState = value;
                    break;
                case State.ValidateLifeline:
                    Debug.Log("validate lifeline:" + currentInput);
                    if ((currentInput == "5050" || currentInput == "5050.") && Lifeline5050.color != DisabledColor)
                    {
                        EliminateTwoIncorrectAnswers();
                        UseLifeline(Lifeline5050);
                    } else if (currentInput == "Ask the audience." && LifelineAskTheAudience.color != DisabledColor)
                    {
                        DisplayAudiencePercentages();
                        UseLifeline(LifelineAskTheAudience);
                    }
                    else if (currentInput == Nevermind)
                    {
                        LifelinesHelp.text = LifelineDefaultText;
                        currentState = State.WaitingForAnswer;
                    }
                    else
                    {
                        currentState = State.WaitingForLifeline;
                    }
                    break;
                case State.Null:
                    StartCoroutine(WaitUserRead(State.PickQuestion));
                    break;
                // default:
                //     throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    private void UseLifeline(Text LifelineText)
    {
        LifelineText.color = DisabledColor;
        UsedALifeline = true;
        LifelinesHelp.text = LifelineUsedOneText;
        currentState = State.WaitingForAnswer;
        LifelinesUsed++;
    }
    private void DisplayAudiencePercentages()
    {
        bool GotItRight = Random.Range(0, 1) <= CorrectPublicAnswerProbability;
        int sum = 100;
            List<int> random = new List<int>();
            int largestPercentage = -1, largestIndex = -1;
            for (int i = 0; i < answers.Count-1; i++)
            {
                random.Add(Random.Range(0, sum));
                Debug.Log(random[i]);
                sum -= random[i];
                if (random[i] > largestPercentage)
                {
                    largestPercentage = random[i];
                    largestIndex = i;
                }
            }
            if (sum > largestPercentage)
            {
                largestPercentage = sum;
                largestIndex = answers.Count - 1;
            }
            random.Add(sum);
            int j = 0;
            for (int i = 0; i < answers.Count; i++)
            {
                percentages[i].gameObject.SetActive(true);
                if (GotItRight)
                {
                    if (answers[i].text == getQuestions.Questions[currentQuestion].correct_answer)
                    {
                        percentages[i].text = largestPercentage + "%";
                    }
                    else
                    {
                        if (j == largestIndex)
                        {
                            j++;
                        }

                        percentages[i].text = random[j++] + "%";
                    }
                }
                else
                {
                    percentages[i].text = random[j++] + "%";
                }
            }
        }

    private void SetThreshold()
    {
        var Question = getQuestions.Questions[currentQuestion];
        var AllAnswers = Question.incorrect_answers.ToList();
        AllAnswers.Add(Question.correct_answer);
        int LongestAnswer = AllAnswers.Aggregate(0, (l, c) => Mathf.Min(l, c.Length));
        threshold = LongestAnswer / 3;
        if (threshold < 2)
        {
            threshold = 2;
        }
    }

    public void ShuffleQuestions()
    {
        Utils.shuffle(getQuestions.Questions);
        var DifficultyDictionary = new Dictionary<string, int>()
        {
            {"easy", 0},
            {"medium", 1},
            {"hard", 2},
        };
        getQuestions.Questions.Sort((a, b) => DifficultyDictionary[a.difficulty] - DifficultyDictionary[b.difficulty]);
        foreach (var q in getQuestions.Questions)
        {
            Debug.Log(q.difficulty + " " + q.question);
        }
    }

    private void EliminateTwoIncorrectAnswers()
    {
        var IncorrectAnswers = getQuestions.Questions[currentQuestion].incorrect_answers.ToList();
        Utils.shuffle(IncorrectAnswers);
        foreach (var answer in answers)
        {
            if (answer.text == IncorrectAnswers[1] || answer.text == IncorrectAnswers[0])
            {
                answer.gameObject.SetActive(false);
            }
        }
    }
    private void Reset()
    {
        foreach (var text in PrizeTexts)
        {
            text.color = Color.white;
        }
        LivesLeft.text = "Lives left: " + Lives;
        ShuffleQuestions();
        IncorrectAnswersCount = 0;
        currentQuestion = 0;
        LifelinesHelp.text = LifelineDefaultText;
        Lifeline5050.color = EnabledColor;
        LifelineAskTheAudience.color = EnabledColor;
        UsedALifeline = false;
        LifelinesUsed = 0;
    }

    private int currentQuestion = 0;
    private float timeToWait = 2;
    public GetQuestions getQuestions;

    
    public List<Text> answers;
    public List<Text> percentages;
    public Text errorText;
    public Text FinalAnswerCheck;
    public Text EndGameMessage;

    private const float CorrectPublicAnswerProbability = 0.64f;

    public Text LivesLeft;
    public Text LifelinesHelp;
    public Text Lifeline5050;
    public Text LifelineAskTheAudience;
    // Start is called before the first frame update
    void Start()
    {
        DefaultColor = Light.color;
        for (var i = 0; i < PrizeStrings.Count; i++)
        {
            PrizeTexts[i].text = "$" + PrizeStrings[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(currentState);
        switch (currentState)
        {
            case State.WaitingForStart:
                // StartCoroutine(WaitForAnswer());
                if (!waitingForReco)
                {
                    currentState = State.ValidateStart;
                }
                break;
            case State.PickQuestion:
                questionDisplay.DisplayQuestion(getQuestions.Questions[currentQuestion]);
                currentState = State.WaitingForAnswer;
                break;
            case State.WaitingForAnswer:
                // StartCoroutine(WaitForAnswer());
                if (!waitingForReco)
                {
                    currentState = State.ValidateAnswer;
                }
                break;
            // case State.ValidateAnswer:
            //     
            //     break;
            case State.CheckForFinalAnswer:
                if (!waitingForReco)
                {
                    currentState = State.ValidateFinalAnswer;
                }
                break;
            case State.WaitingForLifeline:
                if (!waitingForReco)
                {
                    currentState = State.ValidateLifeline;
                }
                break;
            // default:
            //     throw new ArgumentOutOfRangeException();
        }
    }

    
    
    private IEnumerator WaitUserRead(State nextState)
    {
        yield return new WaitForSeconds(timeToWait);
        currentState = nextState;
    }

    private IEnumerator ChangeLightColor(Color newColor)
    {
        Light.color = newColor;
        yield return new WaitForSeconds(ColorFlashTime);
        Light.color = DefaultColor;
    }

    public async void GetInput()
    {
        // Creates an instance of a speech config with specified subscription key and service region.
        // Replace with your own subscription key and service region (e.g., "westus").
        var config = SpeechConfig.FromSubscription("df65df3407f1482da419ff9469f74f6d", "westus");

        // Make sure to dispose the recognizer after use!
        using (var recognizer = new SpeechRecognizer(config))
        {
            lock (threadLocker)
            {
                waitingForReco = true;
            }

            // Starts speech recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result.
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query.
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
            var result = await recognizer.RecognizeOnceAsync();//.ConfigureAwait(false);

            // Checks result.
            string newMessage = string.Empty;
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                newMessage = result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                newMessage = "NOMATCH: Speech could not be recognized.";
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                newMessage = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";
            }

            lock (threadLocker)
            {
                currentInput = newMessage;
                waitingForReco = false;
                recognizer.Dispose();
            }
        }
    }

    
    private string GetClosestAnswer(out int closestDistance)
    {
        string correct = getQuestions.Questions[currentQuestion].correct_answer;
        closestDistance = Utils.LevenshteinDistance(currentInput, correct);
        string closestAnswer = correct;
        foreach (var answer in getQuestions.Questions[currentQuestion].incorrect_answers)
        {
            int distance = Utils.LevenshteinDistance(currentInput, answer);
            if (distance < closestDistance)
            {
                closestAnswer = answer;
                closestDistance = distance;
            }
        }

        return closestAnswer;
    }

    private string GetYesNo(out int minDistance)
    {
        var distanceYes = Utils.LevenshteinDistance(currentInput, Yes);
        var distanceNo = Utils.LevenshteinDistance(currentInput, No);
        if (distanceYes > distanceNo)
        {
            minDistance = distanceNo;
            return No;
        }

        minDistance = distanceYes;
        return Yes;
    }
}
