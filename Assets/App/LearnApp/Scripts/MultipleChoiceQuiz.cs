using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Recaug;

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

    // Very very bad. Only used for stats tracking. Should remove soon if possible;
    public ObjectRegistration registration = null;
    public App parentApp = null;

    public int answer;
    public List<GameObject> responses = new List<GameObject>();
    public Dictionary<int, string> gtResponses = new Dictionary<int, string>();
    public event Action OnCorrectResponse = delegate {};
    public event Action<string> OnResponse = delegate {};

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

    public void SetOption(int option, string value,
        Translator.TargetLanguage targetLanguage = Translator.TargetLanguage.English)
    {
        GameObject response = responses[option];

        gtResponses[option] = value;

        string displayValue = value;
        if (targetLanguage != Translator.TargetLanguage.English)
        {
            displayValue = Translator.Translate(value, targetLanguage);
        }
        response.GetComponentInChildren<TextMesh>().text = displayValue;
    }

    public void SetAnswer(int option)
    {
        answer = option;
    }

    public void SetObject(ObjectRegistration registration)
    {
        this.registration = registration;
    }

    private void OnSelect(int idx)
    {
        // Log quiz response
        StatsTracker.Instance.LogQuiz(registration.className, gtResponses[idx]);

        if (idx == answer)
        {
            // Log correct quiz response as activity completion;
            StatsTracker.Instance.LogActivity(registration.className,
                parentApp.appName, gtResponses[idx]);

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
