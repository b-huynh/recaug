using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIFocusable : MonoBehaviour, IFocusable
{
    // public Color focusColor; // Color to display when focused on
    // private Color defaultColor;

	public float selectDuration = 0.0f; // Focus-to-select duration, 0.0f to disable.
    private float selectTimer = 0.0f;
    public static UIFocusable current;
    public event Action<GameObject> OnSelect = delegate {};
    
    // Start is called before the first frame update
    void Awake()
    {
        // defaultColor = GetComponent<Renderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (current == this)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnSelect(gameObject);
            }

            if (selectDuration > 0.0f)
            {
                selectTimer += Time.deltaTime;
                if (selectTimer >= selectDuration)
                {
                    selectTimer = 0.0f;
                    OnSelect(gameObject);
                }
            }
        }
    }

    public void OnFocusEnter()
	{
        current = this;
        // GetComponent<Renderer>().material.color = Color.blue;
	}

	public void OnFocusExit()
	{
        current = null;
        // GetComponent<Renderer>().material.color = defaultColor;
        selectTimer = 0.0f;
	}
}
