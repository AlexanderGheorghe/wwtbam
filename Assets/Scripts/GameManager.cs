using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private const float threshold = 3;
    public enum State
    {
        PickQuestion,
        WaitingForAnswer,
        ValidateAnswer,
        CheckForFinalAnswer,
        ValidateFinalAnswer,
        GiveVerdictOnAnswer,
        Null
    }
    
    private object threadLocker = new object();
    private bool waitingForReco;
    private string currentInput = "ceva";
    public QuestionDisplay questionDisplay;
    private const string Yes = "Yes.";
    private const string No = "No.";
    private const string UnintelligibleInput = "Didn't get that.";

    private const string Fail = "You fail.";
    private const string Win = "You win.";
    
    private State _currentState = State.PickQuestion;
    private string currentAnswer;
    private int IncorrectAnswersCount = 0;

    public State currentState
    {
        get => _currentState;
        set
        {
            switch (value)
            {
                case State.PickQuestion:
                    Debug.Log("pickQuestion");
                    if (IncorrectAnswersCount == 3)
                    {
                        EndGameMessage.gameObject.SetActive(true);
                        EndGameMessage.color = Color.red;
                        EndGameMessage.text = Fail;
                        // _currentState = State.Null;
                    }
                    else if (currentQuestion == getQuestions.Questions.Count)
                    {
                        EndGameMessage.gameObject.SetActive(true);
                        EndGameMessage.text = Win;
                        // _currentState = State.Null;
                    }
                    else
                    {
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
                    var closestAnswer= GetClosestAnswer(out var closestDistance);
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
                    }

                    _currentState = value;
                    currentState = State.Null;
                    break;
                case State.Null:
                    StartCoroutine(WaitUserRead(State.PickQuestion));
                    break;
                // default:
                //     throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
        
    private int currentQuestion = 0;
    private float timeToWait = 2;
    public GetQuestions getQuestions;

    
    public List<Text> answers;
    public Text errorText;
    public Text FinalAnswerCheck;
    public Text EndGameMessage;
    // Start is called before the first frame update
    void Start()
    {
        Utils.shuffle(getQuestions.Questions);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(currentState);
        switch (currentState)
        {
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
            // default:
            //     throw new ArgumentOutOfRangeException();
        }
    }

    
    
    private IEnumerator WaitUserRead(State nextState)
    {
        yield return new WaitForSeconds(timeToWait);
        currentState = nextState;
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
                Debug.Log("lock");
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
