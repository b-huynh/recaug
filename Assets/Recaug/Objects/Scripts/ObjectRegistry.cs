using System.Collections;
using System.Collections.Generic;
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
		public class ObjectRegisteredEvent : UnityEvent<ObjectRegistration> {}
		public class ObjectRemovedEvent : UnityEvent<ObjectRegistration> {}

		public ObjectRegisteredEvent objectRegisteredEvent;
		public ObjectRemovedEvent objectRemovedEvent;

		public enum Policy { OVERWRITE, FIRST_IN }
		public Policy policy = Policy.FIRST_IN;

		private Dictionary<string, ObjectRegistration> registry;

		protected override void Awake()
		{
			base.Awake();
			
			registry = new Dictionary<string, ObjectRegistration>();
			objectRegisteredEvent = new ObjectRegisteredEvent();
			objectRemovedEvent = new ObjectRemovedEvent();
		}

		public void Update() {}

		public ObjectRegistration Register(string name, Vector3 position)
		{
			if (policy == Policy.FIRST_IN && Contains(name)) {
				// Object already registered, return same object
				return registry[name];
			}

			// Create Registration
			registry[name] = new ObjectRegistration(name, 1.0f, position);
			Debug.LogFormat("Object: {0} registered", name);
			
			objectRegisteredEvent.Invoke(registry[name]);
			return registry[name];
		}

		public void Remove(string name)
		{
			if (Contains(name))
			{
				objectRemovedEvent.Invoke(registry[name]);
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
				Remove(kv.Key);
			}
			registry.Clear();
		}
	}

} // namespace Recaug