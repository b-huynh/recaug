using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.InputModule;

public class GazeSelector : MonoBehaviour, IFocusable {

	bool isFocusedOn = false;
	bool isFocusable = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (isFocusedOn && Input.GetKeyDown(KeyCode.Space)) {
			// Set parent label to this
			RegisteredObject ro = transform.parent.gameObject.GetComponent<RegisteredObject>();
			ro.SwitchLabel(GetComponent<TextMesh>().text);
			isFocusable = false;
		}
	}

	public void OnFocusEnter() {
		if (isFocusable) {
			Debug.Log("Entered focus for: " + GetComponent<TextMesh>().text);
			GetComponent<TextMesh>().color = Color.green;
			isFocusedOn = true;
		}
	}

	public void OnFocusExit() {
		if (isFocusable) {
			Debug.Log("Exited focus for: " + GetComponent<TextMesh>().text);
			GetComponent<TextMesh>().color = Color.white;
			isFocusedOn = false;
		}
	}
}
