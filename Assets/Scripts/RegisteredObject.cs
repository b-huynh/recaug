﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegisteredObject : MonoBehaviour {
	public int ID;
	public List<Annotation> annotations;
	private Dictionary<Annotation.Orientation, Annotation> assignMap;

	// Defines the geometry of this object...
	private struct Geometry {
		public List<Vector3> points;
		public List<GameObject> worldObjects;
	}
	private Geometry geometry;

	public bool confirmed { get; private set; }
	public string label { get; private set; }

	void Awake () {
		assignMap = new Dictionary<Annotation.Orientation, Annotation>();
		geometry = new Geometry();
		geometry.points = new List<Vector3>();
		geometry.worldObjects = new List<GameObject>();
	}

    void Start() {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool ContainsLabel(string label) {
		// Tests if l is any of the alt labels encompassed by this object
		// return altLabels.Any(str => str == label);
		if (confirmed) {
			return this.label == label;
		}
		return assignMap.Any(kv => kv.Value.text == label);
	}

	public void ConfirmLabel(Annotation.Orientation orientation) {
		// Swap with right annotation
		if (orientation != Annotation.Orientation.RIGHT) {
			string temp = assignMap[orientation].text;
			assignMap[orientation].text = 
				assignMap[Annotation.Orientation.RIGHT].text;
			assignMap[Annotation.Orientation.RIGHT].text = temp;
		}

		// Confirm label
		label = assignMap[Annotation.Orientation.RIGHT].text;
		
		// Translate and hide others
		assignMap[Annotation.Orientation.RIGHT].text =
			Translator.translate(label, Config.UIParams.TargetLanguage);

		foreach(var kv in assignMap) {
			if (kv.Key != Annotation.Orientation.RIGHT) {
				kv.Value.gameObject.SetActive(false);
			}
		}

		confirmed = true;
	}

	public void Init(int ID, string label) {
		this.ID = ID;
		AddAltLabel(label);
	}

	public void AddAltLabel(string label) {
		if (annotations.Count > 0) {
			Annotation ann = annotations[0];
			ann.gameObject.SetActive(true);
			ann.text = label;
			assignMap[ann.orientation] = ann;
			annotations.RemoveAt(0);
		}
	}

	public void UpdateGeometry(Vector3 point) {
		geometry.points.Add(point);
	}

	public void UpdateGeometry(GameObject worldObject) {
		geometry.worldObjects.Add(worldObject);
	}

	public bool ContainsGeometry(GameObject toCompare) {
		int toCompareID = toCompare.GetInstanceID();
		return geometry.worldObjects.Any(
			geo => geo.gameObject.GetInstanceID() == toCompareID); 
	}

}
