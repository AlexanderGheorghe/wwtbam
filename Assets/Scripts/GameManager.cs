using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text input;
    public enum States
    {
        PickQuestion,
        WaitingForAnswer,
        ValidateAnswer,
    }
    
    private object threadLocker = new object();
    private bool waitingForReco;
    private string currentInput;
    public QuestionDisplay questionDisplay;
    public States currentState = States.PickQuestion;
    private int currentQuestion = 0;
    private float timeToWait = 10;
    public GetQuestions getQuestions;
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
            case States.PickQuestion:
                questionDisplay.DisplayQuestion(getQuestions.Questions[currentQuestion]);
                currentState = States.WaitingForAnswer;
                break;
            case States.WaitingForAnswer:
                // StartCoroutine(WaitForAnswer());
                ButtonClick();
                break;
            case States.ValidateAnswer:
                Debug.Log("currentInput:" + currentInput);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator WaitForAnswer()
    {
        yield return new WaitForSeconds(timeToWait);
        currentState = States.ValidateAnswer;
        currentInput = input.text;
    }

    public async void ButtonClick()
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
                currentState = States.ValidateAnswer;
            }
        }
    }

    
    private string GetClosestAnswer(out float closestDistance)
    {
        string correct = getQuestions.Questions[currentQuestion].correct_answer;
        closestDistance = Utils.LevenshteinDistance(input.text, correct);
        string closestAnswer = correct;
        foreach (var answer in getQuestions.Questions[currentQuestion].incorrect_answers)
        {
            float distance = Utils.LevenshteinDistance(input.text, answer);
            if (distance < closestDistance)
            {
                closestAnswer = answer;
                closestDistance = distance;
            }
        }

        return closestAnswer;
    }
}
