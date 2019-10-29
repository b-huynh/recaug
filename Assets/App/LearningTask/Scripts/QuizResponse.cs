using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Recaug;

public class QuizResponse : MonoBehaviour, IFocusable
{
	// Orientation of label w.r.t Registered Object
	public enum QuizOption { A, B, C, D }
	public QuizOption response;

	private TextMesh textMesh;

	// Gaze Interaction
	private bool isFocusedOn = false;
	private bool isFocusable = true;
	private float focusConfirmTimer = 0.0f;



	public string text
    {
		get { return textMesh.text; }
		set { textMesh.text = value; }
	}

	public bool enableRenderer
    {
		get 
		{ 
			return GetComponent<MeshRenderer>().enabled && 
				   GetComponent<BoxCollider>().enabled;
		}
		set
		{
			GetComponent<MeshRenderer>().enabled = value;
			GetComponent<BoxCollider>().enabled = value;
		}
	}

	void Awake ()
    {
		textMesh = GetComponentInChildren<TextMesh>();
	}

	// Use this for initialization
	void Start () {
	}


	// Update is called once per frame
	void Update ()
	{
		if (isFocusable && isFocusedOn)
		{
			focusConfirmTimer -= Time.deltaTime;
			if (focusConfirmTimer <= 0.0f)
			{
				// Confirm label of registered object to this one
				isFocusable = false;
				// responseEvent.Invoke(this.text);			
			}
		}
	}

	public void OnFocusEnter()
	{
		if (isFocusable)
		{
			textMesh.color = Color.green;
			isFocusedOn = true;
			focusConfirmTimer = Config.UI.FocusConfirmTime;
		}
	}

	public void OnFocusExit()
	{
		textMesh.color = Color.white;
		if (isFocusable)
		{
			isFocusedOn = false;
			focusConfirmTimer = 0.0f;
		}
	}


    /* Eye Focus Selection */
	// // Update is called once per frame
	// void Update ()
	// {
	// 	if (isFocusable && isFocusedOn)
	// 	{
	// 		focusConfirmTimer -= Time.deltaTime;
	// 		if (focusConfirmTimer <= 0.0f)
	// 		{
	// 			// Confirm label of registered object to this one
	// 			isFocusable = false;
	// 			responseEvent.Invoke(this.text);			
	// 		}
	// 	}
	// }

	// public void OnFocusEnter()
	// {
	// 	if (isFocusable)
	// 	{
	// 		textMesh.color = Color.green;
	// 		isFocusedOn = true;
	// 		focusConfirmTimer = Config.UI.FocusConfirmTime;
	// 	}
	// }

	// public void OnFocusExit()
	// {
	// 	textMesh.color = Color.white;
	// 	if (isFocusable)
	// 	{
	// 		isFocusedOn = false;
	// 		focusConfirmTimer = 0.0f;
	// 	}
	// }
}
