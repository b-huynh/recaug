using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenWithIndicator : MonoBehaviour
{
    public GameObject appIconPrefab;
    public List<GameObject> appIcons;
    private List<GameObject> availableApps;
    private List<Vector3> appIconPositions = new List<Vector3> {
        new Vector3(-2.5f, -1.25f, 0),
        new Vector3(-2.5f, 0, 0),
        new Vector3(-2.5f, -2.5f, 0)
    };

    // Start is called before the first frame update
    void Start()
    {
        availableApps = new List<GameObject>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            AddAvailableApp(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            AddAvailableApp(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            AddAvailableApp(2);
        }
    }

    public void AddAvailableApp(int appID)
    {
        // GameObject newAppIcon = GameObject.Instantiate(appIconPrefab);
        GameObject newAppIcon = appIcons[appID];
        newAppIcon.GetComponent<AppIcon>().AppID = appID;
        newAppIcon.SetActive(true);
        // newAppIcon.GetComponent<UIFocusable>().OnSelect += delegate {
        //     GameManager.Instance.SwitchApp(appID);
        // };
        // newAppIcon.transform.parent = transform;
        // newAppIcon.transform.localPosition = 
        //     appIconPositions[Math.Min(appID, appIconPositions.Count - 1)];
        // availableApps.Add(newAppIcon);
    }
}
