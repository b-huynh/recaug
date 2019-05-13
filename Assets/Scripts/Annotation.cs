using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.InputModule;

[RequireComponent(typeof(TextMesh))]
public class Annotation : MonoBehaviour, IFocusable {
	// Orientation of label w.r.t Registered Object
	public enum Orientation { LEFT, RIGHT, TOP, BOTTOM }
	public Orientation orientation;

	// The Registered Object this annotation belongs to
	public RegisteredObject registeredObject = null;
	public TextMesh textMesh = null;

	// Gaze Interaction
	private bool isFocusedOn = false;
	private bool isFocusable = true;
	private float focusConfirmTimer = 0.0f;

	public string text {
		get { return textMesh.text; }
		set { textMesh.text = value; }
	}

	void Awake () {
		textMesh = GetComponent<TextMesh>();
		textMesh.color = Color.white;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (isFocusable && isFocusedOn) {
			focusConfirmTimer -= Time.deltaTime;
			if (focusConfirmTimer <= 0.0f) {
				// Confirm label of registered object to this one
				Debug.Log("Trying to confirm: " + text);
				registeredObject.ConfirmLabel(orientation);
				isFocusable = false;
			}
		}
	}

	public void OnFocusEnter() {
		if (isFocusable) {
			Debug.Log("Entered focus for: " + textMesh.text);
			textMesh.color = Color.green;
			isFocusedOn = true;
			focusConfirmTimer = Config.UIParams.FocusConfirmTime;
		}
	}

	public void OnFocusExit() {
		if (isFocusable) {
			Debug.Log("Exited focus for: " + textMesh.text);
			textMesh.color = Color.white;
			isFocusedOn = false;
			focusConfirmTimer = 0.0f;
		}
	}
}
