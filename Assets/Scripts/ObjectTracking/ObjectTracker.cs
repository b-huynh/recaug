using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Object tracker. Determines if an object has moved or not...
public class ObjectTracker {
    private ObjectMemory omem;
    private float minDist;
    private int maxCount;

    public class ObjectTrackingInfo {
        public string classname;
        public Vector3 registeredPosition;
        public List<Vector3> divergedPositions;
        public bool moved;
        public ObjectTrackingInfo(string classname, Vector3 currentPosition) {
            this.classname = classname;
            this.registeredPosition = currentPosition;
            this.divergedPositions = new List<Vector3>();
            this.moved = false;
        }

        public Vector3 averageDivergedPosition() {
            Vector3 sum = Vector3.zero;
            foreach(Vector3 pos in divergedPositions) {
                sum += pos;
            }
            return sum / divergedPositions.Count;
        }
    }
    private Dictionary<string, ObjectTrackingInfo> objDiffs;

    public ObjectTracker(ObjectMemory omem, float minDist = 0.4f, 
        int maxCount = 2)
    {
        this.omem = omem;
        this.minDist = minDist;
        this.maxCount = maxCount;
        this.objDiffs = new Dictionary<string, ObjectTrackingInfo>();
    }

    // Given the next set of world predictions, return list of objects that have
    // moved or not.
    public Dictionary<string, Vector3> TrackObjects(WorldPredictions worldPreds)
    {
        foreach(WorldPrediction wp in worldPreds.predictions) 
        {
            string classname = wp.label;
            if (!omem.IsConfirmed(classname))
                continue;

            var registration = omem.GetRegistration(classname);
            Vector3 registeredPosition = registration.position;
            Vector3 newPosition = wp.position;

            // Check if prediction is very far from confirmed
            float dist = Vector3.Distance(newPosition, registeredPosition);
            if (dist >= this.minDist)
            {
                // Create ObjectTrackingInfo if it's not tracked yet
                if (!objDiffs.ContainsKey(classname)) 
                {
                    objDiffs[classname] = new ObjectTrackingInfo(classname,
                        registeredPosition);
                }
                var trackingInfo = objDiffs[classname];

                // Add potentially diverged position
                trackingInfo.divergedPositions.Add(newPosition);

                // Determine if object has moved
                trackingInfo.moved = 
                    trackingInfo.divergedPositions.Count >= this.maxCount;
            }
        }

        var movedObjects = new Dictionary<string, Vector3>();
        foreach(var kv in objDiffs) 
        {
            var trackingInfo = objDiffs[kv.Key];
            if (trackingInfo.moved)
            {
                Vector3 newPosition = trackingInfo.averageDivergedPosition();
                movedObjects[kv.Key] = newPosition;
                Debug.LogFormat("Object Moved: {0}, {1}", kv.Key, newPosition);

                // Reset so it can track again
                trackingInfo.moved = false;
                trackingInfo.divergedPositions.Clear();
            }
        }
        return movedObjects;
    }
}