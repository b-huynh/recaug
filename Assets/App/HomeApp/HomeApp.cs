using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;

// All this does is label words
public class HomeApp : App
{
    public GameObject labelPrefab;
    private bool showLabels = false;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        ObjectRegistry.Instance.OnRegistered += OnObjectRegistered;
    }

    public void Start()
    {
        SetRenderState(showLabels);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            showLabels = !showLabels;
        }
        SetRenderState(showLabels);  
    }

    void OnObjectRegistered(ObjectRegistration registration)
    {
        var label = GameObject.Instantiate(labelPrefab);
        Link(label);
        label.GetComponentInChildren<TextMesh>().text = registration.className;
        label.transform.parent = registration.gameObject.transform;
        label.transform.localPosition = new Vector3();
        // label.transform.position = registration.position;
    }
}
