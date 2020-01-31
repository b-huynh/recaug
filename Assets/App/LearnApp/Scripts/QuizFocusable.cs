using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuizFocusable : MonoBehaviour, IFocusable
{
    public Color focusColor; // Color to display when focused on
    private Color defaultColor;

	public float selectDuration = 0.0f; // Focus-to-select duration, 0.0f to disable.
    private float selectTimer = 0.0f;
    private static QuizFocusable current;

    public int responseID;
    public event Action<int> OnSelect = delegate {};
    
    // Start is called before the first frame update
    void Awake()
    {
        defaultColor = GetComponent<Renderer>().material.color;
    }

    void Start()
    {
        // GameManager.Instance.GetComponent<ClickHandler>().OnClick += OnClick;
        // GameManager.Instance.GetComponent<ClickHandler>().ClickHandlers.Add(
            // gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (current == this)
        {
            // if (Input.GetKeyDown(KeyCode.Space))
            // if (Input.GetButtonDown("Submit"))
            // {
            //     OnSelect(responseID);
            // }

            if (GameManager.Instance.Clicked)
            {
                OnSelect(responseID);
            }

            if (selectDuration > 0.0f)
            {
                selectTimer += Time.deltaTime;
                if (selectTimer >= selectDuration)
                {
                    selectTimer = 0.0f;
                    OnSelect(responseID);
                }
            }
        }
    }

    public void OnClick()
    {
        if (current == this)
        {
            OnSelect(responseID);
        }
    }

    public void OnFocusEnter()
	{
        current = this;
        GetComponent<Renderer>().material.color = Color.blue;
	}

	public void OnFocusExit()
	{
        current = null;
        GetComponent<Renderer>().material.color = defaultColor;
        selectTimer = 0.0f;
	}
}
