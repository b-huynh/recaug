using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectMemory {
	public enum Modes { OVERWRITE, FIRST_IN }

	public GameObject objectPrefab = null;
	private Dictionary<string, GameObject> objects;
	public bool isActive { get; private set; } = true;
	public Modes mode { get; private set; }

	public ObjectMemory(GameObject objectPrefab, Modes mode = Modes.FIRST_IN) {
		this.objects = new Dictionary<string, GameObject>();
		this.objectPrefab = objectPrefab;
		this.mode = mode;
	}

	public GameObject RegisterObject(string name, Vector3 position) {
		if (mode == Modes.FIRST_IN && ContainsObject(name)) {
			// Object already registered
			return null;
		}

		// Create new RegisteredObject
		GameObject newObj = GameObject.Instantiate(objectPrefab);
		newObj.transform.position = position;
		newObj.GetComponent<RegisteredObject>().Init(name);
		newObj.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
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
		GameObject obj = GetRegisteredObject(classname);
		if (obj != null) {
			GameObject.Destroy(obj);
		}
	}

	public void SetMode(Modes mode) {
		this.mode = mode;
	}
}
