using System;
using System.Collections.Generic;

[Serializable]
public class Question
{
    public string category;
    public string type;
    public string difficulty;
    public string question;
    public List<string> incorrect_answers;
    public string correct_answer;
}