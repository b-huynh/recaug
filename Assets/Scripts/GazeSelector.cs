using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.InputModule;

public class GazeSelector : MonoBehaviour, IFocusable {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnFocusEnter() {
		Debug.Log("Entered focus for: " + GetComponent<TextMesh>().text);
		GetComponent<TextMesh>().color = Color.green;
	}

	public void OnFocusExit() {
		Debug.Log("Exited focus for: " + GetComponent<TextMesh>().text);
		GetComponent<TextMesh>().color = Color.white;
	}
}
