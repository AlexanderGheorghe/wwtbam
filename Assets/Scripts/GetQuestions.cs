using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GetQuestions : MonoBehaviour
{
    public List<Question> Questions;
    public void LoadJson()
    {
        // using (StreamReader r = new StreamReader("Assets/questions.json"))
        // {
        string json = "{\n  \"results\": [\n    {\n      \"category\": \"General Knowledge\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"easy\",\n      \"question\": \"What is the first book of the Old Testament?\",\n      \"correct_answer\": \"Genesis\",\n      \"incorrect_answers\": [\n        \"Exodus\",\n        \"Leviticus\",\n        \"Numbers\"\n      ]\n    },\n    {\n      \"category\": \"Vehicles\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"easy\",\n      \"question\": \"Which of the following car manufacturers had a war named after it?\",\n      \"correct_answer\": \"Toyota\",\n      \"incorrect_answers\": [\n        \"Honda\",\n        \"Ford\",\n        \"Volkswagen\"\n      ]\n    },\n    {\n      \"category\": \"Entertainment: Film\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"easy\",\n      \"question\": \"In the movie &quot;Spaceballs&quot;, what are the Spaceballs attempting to steal from Planet Druidia?\",\n      \"correct_answer\": \"Air\",\n      \"incorrect_answers\": [\n        \"The Schwartz\",\n        \"Princess Lonestar\",\n        \"Meatballs\"\n      ]\n    },\n    {\n      \"category\": \"Entertainment: Music\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"easy\",\n      \"question\": \"Which band recorded the album &quot;Parallel Lines&quot;?\",\n      \"correct_answer\": \"Blondie\",\n      \"incorrect_answers\": [\n        \"Paramore\",\n        \"Coldplay\",\n        \"The Police\"\n      ]\n    },\n    {\n      \"category\": \"History\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"easy\",\n      \"question\": \"On what street did the 1666 Great Fire of London start?\",\n      \"correct_answer\": \"Pudding Lane\",\n      \"incorrect_answers\": [\n        \"Baker Street\",\n        \"Houses of Parliament\",\n        \"St Paul&#039;s Cathedral\"\n      ]\n    },\n    {\n      \"category\": \"Entertainment: Video Games\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"medium\",\n      \"question\": \"If you play the Super Mario RPG and nap in a rented hotel room, you will wake up next to what familiar looking character?\",\n      \"correct_answer\": \"Link\",\n      \"incorrect_answers\": [\n        \"Wario\",\n        \"Q*bert\",\n        \"Solid Snake\"\n      ]\n    },\n    {\n      \"category\": \"Geography\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"medium\",\n      \"question\": \"In the 2016 Global Peace Index poll, out of 163 countries, what was the United States of America ranked?\",\n      \"correct_answer\": \"103\",\n      \"incorrect_answers\": [\n        \"10\",\n        \"59\",\n        \"79\"\n      ]\n    },\n    {\n      \"category\": \"Celebrities\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"medium\",\n      \"question\": \"In which of these TV shows did the chef Gordon Ramsay not appear?\",\n      \"correct_answer\": \"Auction Hunters\",\n      \"incorrect_answers\": [\n        \"Ramsay&#039;s Kitchen Nightmares\",\n        \"Hotel Hell\",\n        \"Hell&#039;s Kitchen\"\n      ]\n    },\n    {\n      \"category\": \"Entertainment: Film\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"medium\",\n      \"question\": \"Which actor plays the character &quot;Tommy Jarvis&quot; in &quot;Friday the 13th: The Final Chapter&quot; (1984)?\",\n      \"correct_answer\": \"Corey Feldman\",\n      \"incorrect_answers\": [\n        \"Macaulay Culkin\",\n        \"Mel Gibson\",\n        \"Mark Hamill\"\n      ]\n    },\n    {\n      \"category\": \"Celebrities\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"medium\",\n      \"question\": \"What year was O.J. Simpson aquitted of his murder charges?\",\n      \"correct_answer\": \"1995\",\n      \"incorrect_answers\": [\n        \"1992\",\n        \"1996\",\n        \"1991\"\n      ]\n    },\n    {\n      \"category\": \"Entertainment: Video Games\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"hard\",\n      \"question\": \"Which of these characters in &quot;Undertale&quot; can the player NOT go on a date with?\",\n      \"correct_answer\": \"Toriel\",\n      \"incorrect_answers\": [\n        \"Papyrus\",\n        \"Undyne\",\n        \"Alphys\"\n      ]\n    },\n    {\n      \"category\": \"Animals\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"hard\",\n      \"question\": \"What scientific family does the Aardwolf belong to?\",\n      \"correct_answer\": \"Hyaenidae\",\n      \"incorrect_answers\": [\n        \"Canidae\",\n        \"Felidae\",\n        \"Eupleridae\"\n      ]\n    },\n    {\n      \"category\": \"Politics\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"hard\",\n      \"question\": \"&quot;Yes, America Can!&quot; was this United States politician&#039;s de facto campaign slogan in 2004.\",\n      \"correct_answer\": \"George W. Bush\",\n      \"incorrect_answers\": [\n        \"John Kerry\",\n        \"Barack Obama\",\n        \"Al Gore\"\n      ]\n    },\n    {\n      \"category\": \"Entertainment: Music\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"hard\",\n      \"question\": \"Which of Michael Jackson&#039;s albums sold the most copies?\",\n      \"correct_answer\": \"Thriller\",\n      \"incorrect_answers\": [\n        \"Dangerous\",\n        \"Bad\",\n        \"Off the Wall\"\n      ]\n    },\n    {\n      \"category\": \"General Knowledge\",\n      \"type\": \"multiple\",\n      \"difficulty\": \"hard\",\n      \"question\": \"Originally another word for poppy, coquelicot is a shade of what?\",\n      \"correct_answer\": \"Red\",\n      \"incorrect_answers\": [\n        \"Green\",\n        \"Blue\",\n        \"Pink\"\n      ]\n    }\n  ]\n}";
        Questions = JsonUtility.FromJson<QuestionsClass>(json).results;
    }
    
    [Serializable]
    public class QuestionsClass
    {
        public List<Question> results;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        LoadJson();
        // foreach (var question in Questions)
        // {
        //     Debug.Log(question.question);
        // }
    }
}
