using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    private const float threshold = 3;

    public enum State {
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

    private State currentState = State.PickQuestion;
    private string currentAnswer;
    private int incorrectAnswersCount = 0;
    private const int Lives = 3;
    public Light Light;
    private const int ColorFlashTime = 3;
    private Color defaultColor;

    public State CurrentState {
        get => currentState;
        set {
            switch (value) {
                case State.PickQuestion:
                    Debug.Log("pickQuestion");
                    if (incorrectAnswersCount == Lives) {
                        endGameMessage.gameObject.SetActive(true);
                        endGameMessage.color = Color.red;
                        endGameMessage.text = Fail;
                        // _currentState = State.Null;
                    }
                    else if (currentQuestion == getQuestions.Questions.Count) {
                        endGameMessage.gameObject.SetActive(true);
                        endGameMessage.text = Win;
                        // _currentState = State.Null;
                    }
                    else {
                        currentState = value;
                    }

                    break;
                case State.WaitingForAnswer:
                    Debug.Log("waitingforanswer");
                    waitingForReco = true;
                    GetInput();
                    currentState = value;
                    break;
                case State.ValidateAnswer:
                    Debug.Log("validateAnswer");
                    Debug.Log(currentInput);
                    errorText.gameObject.SetActive(false);
                    var closestAnswer = GetClosestAnswer(out var closestDistance);
                    currentAnswer = closestAnswer;
                    if (closestDistance > threshold) {
                        errorText.gameObject.SetActive(true);
                        errorText.text = UnintelligibleInput;
                        // StartCoroutine(WaitUserRead(State.WaitingForAnswer));
                        CurrentState = State.WaitingForAnswer;
                    }
                    else {
                        for (var index = 0; index < answers.Count; index++) {
                            if (answers[index].text == closestAnswer) {
                                answers[index].color = Color.yellow;
                                Debug.Log(answers[index].text);
                                // break;
                            }
                            else {
                                answers[index].color = Color.white;
                            }
                        }

                        CurrentState = State.CheckForFinalAnswer;
                    }

                    break;
                case State.CheckForFinalAnswer:
                    Debug.Log("check final");
                    errorText.gameObject.SetActive(false);
                    finalAnswerCheck.gameObject.SetActive(true);
                    waitingForReco = true;
                    GetInput();
                    currentState = value;
                    break;
                case State.ValidateFinalAnswer:
                    Debug.Log("validate final answer");
                    finalAnswerCheck.gameObject.SetActive(false);
                    var finalAnswer = GetYesNo(out var minDistance);
                    if (minDistance > 1) {
                        errorText.text = UnintelligibleInput;
                        CurrentState = State.CheckForFinalAnswer;
                        // StartCoroutine(WaitUserRead(State.CheckForFinalAnswer));
                    }
                    else {
                        if (finalAnswer == No) {
                            CurrentState = State.WaitingForAnswer;
                        }
                        else {
                            CurrentState = State.GiveVerdictOnAnswer;
                        }
                    }

                    break;
                case State.GiveVerdictOnAnswer:
                    Debug.Log("give verdict");
                    var correctAnswer = getQuestions.Questions[currentQuestion].correct_answer;
                    int correctAnswerIndex = 0, currentAnswerIndex = 1;
                    for (int i = 0; i < answers.Count; i++) {
                        if (answers[i].text == correctAnswer) {
                            correctAnswerIndex = i;
                        }

                        if (answers[i].text == currentAnswer) {
                            currentAnswerIndex = i;
                        }
                    }

                    answers[currentAnswerIndex].color = Color.red;
                    answers[correctAnswerIndex].color = Color.green;
                    currentQuestion++;
                    if (correctAnswerIndex != currentAnswerIndex) {
                        incorrectAnswersCount++;
                        livesLeft.text = "Lives left: " + (Lives - incorrectAnswersCount);
                        StartCoroutine(ChangeLightColor(Color.red));
                    }
                    else {
                        StartCoroutine(ChangeLightColor(Color.green));
                    }

                    currentState = value;
                    CurrentState = State.Null;
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
    private const float TimeToWait = 2;
    public GetQuestions getQuestions;


    public List<Text> answers;
    public Text errorText;
    public Text finalAnswerCheck;
    public Text endGameMessage;

    public Text livesLeft;

    void Start() {
        livesLeft.text = "Lives left: " + Lives;
        Utils.shuffle(getQuestions.Questions);
        defaultColor = Light.color;
    }

    void Update() {
        switch (CurrentState) {
            case State.PickQuestion:
                questionDisplay.DisplayQuestion(getQuestions.Questions[currentQuestion]);
                CurrentState = State.WaitingForAnswer;
                break;
            case State.WaitingForAnswer:
                if (!waitingForReco) {
                    CurrentState = State.ValidateAnswer;
                }

                break;
            case State.CheckForFinalAnswer:
                if (!waitingForReco) {
                    CurrentState = State.ValidateFinalAnswer;
                }

                break;
        }
    }


    private IEnumerator WaitUserRead(State nextState) {
        yield return new WaitForSeconds(TimeToWait);
        CurrentState = nextState;
    }

    private IEnumerator ChangeLightColor(Color newColor) {
        Light.color = newColor;
        yield return new WaitForSeconds(ColorFlashTime);
        Light.color = defaultColor;
    }

    private async void GetInput() {
        var config = SpeechConfig.FromSubscription("df65df3407f1482da419ff9469f74f6d", "westus");

        using (var recognizer = new SpeechRecognizer(config)) {

            lock (threadLocker) {
                waitingForReco = true;
            }

            var result = await recognizer.RecognizeOnceAsync();

            // Checks result.
            string newMessage = string.Empty;
            if (result.Reason == ResultReason.RecognizedSpeech) {
                newMessage = result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch) {
                newMessage = "NOMATCH: Speech could not be recognized.";
            }
            else if (result.Reason == ResultReason.Canceled) {
                var cancellation = CancellationDetails.FromResult(result);
                newMessage = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";
            }

            lock (threadLocker) {
                currentInput = newMessage;
                waitingForReco = false;
                recognizer.Dispose();
            }
        }
    }


    private string GetClosestAnswer(out int closestDistance) {
        var correct = getQuestions.Questions[currentQuestion].correct_answer;
        closestDistance = Utils.LevenshteinDistance(currentInput, correct);
        var closestAnswer = correct;
        foreach (var answer in getQuestions.Questions[currentQuestion].incorrect_answers) {
            int distance = Utils.LevenshteinDistance(currentInput, answer);
            if (distance < closestDistance) {
                closestAnswer = answer;
                closestDistance = distance;
            }
        }

        return closestAnswer;
    }

    private string GetYesNo(out int minDistance) {
        var distanceYes = Utils.LevenshteinDistance(currentInput, Yes);
        var distanceNo = Utils.LevenshteinDistance(currentInput, No);
        if (distanceYes > distanceNo) {
            minDistance = distanceNo;
            return No;
        }

        minDistance = distanceYes;
        return Yes;
    }
}