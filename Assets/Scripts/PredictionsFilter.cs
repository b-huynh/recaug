using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Filters world predictions to determine where objects are in the scene.
public abstract class WPFilter {
    protected ObjectMemory omem = null;
    protected HashSet<string> excludeSet = null;
    public WPFilter(ObjectMemory omem) {
        this.omem = omem;
        this.excludeSet = new HashSet<string>();
    }

    public virtual void AddPredictions(WorldPredictions wp) {}

    public void ExcludeObjects(IEnumerable<string> toExclude) {
        excludeSet.UnionWith(toExclude);
    }

    protected void FilterExcluded(WorldPredictions wp) {
        wp.RemoveAll(excludeSet);
    }
}

// Naive filter assumes all predictions are correct
public class NaiveWPFilter : WPFilter {
    public NaiveWPFilter(ObjectMemory omem)
    : base(omem) {
        this.omem = omem;
    }

    public override void AddPredictions(WorldPredictions wp) {
        FilterExcluded(wp);

        // All predictions are correct
        foreach(WorldPrediction p in wp.predictions) {
            omem.RegisterObject(p.label, p.position);
        }
    }
}

public class SlidingWindowWPFilter : WPFilter {
    private int window, count;
    private float mindist;
	private class ObjectCandidate {
		public int pointCount = 0;
		public int numFramesPassed = 0;
		public string label;
		public List<string> altLabels;
		public Vector3 position;
		public ObjectCandidate(string label, Vector3 position) {
			this.label = label;
			this.position = position;
		}
	}
	private List<ObjectCandidate> candidates = new List<ObjectCandidate>();
	private List<GameObject> convergedPoints = new List<GameObject>();

    public SlidingWindowWPFilter(ObjectMemory omem, int window = 120, int count = 5, 
        float mindist = 0.2f)
    : base(omem) {
        this.window = window;
        this.count = count;
        this.mindist = mindist;
    }

    public override void AddPredictions(WorldPredictions wp) {
        FilterExcluded(wp);

		foreach(WorldPrediction p in wp.predictions) {
			bool matchFound = false;
			// Attempt to match valid objects first
			foreach(GameObject cp in convergedPoints) {
				if (Vector3.Distance(cp.transform.position, p.position) <= mindist) {
					RegisteredObject reg = cp.GetComponent<RegisteredObject>();
					if (reg.ContainsLabel(p.label)) {
						matchFound = true;
					} else {
						reg.AddAltLabel(p.label);
					}
					break;
				}
			}
			if (matchFound)
				break;

			// Now attempt candidates
			foreach(ObjectCandidate oc in candidates) {
				if (oc.label == p.label && Vector3.Distance(p.position, oc.position) <= mindist) {
					matchFound = true;
					oc.position = (oc.position + p.position) / 2.0f;
					oc.pointCount++;
					if (oc.pointCount >= count) {
                        GameObject newObj = omem.RegisterObject(oc.label, oc.position);
                        if (newObj != null) {
                            convergedPoints.Add(newObj);
                        }
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
		candidates.RemoveAll(c => c.pointCount >= count);
		candidates.RemoveAll(c => c.numFramesPassed >= window);
    }
}