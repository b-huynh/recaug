using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Recaug;

public class ContextMenu : MonoBehaviour
{
    public GameObject appIconPrefab;
    public UIFocusableGroup focusableGroup;

    private Dictionary<int, GameObject> availableApps =
        new Dictionary<int, GameObject>();

    public List<Vector3> appIconPositions = new List<Vector3> {
        new Vector3(-2.5f, -1.25f, 0),
        new Vector3(-2.5f, 0, 0),
        new Vector3(-2.5f, -2.5f, 0)
    };

    public bool isOpen { get; private set; } = false;

    // public static bool showAll = false;

    // public bool show = false;

    // Start is called before the first frame update
    void Start()
    {
        // Get Object Name
        var reg = GetComponentInParent<ObjectRegistration>();
        var textMesh = transform.Find("Prompt").GetComponent<TextMesh>();
        textMesh.text = string.Format("( {0} )\nOpen With", reg.className);

        // var focusable = GetComponentInChildren<UIFocusable>();
        // focusable.OnSelect += delegate {
        //     Open();
        // };
        Close();
    }

    // Update is called once per frame
    void Update()
    {
        // bool render = show || showAll;

        // if (render)
        // {
        //     UpdateApplist();
        // }
        // else
        // {
        //     foreach(var renderer in GetComponentsInChildren<Renderer>())
        //     {
        //         if (renderer.gameObject.name != "LockOn")
        //         {
        //             renderer.enabled = false;
        //         }
        //     }
        // }
    }

    private void UpdateApplist()
    {
        // bool showIndicator = false;

        int idx = 0;
        foreach(var kv in availableApps)
        {
            // if (kv.Key == GameManager.Instance.currAppID)
            // {
            //     // kv.Value.SetActive(false);
            //     continue;
            // }

            kv.Value.transform.localPosition = appIconPositions[idx];
            // kv.Value.SetActive(true);
            // showIndicator = true;
            idx = Math.Min(idx + 1, appIconPositions.Count - 1);
        }

        // foreach (var renderer in GetComponentsInChildren<Renderer>())
        // {
        //     renderer.enabled = showIndicator;
        // }
    }

    public void Open()
    {
        if (Config.Experiment.Multitasking != MultitaskingType.InSitu &&
            Config.Experiment.Multitasking != MultitaskingType.ManualInSitu)
        {
            return; // Context menu not enabled without these multitasking options
        }

        foreach(var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
        GetComponentInChildren<UIFocusableGroup>().enabled = true;
        GetComponentsInChildren<UIFocusable>().ToList().ForEach(c => c.enabled = true);
        isOpen = true;

        string className = GetComponentInParent<ObjectRegistration>().className;
        StatsTracker.Instance.LogContextMenuOpen(className);
    }

    public void Close()
    {
        foreach(var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        GetComponentInChildren<UIFocusableGroup>().enabled = false;
        GetComponentsInChildren<UIFocusable>().ToList().ForEach(c => c.enabled = false);
        isOpen = false;

        string className = GetComponentInParent<ObjectRegistration>().className;
        StatsTracker.Instance.LogContextMenuClose(className);
    }

    public static void OpenAll()
    {
        GameObject.FindObjectsOfType<ContextMenu>().ToList().ForEach(
            menu => menu.Open());
    }

    public static void CloseAll()
    {
        GameObject.FindObjectsOfType<ContextMenu>().ToList().ForEach(
            menu => menu.Close());
    }

    public void AddApp(int appID)
    {
        if (!availableApps.ContainsKey(appID))
        {
            var registration = 
                transform.parent.gameObject.GetComponent<ObjectRegistration>();
            // GameObject appIcon = GameObject.Instantiate(appIconPrefab);

            var group = GetComponentInChildren<UIFocusableGroup>();
            var appIcon = GameObject.Instantiate(appIconPrefab, group.transform);
            appIcon.layer = LayerMask.NameToLayer("OSFocusableUI");

            // appIcon.transform.parent = transform;
            appIcon.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
            appIcon.transform.localScale = new Vector3(1.0f, 1.0f, 0.2f);
            appIcon.GetComponent<AppIcon>().AppID = appID;
            // appIcon.GetComponent<UIFocusable>().focusIndicator = focusIndicator;
            // appIcon.GetComponent<UIFocusable>().group = focusableGroup;
            appIcon.GetComponent<UIFocusable>().selectDuration = 
                // Config.UI.FocusConfirmTime;
                0;
            appIcon.GetComponent<UIFocusable>().OnSelect += delegate {
                // GameManager.Instance.SwitchApp(appID, false);
                // Close();
                GameManager.Instance.RequestAppFocus(appID, registration);
                CloseAll();
            };
            availableApps[appID] = appIcon;

            UpdateApplist();
        }
    }

    public void RemoveApp(int appID)
    {
        if (availableApps.ContainsKey(appID))
        {
            // GameObject.Destroy(availableApps[appID]);
            availableApps[appID].SetActive(false);
            availableApps.Remove(appID);

            UpdateApplist();
        }
    }

    public void OnFocusSelect(GameObject caller)
    {
        // Debug.Log("Called FocusSelect");
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }
}
