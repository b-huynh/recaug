using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Object tracker. Determines if an object has moved or not...
public class ObjectTracker {
    private ObjectMemory omem;
    private float minDist;
    private int maxCount;
    private Dictionary<string, List<float>> objDiffs;

    public ObjectTracker(ObjectMemory omem, float minDist = 0.5f, 
        int maxCount = 3) {
        this.omem = omem;
        this.minDist = minDist;
        this.maxCount = maxCount;
        this.objDiffs = new Dictionary<string, List<float>>();
    }

    // Given the next set of world predictions, return list of objects that have
    // moved or not.
    public List<string> TrackObjects(WorldPredictions worldPreds) {
        List<string> movedObjects = new List<string>();
        foreach(WorldPrediction wp in worldPreds.predictions) {
            GameObject currObj = omem.GetConfirmedObject(wp.label);
            if (currObj != null) {
                // Check if prediction is very far from confirmed
                float dist = Vector3.Distance(
                    wp.position, currObj.transform.position);
                if (dist >= this.minDist) {
                    if (!objDiffs.ContainsKey(wp.label)) {
                        objDiffs[wp.label] = new List<float>();
                    }
                    // Object may have moved
                    this.objDiffs[wp.label].Add(dist);

                    Debug.LogFormat("Predicted: {0}", wp.position);
                    Debug.LogFormat("Confirmed: {0}", currObj.transform.position);
                    string divergences = "[";
                    foreach(float divergence in this.objDiffs[wp.label]) {
                        divergences += divergence.ToString() + ", ";
                    }
                    divergences += "]";
                    Debug.LogFormat(" Object Divergence: {0}, {1}", wp.label, divergences);

                    if (this.objDiffs[wp.label].Count > this.maxCount) {
                        // Object has moved
                        movedObjects.Add(wp.label);
                        this.objDiffs[wp.label].Clear();
                    }
                }
            }
        }
        return movedObjects;
    }
}