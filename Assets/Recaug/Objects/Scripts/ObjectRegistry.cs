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
			{"cake", "donuts"}
		};

		//TODO: Remove this before experiment.
		private float earlyWindow = 45;
		private bool earlyRegistryComplete = false;
		private Dictionary<string, Vector3> EarlyAutoRegistry =
			new Dictionary<string, Vector3>()
		{
			{"apple", new Vector3(-0.591f, 0.286f, -8.618f)},
			{"sports ball", new Vector3(-1.080f, 0.242f, -9.194f)},
		};

		private float autoWindow = 180; // Amount of seconds before auto registry.
		private bool autoRegistryComplete = false;
		private Dictionary<string, Vector3> AutoRegistry =
			new Dictionary<string, Vector3>()
		{
			{"apple", new Vector3(-0.591f, 0.286f, -8.618f)},
			{"sports ball", new Vector3(-1.080f, 0.242f, -9.194f)},
			{"giraffe", new Vector3(-1.079f, 0.222f, -8.911f)},
			{"donut", new Vector3(-0.859f, 0.249f, -8.418f)},
			{"chair", new Vector3(0.649f, -0.104f, -9.013f)},
			{"car", new Vector3(-1.466f, 0.243f, -9.261f)},

			{"clock", new Vector3(-0.097f, -0.275f, -7.761f)},
			{"teddy bear", new Vector3(0.490f, 0.322f, -8.009f)},
			{"scissors", new Vector3(0.261f, 0.221f, -8.345f)},
			{"keyboard", new Vector3(0.794f, 0.218f, -8.665f)},
			{"book", new Vector3(1.166f, 0.274f, -8.689f)},
			{"tv", new Vector3(0.995f, 0.411f, -8.392f)},
			{"potted plant", new Vector3(0.687f, 0.269f, -8.197f)},
			{"mouse", new Vector3(1.036f, 0.211f, -8.886f)},
			{"smartphone", new Vector3(0.485f, 0.214f, -8.449f)},
			{"bottle", new Vector3(-1.292f, 0.297f, -8.806f)},
			{"wine glass", new Vector3(-1.131f, 0.227f, -8.652f)},
			{"cup", new Vector3(-1.227f, 0.277f, -9.043f)},
			{"zebra", new Vector3(-0.887f, 0.228f, -9.071f)},
			{"airplane", new Vector3(-0.816f, 0.243f, -8.845f)},
			{"tennis racket", new Vector3(-1.240f, 0.216f, -9.415f)},
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
			// if (!earlyRegistryComplete && Time.time > earlyWindow)
			// {
			// 	foreach(var kv in EarlyAutoRegistry)
			// 	{
			// 		Register(kv.Key, kv.Value);
			// 	}
			// 	earlyRegistryComplete = true;
			// }

			// if (!autoRegistryComplete && Time.time > autoWindow)
			// {
			// 	foreach(var kv in AutoRegistry)
			// 	{
			// 		Register(kv.Key, kv.Value);
			// 	}
			// 	autoRegistryComplete = true;
			// }

			if (Input.GetKeyDown(KeyCode.T))
			{
				string filePath = "C:\\Users\\peter\\AppData\\LocalLow\\DefaultCompany\\Recaug-v0_1\\a55b40a1-7848-4329-9a06-522775a9f292.log";
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
					string[] positionValues = positionStr.Split(';');
					
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