using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class QuestionDisplay : MonoBehaviour
{
    public GetQuestions getQuestions;

    public Text question;

    public Text answer1;

    public Text answer2;

    public Text answer3;

    public Text answer4;
    // Start is called before the first frame update
    public void DisplayQuestion(Question q)
    {
        question.text = WebUtility.HtmlDecode(q.question);
        var answers = shuffledAnswers(q);
        answer1.text = WebUtility.HtmlDecode(answers[0]);
        answer2.text = WebUtility.HtmlDecode(answers[1]);
        answer3.text = WebUtility.HtmlDecode(answers[2]);
        answer4.text = WebUtility.HtmlDecode(answers[3]);
    }

    private List<string> shuffledAnswers(Question q) {
        var answers = q.incorrect_answers.ToList();
        // foreach (var answer in answers)
        // {
        //     Debug.Log(answer);
        // }
        answers.Add(q.correct_answer);
        Utils.shuffle(answers);
        return new List<string>(answers);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
