using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;

[RequireComponent(typeof(TextMesh))]
public class Annotation : MonoBehaviour, IFocusable {
	// Orientation of label w.r.t Registered Object
	public enum Orientation { LEFT, RIGHT, TOP, BOTTOM }
	public Orientation orientation;

	// This object's registration this annotation belongs to
	public ObjectRegistration registration = null;

	private TextMesh textMesh = null;

	// Gaze Interaction
	private bool isFocusedOn = false;
	private bool isFocusable = true;
	private float focusConfirmTimer = 0.0f;

	public string text {
		get { return textMesh.text; }
		set { textMesh.text = value; }
	}

	public bool renderEnabled {
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

	void Awake () {
		textMesh = GetComponent<TextMesh>();
		textMesh.color = Color.white;
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
				// registration.ConfirmLabel(this.text);				
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
}
