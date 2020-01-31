using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class UIFocusableGroup : MonoBehaviour
{
    public string groupName = "";
    public GameObject focusIndicator;
    // public List<UIFocusable> elements = new List<UIFocusable>();
    public UIFocusable current = null;

    void Awake()
    {
    }

    void Start()
    {
        if (groupName == "")
        {
            groupName = System.Guid.NewGuid().ToString();
        }
    }

    void Update()
    {
    }

    public void OnEnable()
    {
        // Debug.Log("UIFocusable OnEnabled");
        current = null;
        // Enable all UI elements of this UI group
        focusIndicator.SetActive(false);
        focusIndicator.GetComponent<Renderer>().enabled = false;
        // GetComponentInChildren<UIFocusable>().enabled = true;
        GetComponentsInChildren<UIFocusable>().ToList().ForEach(x => x.enabled = true);
    }

    public void OnDisable()
    {
        // Debug.Log("UIFocusable OnDisabled");
        current = null;
        focusIndicator.SetActive(false);
        focusIndicator.GetComponent<Renderer>().enabled = false;
        // GetComponentInChildren<UIFocusable>().enabled = false;
        GetComponentsInChildren<UIFocusable>().ToList().ForEach(x => x.enabled = false);
    }
}