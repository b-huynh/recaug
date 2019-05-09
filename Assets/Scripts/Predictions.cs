using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A single prediction from an image
[System.Serializable]
public class ImagePrediction {
	public string className;
	public int xmin;
	public int xmax;
	public int ymin;
	public int ymax;
	public int xcen;
	public int ycen;
}

// Predictions in Image space, deserializable from JSON server messages.
[System.Serializable]
public class ImagePredictions {
	public List<ImagePrediction> labels;
}

// A single world prediction
public class WorldPrediction {
	public string label;
	public Vector3 position;
	public WorldPrediction(string name, Vector3 pos) {
		this.label = name;
		this.position = pos;
	}
}

// Collection of world predictions from a particular Image (by timestamp).
// Do not create yourself, use ImageToWorldProjector
public class WorldPredictions {
	public string timestamp;
	public List<WorldPrediction> predictions = new List<WorldPrediction> ();

	public WorldPredictions(string ts) {
		this.timestamp = ts;
	}

	public void Add(WorldPrediction wp) {
		predictions.Add(wp);
	}

	public void RemoveAll(HashSet<string> toRemove) {
		predictions.RemoveAll(item => toRemove.Contains(item.label));
	}
}

public class ImageToWorldProjector {
    private LayerMask hitmask;
    private HoloLensCameraStream.Resolution resolution;
	float delta; // Shift point towards user by a small factor

	public ImageToWorldProjector (LayerMask hitmask,
		HoloLensCameraStream.Resolution resolution, float delta = 0.03f)
	{
		this.hitmask = hitmask;
		this.resolution = resolution;
		this.delta = delta;
	}

	public bool ToWorldPoint(ref Matrix4x4 camera2World,
		ref Matrix4x4 projection, Vector2 imgPoint, out Vector3 worldPoint)
	{
		Vector3 from = camera2World.GetColumn(3);
		Vector3 rayDirection = LocatableCameraUtils.PixelCoordToWorldCoord(
			camera2World, projection, resolution, imgPoint);
		RaycastHit hitInfo;
		bool isHit = Physics.Raycast(from, rayDirection, out hitInfo,
			Mathf.Infinity, hitmask);
		worldPoint = new Vector3();
		if (isHit) {
			Vector3 towardsUser = (Camera.main.transform.position - hitInfo.point).normalized;
			worldPoint = hitInfo.point + (towardsUser * delta);
		}
		return isHit;
	}

	public WorldPredictions ToWorldPredictions(ref Matrix4x4 camera2World,
		ref Matrix4x4 projection, ImagePredictions imgPreds)
	{ 
		string ts = System.DateTime.Now.Ticks.ToString();
		WorldPredictions worldPreds = new WorldPredictions(ts);
		foreach(ImagePrediction p in imgPreds.labels) {
			Vector2 imgPoint = new Vector2(p.xcen, p.ycen);
			Vector3 worldPoint;
			bool hasWorldPoint = ToWorldPoint(ref camera2World, ref projection,
				imgPoint, out worldPoint);
			if (hasWorldPoint) {
				worldPreds.Add(new WorldPrediction(p.className, worldPoint));
			}
		}
		return worldPreds;
	}
}