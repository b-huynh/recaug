using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prediction {
	public string label;
	public Vector3 position;
	public Prediction(string name, float x, float y, float z) {
		label = name;
		position = new Vector3(x, y, z);
	}
}

public class FramePredictions {
	public string timestamp;
	public List<Prediction> predictions = new List<Prediction> ();

	public FramePredictions(string ts) {
		this.timestamp = ts;
	}
}