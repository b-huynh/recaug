using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuizFocusable : MonoBehaviour, IFocusable
{
	public static GameObject current;
    private Color defaultColor;

    // Start is called before the first frame update
    void Awake()
    {
        defaultColor = GetComponent<Renderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnFocusEnter()
	{
		current = gameObject;
        GetComponent<Renderer>().material.color = Color.blue;
	}

	public void OnFocusExit()
	{
		current = null;
        GetComponent<Renderer>().material.color = defaultColor;
	}
}
