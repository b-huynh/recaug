using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectMemory {
	public GameObject pointPrefab = null;
	private bool displayPointPrefab = true;
	public List<GameObject> convergedPoints = new List<GameObject>();

	public class ObjectCandidate {
		public int pointCount = 0;
		public int numFramesPassed = 0;
		public string label;
		public Vector3 position;
		public ObjectCandidate(string label, Vector3 position) {
			this.label = label;
			this.position = position;
		}
	}
	private List<ObjectCandidate> candidates = new List<ObjectCandidate>();

	public void ConvergePointsPerFrame(FramePredictions fp) {
		int aggregateWindow = 60; // Use last N frames
		float distThresh = 0.2f;
		int countThresh = 20;

		foreach(Prediction p in fp.predictions) {
			bool matchFound = false;
			// Attempt to match valid objects first
			foreach(GameObject cp in convergedPoints) {
				if (Vector3.Distance(cp.transform.position, p.position) <= distThresh &&
					cp.GetComponentInChildren<TextMesh>().text == p.label) {
					matchFound = true;
					break;
				}
			}
			if (matchFound)
				break;

			// Now attempt candidates
			foreach(ObjectCandidate oc in candidates) {
				if (oc.label == p.label && Vector3.Distance(p.position, oc.position) <= distThresh) {
					matchFound = true;
					oc.position = (oc.position + p.position) / 2.0f;
					oc.pointCount++;
					if (oc.pointCount >= countThresh) {
						GameObject newValidObj = GameObject.Instantiate(pointPrefab);
						newValidObj.transform.position = oc.position;
						newValidObj.GetComponentInChildren<TextMesh>().text = oc.label;
						newValidObj.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
						newValidObj.SetActive(displayPointPrefab);
						convergedPoints.Add(newValidObj);
					}
					break;
				}
			}

			if (!matchFound) {
				candidates.Add(new ObjectCandidate(p.label, p.position));
			}
		}

		for (int i = 0; i < candidates.Count; ++i) {
			candidates[i].numFramesPassed++;
		}
		candidates.RemoveAll(c => c.pointCount >= countThresh);
		candidates.RemoveAll(c => c.numFramesPassed >= aggregateWindow);
	}

	public void SetDisplayPoints(bool val) {
		displayPointPrefab = val;
		foreach(GameObject g in convergedPoints) {
			g.SetActive(displayPointPrefab);
		}
	}
}
