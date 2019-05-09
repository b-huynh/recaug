using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectMemory {
	public enum Modes { OVERWRITE, FIRST_IN }

	public GameObject objectPrefab = null;
	private Dictionary<string, GameObject> objects;
	public bool isActive { get; private set; } = true;
	public Modes mode { get; private set; }

	public ObjectMemory(GameObject objectPrefab, Modes mode = Modes.FIRST_IN) {
		this.objects = new Dictionary<string, GameObject>();
		this.objectPrefab = objectPrefab;
		this.mode = mode;
	}

	public GameObject RegisterObject(string name, Vector3 position) {
		if (mode == Modes.FIRST_IN && objects.ContainsKey(name)) {
			// Object already registered
			return null;
		}

		// Create new RegisteredObject
		GameObject newObj = GameObject.Instantiate(objectPrefab);
		newObj.transform.position = position;
		newObj.GetComponent<RegisteredObject>().AddAltLabel(name);
		newObj.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
		newObj.SetActive(isActive);
		
		objects[name] = newObj;
		return newObj;
	}

	public void SetActive(bool val) {
		isActive = val;
		foreach(var obj in objects) {
			obj.Value.SetActive(isActive);
		}
	}

	public void SetMode(Modes mode) {
		this.mode = mode;
	}

	// private bool displayPointPrefab = true;

	// public List<GameObject> convergedPoints = new List<GameObject>();

	// public class ObjectCandidate {
	// 	public int pointCount = 0;
	// 	public int numFramesPassed = 0;
	// 	public string label;
	// 	public List<string> altLabels;
	// 	public Vector3 position;
	// 	public ObjectCandidate(string label, Vector3 position) {
	// 		this.label = label;
	// 		this.position = position;
	// 	}
	// }
	// private List<ObjectCandidate> candidates = new List<ObjectCandidate>();

	// public void ConvergePointsPerFrame(FramePredictions fp) {
	// 	// int aggregateWindow = 60; // Use last N frames
	// 	int aggregateWindow = 120;
	// 	float distThresh = 0.2f;
	// 	// int countThresh = 20;
	// 	int countThresh = 5;

	// 	foreach(Prediction p in fp.predictions) {
	// 		bool matchFound = false;
	// 		// Attempt to match valid objects first
	// 		foreach(GameObject cp in convergedPoints) {
	// 			if (Vector3.Distance(cp.transform.position, p.position) <= distThresh) {
	// 				RegisteredObject reg = cp.GetComponent<RegisteredObject>();
	// 				if (reg.ContainsLabel(p.label)) {
	// 					matchFound = true;
	// 				} else {
	// 					reg.AddAltLabel(p.label);
	// 				}
	// 				break;
	// 				// if (cp.GetComponentInChildren<TextMesh>().text == p.label) {
	// 				// 	matchFound = true;
	// 				// 	break;
	// 				// }
	// 			}
	// 		}
	// 		if (matchFound)
	// 			break;

	// 		// Now attempt candidates
	// 		foreach(ObjectCandidate oc in candidates) {
	// 			if (oc.label == p.label && Vector3.Distance(p.position, oc.position) <= distThresh) {
	// 				matchFound = true;
	// 				oc.position = (oc.position + p.position) / 2.0f;
	// 				oc.pointCount++;
	// 				if (oc.pointCount >= countThresh) {
	// 					// Create new RegisteredObject
	// 					GameObject newValidObj = GameObject.Instantiate(pointPrefab);
	// 					newValidObj.transform.position = oc.position;
	// 					// newValidObj.GetComponentInChildren<TextMesh>().text = oc.label;
	// 					newValidObj.GetComponent<RegisteredObject>().AddAltLabel(oc.label);
	// 					newValidObj.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
	// 					newValidObj.SetActive(displayPointPrefab);
	// 					convergedPoints.Add(newValidObj);
    //                     Debug.Log("Converged Label: " + newValidObj.GetComponent<RegisteredObject>().label);
	// 				}
	// 				break;
	// 			}
	// 		}

	// 		if (!matchFound) {
	// 			candidates.Add(new ObjectCandidate(p.label, p.position));
	// 		}
	// 	}

	// 	for (int i = 0; i < candidates.Count; ++i) {
	// 		candidates[i].numFramesPassed++;
	// 	}
	// 	candidates.RemoveAll(c => c.pointCount >= countThresh);
	// 	candidates.RemoveAll(c => c.numFramesPassed >= aggregateWindow);
	// }

	// public void SetDisplayPoints(bool val) {
	// 	displayPointPrefab = val;
	// 	foreach(GameObject g in convergedPoints) {
	// 		g.SetActive(displayPointPrefab);
	// 	}
	// }
}
