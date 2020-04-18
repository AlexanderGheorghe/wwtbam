using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const float threshold = 10;
    public enum State
    {
        PickQuestion,
        WaitingForAnswer,
        ValidateAnswer,
        Null
    }
    
    private object threadLocker = new object();
    private bool waitingForReco;
    private string currentInput;
    public QuestionDisplay questionDisplay;
    
    private State _currentState = State.PickQuestion;

    public State currentState
    {
        get => _currentState;
        set
        {
            switch (value)
            {
                case State.PickQuestion:
                    Debug.Log("pickQuestion");
                    break;
                case State.WaitingForAnswer:
                    Debug.Log("waitingforanswer");
                    GetInput(State.ValidateAnswer);
                    break;
                case State.ValidateAnswer:
                    Debug.Log("validateAnswer");
                    var closestAnswer= GetClosestAnswer(out var closestDistance);
                    if (closestDistance > threshold)
                    {
                        errorText.gameObject.SetActive(true);
                        errorText.text = "Didn't get that.";
                        StartCoroutine(WaitUserRead());
                    }
                    else
                    {
                        for (var index = 0; index < answers.Count; index++)
                        {
                            if (answers[index].text == closestAnswer)
                            {
                                answers[index].color = Color.yellow;
                                Debug.Log(answers[index].text);
                                break;
                            }
                        }
                    }
                    break;
                case State.Null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            _currentState = value;
        }
    }
        
    private int currentQuestion = 0;
    private float timeToWait = 2;
    public GetQuestions getQuestions;

    public List<Text> answers;
    public Text errorText;
    // Start is called before the first frame update
    void Start()
    {
        Utils.shuffle(getQuestions.Questions);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.PickQuestion:
                questionDisplay.DisplayQuestion(getQuestions.Questions[currentQuestion]);
                currentState = State.WaitingForAnswer;
                break;
            case State.WaitingForAnswer:
                // StartCoroutine(WaitForAnswer());
                
                break;
            case State.ValidateAnswer:
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    
    
    private IEnumerator WaitUserRead()
    {
        yield return new WaitForSeconds(timeToWait);
        currentState = State.WaitingForAnswer;
        // errorText.gameObject.SetActive(false);
    }

    public async void GetInput(State nextState = State.Null)
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
            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

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
                if (nextState != State.Null)
                {
                    currentState = nextState;
                }
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
}
