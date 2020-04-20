using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GetQuestions : MonoBehaviour {
    public List<Question> Questions;

    private void LoadJson() {
        const string json = "{\"response_code\":0,\"results\":[{\"category\":\"Entertainment: Television\",\"type\":\"multiple\",\"difficulty\":\"easy\",\"question\":\"In the TV show &quot;Cheers&quot;, Sam Malone was a former relief pitcher for which baseball team?\",\"correct_answer\":\"Boston Red Sox\",\"incorrect_answers\":[\"New York Mets\",\"Baltimore Orioles\",\"Milwaukee Brewers\"]},{\"category\":\"Entertainment: Japanese Anime & Manga\",\"type\":\"multiple\",\"difficulty\":\"medium\",\"question\":\"In Dragon Ball Z, who was the first character to go Super Saiyan 2?\",\"correct_answer\":\"Gohan\",\"incorrect_answers\":[\"Goku\",\"Vegeta\",\"Trunks\"]},{\"category\":\"Science & Nature\",\"type\":\"multiple\",\"difficulty\":\"medium\",\"question\":\"To the nearest minute, how long does it take for light to travel from the Sun to the Earth?\",\"correct_answer\":\"8 Minutes\",\"incorrect_answers\":[\"6 Minutes\",\"2 Minutes\",\"12 Minutes\"]},{\"category\":\"Entertainment: Film\",\"type\":\"multiple\",\"difficulty\":\"medium\",\"question\":\"In Back to the Future Part II, Marty and Dr. Emmett Brown go to which future date?\",\"correct_answer\":\"October 21, 2015\",\"incorrect_answers\":[\"August 28, 2015\",\"July 20, 2015\",\"January 25, 2015\"]},{\"category\":\"Entertainment: Video Games\",\"type\":\"multiple\",\"difficulty\":\"easy\",\"question\":\"In which year was League of Legends released?\",\"correct_answer\":\"2009\",\"incorrect_answers\":[\"2010\",\"2003\",\"2001\"]}]}"; //r.ReadToEnd();
        Questions = JsonUtility.FromJson<QuestionsClass>(json).results;
    }

    [Serializable]
    public class QuestionsClass {
        public List<Question> results;
    }

    private void Awake() {
        LoadJson();
    }
}