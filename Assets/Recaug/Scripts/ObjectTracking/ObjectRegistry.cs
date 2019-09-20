using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

public class ObjectRegistry : Singleton<ObjectRegistry> {
	public enum Modes { OVERWRITE, FIRST_IN }
	public Modes mode = Modes.FIRST_IN;
	public GameObject objectPrefab = null;
	private Dictionary<string, ObjectRegistration> objects;

	// public ObjectRegistry(GameObject objectPrefab, Modes mode = Modes.FIRST_IN) {
	// 	this.objects = new Dictionary<string, ObjectRegistration>();
	// 	this.objectPrefab = objectPrefab;
	// 	this.mode = mode;
	// }

	// public HashSet<string> confirmedObjects;

	public void Start() {
		this.objects = new Dictionary<string, ObjectRegistration>();
		// this.mode = mode;
		// this.confirmedObjects = new HashSet<string>();
	}

	public void Update() {

	}

	public void SetMode(Modes mode) {
		this.mode = mode;
	}

	public ObjectRegistration RegisterObject(string name, Vector3 position)
	{
		if (mode == Modes.FIRST_IN && IsRegistered(name)) {
			// Object already registered
			return null;
		}

		GameObject newObj = GameObject.Instantiate(objectPrefab);
		// newObj.transform.position = position;

		Renderer rend = newObj.GetComponent<Renderer>();
		rend.material.SetColor("_Color", Color.green);

		var registration = newObj.GetComponent<ObjectRegistration>();
		registration.Init(name, position);
		// registration.UpdateGeometry(position);
		registration.confirmLabelEvent.AddListener(OnConfirmLabel);
		registration.confirmTimeoutEvent.AddListener(OnConfirmTimeout);

		newObj.SetActive(true);
		objects[name] = registration;
		return registration;
	}

	public void UnregisterObject(string name)
	{
		if (IsRegistered(name))
		{
			objects[name].Destroy();
			GameObject.DestroyImmediate(objects[name].gameObject);
		}
	}

	public bool IsRegistered(string classname) {
		return objects.ContainsKey(classname);
	}

	public ObjectRegistration GetRegistration(string classname) {
		return IsRegistered(classname) ? objects[classname] : null;
	}

	public bool IsConfirmed(string classname) {
		return IsRegistered(classname) && GetRegistration(classname).confirmed;
	}

	public List<KeyValuePair<string, ObjectRegistration>> GetNearbyObjects(
		Vector3 pos, float mindist)
	{
		var retval = new List<KeyValuePair<string, ObjectRegistration>>();
		foreach(var kv in objects) {
			if (Vector3.Distance(kv.Value.transform.position, pos) <= mindist) {
				retval.Add(kv);
			}
		}
		return retval;
	}

	public void AddPredictions(WorldPredictions wp) {
		foreach(WorldPrediction p in wp.predictions) {
			AddPrediction(p);
		}
	}

	private void AddPrediction(WorldPrediction p) {
		// Determine whether this prediction might provide any useful info
		var nearby = GetNearbyObjects(p.position, Config.System.MaxObjectRadius);

		// Update the registration
		foreach(var kv in nearby)
		{
			kv.Value.UpdateRegistration(p);
		}

		if (objects.ContainsKey(p.label))
		{
			objects[p.label].UpdateRegistration(p);
		}
	}

	private void OnConfirmLabel(string correctClass) {
		// Remove all alternative class labels
		// confirmedObjects.Add(correctClass);

		foreach(var kv in objects) {
			kv.Value.RemoveLabel(correctClass);
		}
	}

	private void OnConfirmTimeout(string registeredClass) {
		objects.Remove(registeredClass);
	}

	public void ClearMemory()
	{
		foreach(var kv in objects)
		{
			UnregisterObject(kv.Key);
		}
		objects.Clear();
	}

	// public void SetActive(bool val) {
	// 	isActive = val;
	// 	foreach(var obj in objects) {
	// 		obj.Value.SetActive(isActive);
	// 	}
	// }

	// public bool EnabledStatus(string classname) {
	// 	GameObject obj = GetRegisteredObject(classname);
	// 	if (obj != null) {
	// 		return obj.activeSelf;
	// 	}
	// 	return false;
	// }

	// public void EnableObject(string classname) {
	// 	GameObject obj = GetRegisteredObject(classname);
	// 	if (obj != null) {
	// 		obj.SetActive(true);
	// 	}
	// }

	// public void MoveObject(string classname, Vector3 newPosition) {
	// 	GameObject obj = GetRegisteredObject(classname);
	// 	if (obj != null) {
	// 		RegisteredObject registration = obj.GetComponent<RegisteredObject>();
	// 		registration.ClearGeometry();
	// 		registration.UpdateGeometry(newPosition);
	// 	}
	// }

	// public void DisableObject(string classname) {
	// 	GameObject obj = GetRegisteredObject(classname);
	// 	if (obj != null) {
	// 		obj.SetActive(false);
	// 	}
	// }



	// public void RemoveObject(string classname) {
	// 	Debug.Log("Removing object: " + classname);
	// 	GameObject obj = GetRegisteredObject(classname);
	// 	if (obj != null) {
	// 		GameObject.Destroy(obj);
	// 	}
	// }

	// public void OnSurfaceRemoved(
	// 	object sender,
	// 	DataEventArgs<SpatialMappingSource.SurfaceObject> e)
	// {
	// 	SpatialMappingSource.SurfaceObject surface = e.Data;
		
	// 	foreach(KeyValuePair<string, GameObject> kv in objects) {
	// 		RegisteredObject reg = kv.Value.GetComponent<RegisteredObject>();
	// 		if (reg.ContainsGeometry(e.Data.Object)) {
	// 			// An object we have registered and confirmed is gone.
	// 			RemoveObject(kv.Key);
	// 			Debug.LogFormat("Removed GameObject {0}, id: {1}, active: {2}", 
	// 				e.Data.Object, e.Data.Object.GetInstanceID(),
	// 				e.Data.Object.activeSelf);
	// 			break;
	// 		}
	// 	}
	// 	// Check if surface removed is one of our registered objects.
	// }
}
