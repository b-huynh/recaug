using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Recaug
{
    // Defines the geometry of an object.
    public class ObjectGeometry
    {
        public CircularBuffer<PredPoint3D> points;
        // Unity geometry components like meshes and colliders.
        public List<GameObject> gameObjects;

        public ObjectGeometry(int capacity)
        {
            points = new CircularBuffer<PredPoint3D>(capacity);
            gameObjects = new List<GameObject>(capacity);
        }
    }

    // ObjectRegistration contains all the info needed to represent a real object.
    public class ObjectRegistration
    {
        public string className;
        public float confidence;
        public Vector3 position
        {
            get { return gameObject.transform.position; }
            set { gameObject.transform.position = value; }
        }
        public int instanceID
        {
            get { return gameObject.GetInstanceID(); }
            private set {}
        }

        private ObjectGeometry geometry;
        private GameObject gameObject; // An empty gameObject for convenience.

        public ObjectRegistration(string className, float confidence,
            Vector3 position)
        {
            this.gameObject = new GameObject();
         
            this.className = className;
            this.confidence = confidence;
            this.position = position;

            this.geometry = new ObjectGeometry(Config.System.GeometryCapacity);
        }

        public void UpdateGeometry(PredPoint3D point)
        {
            geometry.points.PushBack(point);
        }

        public void Destroy()
        {
            GameObject.Destroy(gameObject);
        }
    }

} // namespace Recaug