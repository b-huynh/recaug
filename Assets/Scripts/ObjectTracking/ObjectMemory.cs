using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HoloToolkit.Unity.SpatialMapping;

public class ObjectMemory {
	public enum Modes { OVERWRITE, FIRST_IN }

	public GameObject objectPrefab = null;
	private Dictionary<string, GameObject> objects;
	private int regIDCounter;
	public bool isActive { get; private set; } = true;
	public Modes mode { get; private set; }

	// DEBUG
	public Material debugMaterial = null;

	public ObjectMemory(GameObject objectPrefab, Modes mode = Modes.FIRST_IN) {
		this.objects = new Dictionary<string, GameObject>();
		this.objectPrefab = objectPrefab;
		this.mode = mode;
		regIDCounter = 0;
	}

	public GameObject RegisterObject(string name, Vector3 position,
		GameObject worldObject)
	{
		if (mode == Modes.FIRST_IN && ContainsObject(name)) {
			// Object already registered
			return null;
		}

		// Create new RegisteredObject
		GameObject newObj = GameObject.Instantiate(objectPrefab);
		newObj.transform.position = position;

		RegisteredObject registration = newObj.GetComponent<RegisteredObject>();
		registration.Init(++regIDCounter, name);
		registration.UpdateGeometry(position);
		// registration.UpdateGeometry(worldObject);

		Renderer rend = newObj.GetComponent<Renderer>();
		rend.material.SetColor("_Color", Color.green);
		
		newObj.SetActive(isActive);
		objects[name] = newObj;
		return newObj;
	}

	public void SetActive(bool val) {
		isActive = val;
		foreach(var obj in objects) {
			obj.Value.SetActive(isActive);
		}
	}

	public bool ContainsObject(string classname) {
		return objects.ContainsKey(classname);
	}

	public GameObject GetRegisteredObject(string classname) {
		return ContainsObject(classname) ? objects[classname] : null;
	}

	public GameObject GetConfirmedObject(string classname) {
		GameObject obj = GetRegisteredObject(classname);
		if (obj != null) {
			return obj.GetComponent<RegisteredObject>().confirmed ? obj : null;
		}
		return null;
	}

	public void RemoveObject(string classname) {
		Debug.Log("Removing object: " + classname);
		GameObject obj = GetRegisteredObject(classname);
		if (obj != null) {
			GameObject.Destroy(obj);
			
		}
	}

	public void UpdateObject(string classname, string altLabel) {
		
	}

	public void MoveObject(string classname, Vector3 newPosition) {
		GameObject obj = GetRegisteredObject(classname);
		if (obj != null) {
			RegisteredObject registration = obj.GetComponent<RegisteredObject>();
			registration.ClearGeometry();
			registration.UpdateGeometry(newPosition);
		}
	}

	public void DisableObject(string classname) {
		GameObject obj = GetRegisteredObject(classname);
		if (obj != null) {
			obj.SetActive(false);
		}
	}

	public void EnableObject(string classname) {
		GameObject obj = GetRegisteredObject(classname);
		if (obj != null) {
			obj.SetActive(true);
		}
	}

	public bool EnabledStatus(string classname) {
		GameObject obj = GetRegisteredObject(classname);
		if (obj != null) {
			return obj.activeSelf;
		}
		return false;
	}

	public void SetMode(Modes mode) {
		this.mode = mode;
	}

	public List<KeyValuePair<string, GameObject>> GetNearbyObjects(Vector3 pos,
		float mindist)
	{
		var retval = new List<KeyValuePair<string, GameObject>>();
		foreach(var kv in objects) {
			if (Vector3.Distance(kv.Value.transform.position, pos) <= mindist) {
				retval.Add(kv);
			}
		}
		return retval;
	}

	public void OnSurfaceRemoved(
		object sender,
		DataEventArgs<SpatialMappingSource.SurfaceObject> e)
	{
		SpatialMappingSource.SurfaceObject surface = e.Data;
		
		foreach(KeyValuePair<string, GameObject> kv in objects) {
			RegisteredObject reg = kv.Value.GetComponent<RegisteredObject>();
			if (reg.ContainsGeometry(e.Data.Object)) {
				// An object we have registered and confirmed is gone.
				RemoveObject(kv.Key);
				Debug.LogFormat("Removed GameObject {0}, id: {1}, active: {2}", 
					e.Data.Object, e.Data.Object.GetInstanceID(),
					e.Data.Object.activeSelf);
				break;
			}
		}
		// Check if surface removed is one of our registered objects.
	}
}
