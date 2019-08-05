using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

// ObjectRegistration contains all the info needed to represent a real object.
public class ObjectRegistration : MonoBehaviour {
	// Defines the geometry of this object...
	private struct Geometry {
		// public List<Vector3> points;
        public CircularBuffer<Vector3> points;
		public List<GameObject> worldObjects;
	}
	private Geometry geometry;
    public HashSet<string> possibleLabels;

	// public List<Annotation> annotations;
	// private Dictionary<Annotation.Orientation, Annotation> assignMap;

	public bool confirmed { get; private set; }
    private float confirmTimeout;
    public class ConfirmTimeoutEvent : UnityEvent<string> {}
    public ConfirmTimeoutEvent confirmTimeoutEvent;

	public string label { get; private set; }
    public int instanceID {
        get { return gameObject.GetInstanceID(); }
        private set {}
    }
	public Vector3 position {
		get { return transform.position; }
		private set {}
	}
    private float updatePoseTimer;

    // To register callbacks for when this object registration is "confirmed"
    // // by the user to be a certain object. Passes confirmed class to listener.
    public class LabelEvent : UnityEvent<string> {}
	public LabelEvent addLabelEvent;
	public LabelEvent removeLabelEvent;
    public LabelEvent confirmLabelEvent;

    public static HashSet<string> ConfirmedObjects = new HashSet<string>();

	void Awake () {
		// assignMap = new Dictionary<Annotation.Orientation, Annotation>();
		geometry = new Geometry();
		// geometry.points = new List<Vector3>();
        geometry.points = 
            new CircularBuffer<Vector3>(Config.System.GeometryCapacity);
		geometry.worldObjects = new List<GameObject>();
        possibleLabels = new HashSet<string>();
        
        addLabelEvent = new LabelEvent();
        removeLabelEvent = new LabelEvent();
        confirmLabelEvent = new LabelEvent();
        
        confirmed = false;
        confirmTimeout = Config.UI.ConfirmTimeout;
        confirmTimeoutEvent = new ConfirmTimeoutEvent();

        updatePoseTimer = Config.UI.PositionUpdateRate;
    }

    void Start() {
    }
	
	// Update is called once per frame
	void Update () {
        if (!confirmed)
        {
            if (confirmTimeout > 0.0f)
            {
                confirmTimeout -= Time.deltaTime;
                if (confirmTimeout <= 0.0f)
                {
                    DoConfirmTimeout();
                }
            }
        }

        UpdatePose();
	}

    public void UpdateRegistration(WorldPrediction p) {
        // Extract information that might describe current state of this object
        if (!confirmed)
        {
            AddLabel(p.label);
        }
        else
        {
            if (p.label == this.label)
            {
                UpdateGeometry(p.position);
            }
        }
    }

	// public void ConfirmLabel(Annotation.Orientation orientation) {
	// 	// Swap with right annotation
	// 	if (orientation != Annotation.Orientation.RIGHT) {
	// 		string temp = assignMap[orientation].text;
	// 		assignMap[orientation].text = 
	// 			assignMap[Annotation.Orientation.RIGHT].text;
	// 		assignMap[Annotation.Orientation.RIGHT].text = temp;
	// 	}

	// 	// Confirm label
	// 	this.label = assignMap[Annotation.Orientation.RIGHT].text;

	// 	// Translate and hide others
	// 	assignMap[Annotation.Orientation.RIGHT].text =
	// 		Translator.Translate(this.label, Config.Experiment.TargetLanguage);

	// 	foreach(var kv in assignMap) {
	// 		if (kv.Key != Annotation.Orientation.RIGHT) {
	// 			kv.Value.gameObject.SetActive(false);
	// 		}
	// 	}

	// 	this.confirmed = true;
    //     ObjectRegistration.ConfirmedObjects.Add(this.label);
    //     labelConfirmedEvent.Invoke(this.label);
	// }

	public void Init(string label, Vector3 position) {
        this.label = label;
        AddLabel(label);
        UpdateGeometry(position);
        DeterminePose();
    }

    public void Destroy()
    {
        if (this.confirmed)
        {
            ObjectRegistration.ConfirmedObjects.Remove(this.label);
        }
    }

    public void ConfirmLabel(string label) {
        if (!possibleLabels.Contains(label))
        {
            throw new System.ArgumentException(
                "Argument not in list of possible labels", "label");
        }
        this.label = label;
		this.confirmed = true;
        ObjectRegistration.ConfirmedObjects.Add(this.label);

        ConfirmLabelEvent e = new ConfirmLabelEvent();
        e.timestamp = Utils.UnixTimestampMilliseconds();
        e.label = this.label;
        e.x = this.position.x;
        e.y = this.position.y;
        e.z = this.position.z;
        HologramManager.Instance.confirm_log.Add(JsonUtility.ToJson(e));

        confirmLabelEvent.Invoke(this.label);
    }

	public void AddLabel(string label) {
        if (ObjectRegistration.ConfirmedObjects.Contains(label))
            return;

        possibleLabels.Add(label);
        addLabelEvent.Invoke(label);

		// if (annotations.Count > 0) {
		// 	Annotation ann = annotations[0];
		// 	ann.Enable();
		// 	ann.text = label;
		// 	assignMap[ann.orientation] = ann;
		// 	annotations.RemoveAt(0);
		// }
	}

    public void RemoveLabel(string label) {
        possibleLabels.Remove(label);
        removeLabelEvent.Invoke(label);

        // if (possibleLabels.Count <= 0)
        // {
        //     // If all possible labels are confirmed elsewhere, just destroy it.
        //     DoConfirmTimeout();
        // }

        // Debug.Log("Removing label... " + label);
        
        // if (annotations == null) {
        //     Debug.Log("Annotations is null...");
        // } else {
        //     Debug.Log("Annotations count... " + annotations.Count.ToString());
        // }

        // foreach(Annotation ann in annotations) {
        //     if (ann == null) {
        //         Debug.Log("Ann is null..");
        //     }
        //     Debug.Log("Ann text..." + ann.text);
        //     if (ann.text == label) {
        //         Debug.Log("Ann set text...");
        //         ann.text = "";
        //     }
        // }
    }

    public void DoConfirmTimeout() {
        // This object has not been confirmed by the user, get rid of it.
        DestroyImmediate(this.gameObject);
        confirmTimeoutEvent.Invoke(this.label);
    }

	public void UpdateGeometry(Vector3 point) {
		// geometry.points.Add(point);
        geometry.points.PushBack(point);
		// DeterminePose();
	}

	// public void UpdateGeometry(GameObject worldObject) {
	// 	geometry.worldObjects.Add(worldObject);
	// 	DeterminePose();
	// }

	// public void ClearGeometry() {
	// 	geometry.points.Clear();
	// 	geometry.worldObjects.Clear();
	// 	DeterminePose();
	// }

	// public bool ContainsGeometry(GameObject toCompare) {
	// 	int toCompareID = toCompare.GetInstanceID();
	// 	return geometry.worldObjects.Any(
	// 		geo => geo.gameObject.GetInstanceID() == toCompareID); 
	// }

    private void UpdatePose()
    {
        if (this.confirmed)
        {
            updatePoseTimer -= Time.deltaTime;
            if (updatePoseTimer < 0.0)
            {
                DeterminePose();
                updatePoseTimer = Config.UI.PositionUpdateRate;
            }
        }
    }

	private void DeterminePose() {
		// Pose is the average position of all points / surfaces
		// List<Vector3> points = new List<Vector3>(geometry.points);
		// points.AddRange(geometry.worldObjects.Select(o => o.transform.position));

        int numPoints = geometry.points.Count();
        Vector3 newPosition = Vector3.zero;

        if (Config.UI.DeterminePoseMethod == "average")
        {
            Vector3 avg = Vector3.zero;
            foreach(Vector3 p in geometry.points)
            {
                avg += p;
            }
            if (numPoints > 0)
            {
                newPosition = avg / numPoints;
            } 
            else
            {
                newPosition = Vector3.zero;
            }
        }
        else if (Config.UI.DeterminePoseMethod == "median")
        {
            int medianTimeIdx = numPoints / 2;
            newPosition = geometry.points[medianTimeIdx];
            
            // n point averaging window around median
            int windowSize = Config.UI.MedianAverageWindow;
            if (numPoints > (windowSize * 2))
            {
                newPosition = Vector3.zero;
                for(int i = medianTimeIdx - windowSize;
                        i < medianTimeIdx + windowSize; ++i)
                {
                    newPosition += geometry.points[i];
                }
                newPosition = newPosition / (windowSize * 2);
            }

            Debug.LogFormat("Object {0}, NewPosition {1}", label, newPosition);
        }
        else if (Config.UI.DeterminePoseMethod == "latest")
        {
            newPosition = geometry.points[numPoints - 1];

            int windowSize = 10;
            if (numPoints > windowSize)
            {
                newPosition = Vector3.zero;
                for(int i = numPoints - windowSize; i < numPoints; ++i)
                {
                    newPosition += geometry.points[i];
                }
                newPosition = newPosition / windowSize;
            }
            Debug.LogFormat("Object {0}, NewPosition {1}", label, newPosition);
        }
        else
        {
            throw new System.Exception(
                "Config.UI.DeterminePoseMethod value invalid!");
        }

        transform.position = newPosition;
	}
}
