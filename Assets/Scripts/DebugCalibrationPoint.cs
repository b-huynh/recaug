using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCalibrationPoint : MonoBehaviour {

	public Transform cameraHead = null;
	private TextMesh tm;

	// Use this for initialization
	void Start () {
		GameObject textObj = new GameObject("[DebugCalibrationPoint] Label");
		textObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		textObj.transform.SetParent(this.transform);
		tm = textObj.AddComponent<TextMesh>() as TextMesh;
	}
	
	// Update is called once per frame
	void Update () {
		// Update TextMesh
		tm.text = string.Format("Position: {0}\nScale: {1}",
			transform.position.ToString("F2"), transform.localScale.ToString("F2"));

		if (Input.GetKeyDown(KeyCode.R)) 
			transform.position = cameraHead.position; // Reset
		if (Input.GetKeyDown(KeyCode.PageUp))
			transform.localScale += new Vector3(0.02f, 0.02f, 0.02f); // Scale Up
		if (Input.GetKeyDown(KeyCode.PageDown)) 
			transform.localScale -= new Vector3(0.02f, 0.02f, 0.02f); // Scale Down
        float vertTrans = Input.GetAxis("Vertical") * 0.1f;
        float horizTrans = Input.GetAxis("Horizontal") * 0.1f;
        transform.Translate(horizTrans, 0, vertTrans);

		if (Input.GetKeyDown(KeyCode.Home))
			transform.Translate(0, 0.1f, 0);
		if (Input.GetKeyDown(KeyCode.End))
			transform.Translate(0, -0.1f, 0);

		if (Input.GetKeyDown(KeyCode.L))
			Debug.Log("[DebugCalibrationPoint] " + tm.text);
	}
}
