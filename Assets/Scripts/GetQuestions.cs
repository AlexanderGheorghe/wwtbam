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
        using (StreamReader r = new StreamReader("Assets/questions.json"))
        {
            string json = r.ReadToEnd();
            Questions = JsonUtility.FromJson<QuestionsClass>(json).results;
        }
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
