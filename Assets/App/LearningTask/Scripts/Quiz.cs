using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Quiz : MonoBehaviour
{
    public string question
    {
        // get 
        // {
        //     return transform.Find("QuestionPlane/Question").
        //         GetComponent<TextMesh>().text;
        // }
        set
        {
            transform.Find("QuestionPlane/Question").
                GetComponent<TextMesh>().text = value;
        }
    }

    public List<GameObject> responses = new List<GameObject>();

    public class QuizResponseEvent : UnityEvent<int> {}
    public QuizResponseEvent responseEvent = new QuizResponseEvent();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (QuizFocusable.current != null)
        {
            QuizFocusable.current.GetComponent<Renderer>().material.color = Color.blue;
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.LogFormat("Focused: {0}", QuizFocusable.current.name);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            int idx = responses.FindIndex(
                x => GameObject.ReferenceEquals(x, QuizFocusable.current));
            if (idx != -1)
            {
                responseEvent.Invoke(idx);
            }
        }
    }

    public void SetOption(int option, string value)
    {
        GameObject response = responses[option];
        response.GetComponentInChildren<TextMesh>().text = value;
    }
}
