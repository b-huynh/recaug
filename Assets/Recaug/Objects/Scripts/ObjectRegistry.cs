using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

namespace Recaug
{
	/* 
		A registry of all known objects in the physical space.
		Registration policy can either be:
		- OVERWRITE: New objects with the same name overwrite old objects
		- FIRST_IN: The first instance of an object class is registered. All
			others are ignored
	*/ 
	public class ObjectRegistry : Singleton<ObjectRegistry>
	{
		public event Action<ObjectRegistration> OnRegistered = delegate {};
		public event Action<ObjectRegistration> OnRemoved = delegate {};

		public enum Policy { OVERWRITE, FIRST_IN }
		public Policy policy = Policy.FIRST_IN;

		public GameObject objectPrefab;

		public Dictionary<string, ObjectRegistration> registry;

		// Allows manual override of certain classifications to other names.
		private Dictionary<string, string> NameOverlaps = 
			new Dictionary<string, string>()
		{
			{"remote", "keyboard"},
			{"vase", "potted plant"},
			{"laptop", "tv"},
			{"cell phone", "smartphone"},
		};

		protected override void Awake()
		{
			base.Awake();
			registry = new Dictionary<string, ObjectRegistration>();
			if (objectPrefab == null || 
				objectPrefab.GetComponent<ObjectRegistration>() == null)
			{
				Debug.Log("Invalid ObjectPrefab Assigned!");
			}

		}

		public void Start()
		{
		}

		public void Update() 
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				string filePath = "C:\\Users\\yukib\\AppData\\LocalLow\\DefaultCompany\\Recaug-v0_1\\258707b6-a269-4471-a9a9-c0a4173a17eb.log";
				LoadFromLog(filePath);
			}
		}

		public ObjectRegistration Register(string name, Vector3 position)
		{
			if (NameOverlaps.ContainsKey(name))
			{
				name = NameOverlaps[name];
			}

			if (policy == Policy.FIRST_IN && Contains(name))
			{
				// Object already registered, return same object
				return registry[name];
			}

			// Create Registration
			var obj = GameObject.Instantiate(objectPrefab);
			var registration = obj.GetComponent<ObjectRegistration>();
			registration.Init(name, 1.0f, position);
			registry.Add(name, registration);

			Debug.LogFormat("Object: {0} registered", registration.className);
			StatsTracker.Instance.LogObjectDiscovered(
				registration.className, registration.position);
			OnRegistered(registration);
			
			return registration;
		}

		public void Remove(string name)
		{
			if (Contains(name))
			{
				OnRemoved(registry[name]);
				registry[name].Destroy();
				registry.Remove(name);
			}
		}

		public bool Contains(string classname)
		{
			return registry.ContainsKey(classname);
		}

		public ObjectRegistration Find(string classname)
		{
			return Contains(classname) ? registry[classname] : null;
		}

		public Dictionary<string, ObjectRegistration> Nearby(Vector3 pos,
			float mindist)
		{
			var retval = new Dictionary<string, ObjectRegistration>();
			foreach(var kv in registry)
			{
				if (Vector3.Distance(kv.Value.position, pos) <= mindist)
				{
					retval[kv.Key] = kv.Value;
				}
			}
			return retval;
		}

		public void Clear()
		{
			foreach(var kv in registry)
			{
				// Remove(kv.Key);
				kv.Value.gameObject.SetActive(false);
			}
			registry.Clear();
		}
		
		public void LoadFromLog(string filePath)
		{
			string[] logLines = File.ReadAllLines(filePath);
			foreach(string line in logLines)
			{
				if (line.StartsWith("object"))
				{
					string[] columns = line.Split(',');
					string positionStr = columns[3].TrimStart(' ');
					Debug.Log(positionStr);
					string[] positionValues = positionStr.Split(',');
					
					string xStr = positionValues[0].TrimStart('(');
					float x = float.Parse(xStr);

					string yStr = positionValues[1].TrimStart(' ');
					float y = float.Parse(yStr);

					string zStr = positionValues[2].TrimStart(' ').TrimEnd(')');
					float z = float.Parse(zStr);

					string parsedName = columns[2].TrimStart(' ');
					Vector3 parsedPosition = new Vector3(x, y, z);
					Register(parsedName, parsedPosition);
				}
			}
		}
	}

} // namespace Recaug