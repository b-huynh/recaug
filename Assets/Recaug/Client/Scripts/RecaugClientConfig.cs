using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recaug.Client
{
    [System.Serializable]
    public class RecaugClientConfig : ISerializationCallbackReceiver
    {
        public int CameraWidth = 896;
        public int CameraHeight = 504;
        public int FilterWindowSize = 120;
        public int FilterWindowMinCount = 5;
        public float FilterMinDist = 0.2f;
        public HashSet<string> KnownObjects = new HashSet<string>();
        public HashSet<string> ValidObjects = new HashSet<string>();

        public List<string> _knownObjects = new List<string>();
        public List<string> _validObjects = new List<string>();

        public void OnBeforeSerialize()
        {
            _knownObjects = new List<string>(KnownObjects);
            _validObjects = new List<string>(ValidObjects);
        }

        public void OnAfterDeserialize()
        {
            KnownObjects = new HashSet<string>(_knownObjects);
            ValidObjects = new HashSet<string>(_validObjects);
        }
    }
} // namespace Recaug.Client