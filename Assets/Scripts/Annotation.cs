using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.InputModule;

public class Annotation : MonoBehaviour, IFocusable {
	// Orientation of label w.r.t Registered Object
	public enum Orientation { LEFT, RIGHT, TOP, BOTTOM }
	public Orientation orientation;

	// The Registered Object this annotation belongs to
	private RegisteredObject ro = null;

	// Gaze Interaction
	private bool isFocusedOn = false;
	private bool isFocusable = true;
	private float focusConfirmTimer = 0.0f;

	// Use this for initialization
	void Start () {
		GetComponent<TextMesh>().color = Color.white;
		RegisteredObject ro = 
			transform.parent.gameObject.GetComponent<RegisteredObject>();
	}
	
	// Update is called once per frame
	void Update () {
		if (isFocusedOn) {
			focusConfirmTimer -= Time.deltaTime;
			if (focusConfirmTimer <= 0.0f) {
				// Set parent label to this

				ro.SwitchLabel(GetComponent<TextMesh>().text);
				isFocusable = false;
			}
		}

		if (isFocusedOn && Input.GetKeyDown(KeyCode.Space)) {

		}
	}

	public void OnFocusEnter() {
		if (isFocusable) {
			Debug.Log("Entered focus for: " + GetComponent<TextMesh>().text);
			GetComponent<TextMesh>().color = Color.green;
			isFocusedOn = true;
			focusConfirmTimer = Config.UIParams.FocusConfirmTime;
		}
	}

	public void OnFocusExit() {
		if (isFocusable) {
			Debug.Log("Exited focus for: " + GetComponent<TextMesh>().text);
			GetComponent<TextMesh>().color = Color.white;
			isFocusedOn = false;
			focusConfirmTimer = 0.0f;
		}
	}
}
