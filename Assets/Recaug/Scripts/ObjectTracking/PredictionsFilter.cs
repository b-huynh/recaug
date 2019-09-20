using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Recaug
{
	// Filters world predictions to determine where objects are in the scene.
	public abstract class WPFilter
	{
		protected ObjectMemory omem = null;
		protected HashSet<string> excludeSet = null;
		public WPFilter(ObjectMemory omem)
		{
			this.omem = omem;
			this.excludeSet = new HashSet<string>();
		}

		public abstract List<PredPoint3D> Filter(List<PredPoint3D> wp);

		public void ExcludeObjects(IEnumerable<string> toExclude)
		{
			excludeSet.UnionWith(toExclude);
		}

		protected void FilterExcluded(List<PredPoint3D> wp)
		{
			wp.RemoveAll(p => excludeSet.Contains(p.className));
		}
	}

	// Naive filter assumes all predictions are correct
	public class NaiveWPFilter : WPFilter
	{
		public NaiveWPFilter(ObjectMemory omem)
		: base(omem)
		{
			this.omem = omem;
		}

		public override List<PredPoint3D> Filter(List<PredPoint3D> wp)
		{
			// All predictions are correct
			FilterExcluded(wp);
			return wp;
		}
	}

	public class SlidingWindowWPFilter : WPFilter
	{
		private int window, count;
		private float mindist;
		private class ObjectCandidate
		{
			public int pointCount = 0;
			public int numFramesPassed = 0;
			public string label;
			public Vector3 position;
			public ObjectCandidate(string label, Vector3 position)
			{
				this.label = label;
				this.position = position;
			}
		}
		private List<ObjectCandidate> candidates = new List<ObjectCandidate>();

		public SlidingWindowWPFilter(ObjectMemory omem, int window = 120,
			int count = 5, float mindist = 0.2f)
		: base(omem)
		{
			this.window = window;
			this.count = count;
			this.mindist = mindist;
		}

		private bool FuzzyMatchRegistered(PredPoint3D p)
		{
			var nearby = omem.GetNearbyObjects(p.position, mindist);
			return nearby.Count > 0;
		}

		private int FuzzyMatchCandidates(PredPoint3D p)
		{
			for (int i = 0; i < candidates.Count; ++i)
			{
				ObjectCandidate oc = candidates[i];
				if (oc.label == p.className && 
					Vector3.Distance(p.position, oc.position) <= mindist)
				{
					return i;
				}
			}
			return -1;
		}

		public override List<PredPoint3D> Filter(List<PredPoint3D> wp)
		{
			FilterExcluded(wp);

			List<PredPoint3D> result = new List<PredPoint3D>();
			foreach(PredPoint3D p in wp)
			{
				// Ignore prediction if this object is already registered.
				if (FuzzyMatchRegistered(p))
				{
					continue;
				}

				// Check if this prediction reinforces an existing candidate.
				int candidate_idx = FuzzyMatchCandidates(p);
				if (candidate_idx != -1)
				{
					var oc = candidates[candidate_idx];
					oc.position = (oc.position + p.position) / 2.0f;
					oc.pointCount++;
					if (oc.pointCount >= count)
					{
						// TODO: uncouple object registration from filtering!
						result.Add(new PredPoint3D(oc.label, 1.0f, oc.position));
						candidates.RemoveAt(candidate_idx);
					}
				}
				else // This prediction is a new, unknown object or candidate.
				{
					candidates.Add(new ObjectCandidate(p.className, p.position));
				}
			}

			for (int i = 0; i < candidates.Count; ++i)
			{
				candidates[i].numFramesPassed++;
			}
			candidates.RemoveAll(c => c.pointCount >= count);
			candidates.RemoveAll(c => c.numFramesPassed >= window);

			return result;
		}
	}
} // namespace Recaug