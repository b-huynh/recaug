using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug.Client;

public class LearningTask : MonoBehaviour
{
    public GameObject objectTagPrefab;

    private List<string> knownObjects;

    // Start is called before the first frame update
    void Start()
    {
        RecaugClient.Instance.objectNearbyEvent.AddListener(OnObjectNearby);
        knownObjects = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnObjectNearby(ContextEventData eventData)
    {
        if (knownObjects.Contains(eventData.objectType))
            return;

        // Create object tag
        var objectTag = GameObject.Instantiate(objectTagPrefab,
            eventData.position, transform.rotation);
        objectTag.GetComponent<AnnotationVisualizer>().text = eventData.objectType;
        
        // Update known objects cache
        knownObjects.Add(eventData.objectType);
    }
}
