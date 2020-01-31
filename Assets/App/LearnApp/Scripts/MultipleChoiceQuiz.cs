using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MultipleChoiceQuiz : MonoBehaviour
{
    public string question
    {
        get 
        {
            return transform.Find("QuestionPlane/Question").
                GetComponent<TextMesh>().text;
        }
        set
        {
            transform.Find("QuestionPlane/Question").
                GetComponent<TextMesh>().text = value;
        }
    }

    public int answer;
    public List<GameObject> responses = new List<GameObject>();
    public event Action OnCorrectResponse = delegate {};

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i <  responses.Count; ++i)
        {
            responses[i].GetComponent<QuizFocusable>().responseID = i;
            responses[i].GetComponent<QuizFocusable>().focusColor = Color.blue;
            responses[i].GetComponent<QuizFocusable>().OnSelect += OnSelect;
            responses[i].GetComponent<QuizFocusable>().selectDuration = 
                // Config.UI.FocusConfirmTime;
                0;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetOption(int option, string value)
    {
        GameObject response = responses[option];
        response.GetComponentInChildren<TextMesh>().text = value;
    }

    public void SetAnswer(int option)
    {
        answer = option;
    }

    private void OnSelect(int idx)
    {
        if (idx == answer)
        {
            // Do Correct Animation
            var animation = responses[idx].transform.Find(
                "Correct").GetComponent<FadeInAnimation>();
            animation.OnAnimationStop += delegate { OnCorrectResponse(); };
            animation.StartAnimation();
        }
        else
        {
            // Do Incorrect Animation
            var animation = responses[idx].transform.Find(
                "Incorrect").GetComponent<FadeInAnimation>();
            animation.StartAnimation();
        }
    }
}
