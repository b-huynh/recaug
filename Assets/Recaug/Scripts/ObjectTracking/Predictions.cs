using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Recaug
{
	[System.Serializable]
	public class Prediction
	{
		public string className;
		public float confidence;
		public Prediction(string className, float confidence)
		{
			this.className = className;
			this.confidence = confidence;
		}
	}

	[System.Serializable]
	public class PredPoint2D : Prediction
	{
		public int x, y;
		public float r, g, b;

		public Vector2 position
		{
			get { return new Vector2(x, y); }
			set
			{
				this.x = (int)value.x; 
				this.y = (int)value.y;
			}
		}

		public PredPoint2D(string className, float conf, Vector2 p,
			Color c = new Color())
		: base(className, conf)
		{
			this.x = (int)p.x;
			this.y = (int)p.y;
			this.r = c.r;
			this.g = c.g;
			this.b = c.b;
		}

		public PredPoint2D(Prediction pred, Vector2 p, Color c = new Color())
		: base(pred.className, pred.confidence)
		{
			this.x = (int)p.x;
			this.y = (int)p.y;
			this.r = c.r;
			this.g = c.g;
			this.b = c.b;
		}
	}

    /*
        A single prediction from an image
     */
    [System.Serializable]
	public class PredBox2D : Prediction
	{
        public int xmin, xmax, ymin, ymax, xcen, ycen;
        public float cen_r, cen_g, cen_b;

		public PredBox2D(string className, float conf, Vector2 p1, Vector2 p2,
			Vector2 cen, Color col = new Color())
		: base(className, conf)
		{
			this.xmin = (int)p1.x;
			this.ymin = (int)p1.y;
			this.xmax = (int)p2.x;
			this.ymax = (int)p2.y;
			this.xcen = (int)cen.x;
			this.ycen = (int)cen.y;
			this.cen_r = col.r;
			this.cen_g = col.g;
			this.cen_b = col.b;
		}

		public PredBox2D(Prediction pred, Vector2 p1, Vector2 p2,
			Vector2 cen, Color col = new Color())
		: base(pred.className, pred.confidence)
		{
			this.xmin = (int)p1.x;
			this.ymin = (int)p1.y;
			this.xmax = (int)p2.x;
			this.ymax = (int)p2.y;
			this.xcen = (int)cen.x;
			this.ycen = (int)cen.y;
			this.cen_r = col.r;
			this.cen_g = col.g;
			this.cen_b = col.b;
		}

	}

	[System.Serializable]
	public class PredPoint3D : Prediction
	{
		public float x, y, z, r, g, b;
		
		public Vector3 position
		{
			get { return new Vector3(x, y, z); }
			set
			{
				this.x = value.x; 
				this.y = value.y;
				this.z = value.z;
			}
		}

		public PredPoint3D(string className, float conf, Vector3 p,
			Color c = new Color())
		: base(className, conf)
		{
			this.x = p.x;
			this.y = p.y;
			this.z = p.z;
			this.r = c.r;
			this.g = c.g;
			this.b = c.b;
		}

		public PredPoint3D(Prediction pred, Vector3 p, Color c = new Color())
		: base(pred.className, pred.confidence)
		{
			this.x = p.x;
			this.y = p.y;
			this.z = p.z;
			this.r = c.r;
			this.g = c.g;
			this.b = c.b;
		}
	}

	// [System.Serializable]
	// public class PredBox3D : Prediction
	// {
	// 	public float xmin, ymin, zmin, xmax, ymax, zmax;
	// }


	// Predictions in Image space, deserializable from JSON server messages.
	// [System.Serializable]
	// public class ImagePredictions
	// {
	// 	public List<ImagePrediction> labels;
	// 	public List<WorldPoint> projected;
	// }

	// // A single world prediction
	// public class WorldPrediction {
	// 	public string label;
	// 	public Vector3 position;
	// 	public Color color;
	// 	public GameObject worldObject;
	// 	public WorldPrediction(string name, Vector3 pos, GameObject worldObject) {
	// 		this.label = name;
	// 		this.position = pos;
	// 		this.worldObject = worldObject;
	// 	}
	// }

	// // Collection of world predictions from a particular Image (by timestamp).
	// // Do not create yourself, use ImageToWorldProjector
	// public class WorldPredictions {
	// 	public string timestamp;
	// 	public List<WorldPrediction> predictions = new List<WorldPrediction> ();

	// 	public WorldPredictions(string ts) {
	// 		this.timestamp = ts;
	// 	}

	// 	public void Add(WorldPrediction wp) {
	// 		predictions.Add(wp);
	// 	}

	// 	public void RemoveAll(HashSet<string> toRemove) {
	// 		predictions.RemoveAll(item => toRemove.Contains(item.label));
	// 	}
	// }

	public class ImageToWorldProjector {
		private LayerMask hitmask;
		private HoloLensCameraStream.Resolution resolution;
		private HashSet<string> toExclude;
		float delta; // Shift point towards user by a small factor

		public ImageToWorldProjector (LayerMask hitmask,
			HoloLensCameraStream.Resolution resolution, float delta = 0.03f)
		{
			this.hitmask = hitmask;
			this.resolution = resolution;
			this.delta = delta;
			toExclude = new HashSet<string>();
		}

		public void ExcludeObjects(IEnumerable<string> toExclude)
		{
			this.toExclude.UnionWith(toExclude);
		}

		public bool ToWorld(ref Matrix4x4 camMat, ref Matrix4x4 projMat,
			Vector2 point2D, out Vector3 point3D)
		{
			Vector3 from = camMat.GetColumn(3);
			Vector3 rayDirection = LocatableCameraUtils.PixelCoordToWorldCoord(
				camMat, projMat, resolution, point2D);
			RaycastHit hitInfo;
			bool isHit = Physics.Raycast(from, rayDirection, out hitInfo,
				Mathf.Infinity, hitmask);
			point3D = new Vector3();
			if (isHit)
			{
				Vector3 towardsUser = (Camera.main.transform.position - hitInfo.point).normalized;
				point3D = hitInfo.point + (towardsUser * delta);
			}
			return isHit;
		}

		public bool ToWorld(ref Matrix4x4 camMat, ref Matrix4x4 projMat,
			List<PredPoint2D> points2D, out List<PredPoint3D> points3D)
		{
			points3D = new List<PredPoint3D>();
			var filtered = points2D.Where(p => toExclude.Contains(p.className));
			foreach(PredPoint2D p in filtered)
			{
				Vector3 point3D;
				bool isHit = ToWorld(ref camMat, ref projMat,
					new Vector2(p.x, p.y), out point3D);
				if (isHit)
				{
					points3D.Add(
						new PredPoint3D(p, point3D, new Color(p.r, p.g, p.b)));
				}
			}
			return points3D.Count != 0;
		}

		// public bool ToWorldPoint(ref Matrix4x4 camera2World,
		// 	ref Matrix4x4 projection, Vector2 imgPoint, out Vector3 worldPoint)
		// {
		// 	Vector3 from = camera2World.GetColumn(3);
		// 	Vector3 rayDirection = LocatableCameraUtils.PixelCoordToWorldCoord(
		// 		camera2World, projection, resolution, imgPoint);
		// 	RaycastHit hitInfo;
		// 	bool isHit = Physics.Raycast(from, rayDirection, out hitInfo,
		// 		Mathf.Infinity, hitmask);
		// 	worldPoint = new Vector3();
		// 	if (isHit) {
		// 		Vector3 towardsUser = (Camera.main.transform.position - hitInfo.point).normalized;
		// 		worldPoint = hitInfo.point + (towardsUser * delta);
		// 	}
		// 	return isHit;
		// }

		// public List<PredPoint3D> ToWorldPredictions(ref Matrix4x4 camera2World,
		// 	ref Matrix4x4 projection, List<PredPoint2D> imagePredictions)
		// {
		// 	string ts = System.DateTime.Now.Ticks.ToString();
		// 	WorldPredictions worldPreds = new WorldPredictions(ts);
		// 	foreach(ImagePrediction p in imgPreds.labels) {
		// 		if (toExclude.Contains(p.className)) {
		// 			continue;
		// 		}
		// 		Vector2 imgPoint = new Vector2(p.xcen, p.ycen);
		// 		Vector3 worldPoint;
		// 		GameObject worldObject = ToWorldSurface(ref camera2World,
		// 			ref projection, imgPoint, out worldPoint);
		// 		if (worldObject != null) {
		// 			var wp = new WorldPrediction(p.className, worldPoint, 
		// 				worldObject);
		// 			wp.color = new Color(p.cen_r, p.cen_g, p.cen_b);
		// 			worldPreds.Add(wp);
		// 		}
		// 	}
		// 	return worldPreds;
		// }

		// public WorldPredictions ToWorldPredictions(ref Matrix4x4 camera2World,
		// 	ref Matrix4x4 projection, ImagePredictions imgPreds)
		// { 
		// 	string ts = System.DateTime.Now.Ticks.ToString();
		// 	WorldPredictions worldPreds = new WorldPredictions(ts);
		// 	foreach(ImagePrediction p in imgPreds.labels) {
		// 		if (toExclude.Contains(p.className)) {
		// 			continue;
		// 		}
		// 		Vector2 imgPoint = new Vector2(p.xcen, p.ycen);
		// 		Vector3 worldPoint;
		// 		bool hasWorldPoint = ToWorldPoint(ref camera2World, ref projection,
		// 			imgPoint, out worldPoint);
		// 		if (hasWorldPoint) {
		// 			worldPreds.Add(new WorldPrediction(p.className, worldPoint));
		// 		}
		// 	}
		// 	return worldPreds;
		// }

		// public GameObject ToWorldSurface(ref Matrix4x4 camera2World,
		// 	ref Matrix4x4 projection, Vector2 imgPoint, out Vector3 worldPoint)
		// {
		// 	Vector3 from = camera2World.GetColumn(3);
		// 	Vector3 rayDirection = LocatableCameraUtils.PixelCoordToWorldCoord(
		// 		camera2World, projection, resolution, imgPoint);
		// 	RaycastHit hitInfo;
		// 	bool isHit = Physics.Raycast(from, rayDirection, out hitInfo,
		// 		Mathf.Infinity, hitmask);
		// 	worldPoint = new Vector3();
		// 	GameObject res = null;
		// 	if (isHit) {
		// 		Vector3 towardsUser = (Camera.main.transform.position - hitInfo.point).normalized;
		// 		worldPoint = hitInfo.point + (towardsUser * delta);
		// 		res = hitInfo.collider.gameObject;
		// 	}
		// 	return res;
		// }

		// public WorldPredictions ToWorldPredictions(ref Matrix4x4 camera2World,
		// 	ref Matrix4x4 projection, ImagePredictions imgPreds)
		// {
		// 	string ts = System.DateTime.Now.Ticks.ToString();
		// 	WorldPredictions worldPreds = new WorldPredictions(ts);
		// 	foreach(ImagePrediction p in imgPreds.labels) {
		// 		if (toExclude.Contains(p.className)) {
		// 			continue;
		// 		}
		// 		Vector2 imgPoint = new Vector2(p.xcen, p.ycen);
		// 		Vector3 worldPoint;
		// 		GameObject worldObject = ToWorldSurface(ref camera2World,
		// 			ref projection, imgPoint, out worldPoint);
		// 		if (worldObject != null) {
		// 			var wp = new WorldPrediction(p.className, worldPoint, 
		// 				worldObject);
		// 			wp.color = new Color(p.cen_r, p.cen_g, p.cen_b);
		// 			worldPreds.Add(wp);
		// 		}
		// 	}
		// 	return worldPreds;
		// }

	}
}