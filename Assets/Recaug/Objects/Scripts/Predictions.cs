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

    [System.Serializable]
	public class PredBox3D : Prediction
	{
        public int xmin, ymin, zmin;
		public float xmax, ymax, zmax;
		public float xcen, ycen, zcen;

		public PredBox3D(string className, float conf, Vector3 p1, Vector3 p2,
			Vector3 cen)
		: base(className, conf)
		{
			this.xmin = (int)p1.x;
			this.ymin = (int)p1.y;
			this.zmin = (int)p1.z;

			this.xmax = (int)p2.x;
			this.ymax = (int)p2.y;
			this.zmax = (int)p2.z;

			this.xcen = (int)cen.x;
			this.ycen = (int)cen.y;
			this.zcen = (int)cen.z;
		}

		public PredBox3D(Prediction pred, Vector3 p1, Vector3 p2,
			Vector3 cen)
		: base(pred.className, pred.confidence)
		{
			this.xmin = (int)p1.x;
			this.ymin = (int)p1.y;
			this.zmin = (int)p1.z;

			this.xmax = (int)p2.x;
			this.ymax = (int)p2.y;
			this.zmax = (int)p2.z;

			this.xcen = (int)cen.x;
			this.ycen = (int)cen.y;
			this.zcen = (int)cen.z;
		}
	}

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
			// var filtered = points2D.Where(p => toExclude.Contains(p.className));
			foreach(PredPoint2D p in points2D)
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

		public bool ToWorld(ref Matrix4x4 camMat, ref Matrix4x4 projMat,
			List<PredBox2D> boxes2D, out List<PredBox3D> boxes3D)
		{
			boxes3D = new List<PredBox3D>();
			// var filtered = points2D.Where(p => toExclude.Contains(p.className));
			foreach(PredBox2D box in boxes2D)
			{
				Vector3 min, max, cen;
				bool minHit = ToWorld(ref camMat, ref projMat,
					new Vector2(box.xmin, box.ymin), out min);
				bool maxHit = ToWorld(ref camMat, ref projMat,
					new Vector2(box.xmax, box.ymax), out max);
				bool cenHit = ToWorld(ref camMat, ref projMat,
					new Vector2(box.xcen, box.ycen), out cen);

				if (minHit && maxHit && cenHit)
				{
					boxes3D.Add(new PredBox3D(box, min, max, cen));
				}
			}
			return boxes3D.Count != 0;
		}
	}
}