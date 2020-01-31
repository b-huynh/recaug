using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class stores information about how to display AppIcons
public class AppIcon : MonoBehaviour
{   
    public int _appID;
    public int AppID
    {
        get => _appID;
        set
        {
            _appID = value;
            SetDisplayProperties();
        }
    }

    private App appRef;

    private void SetDisplayProperties()
    {
        appRef = GameManager.Instance.GetApp(AppID);
        GetComponent<MeshRenderer>().material = appRef.appLogo;
        transform.Find("AppName").GetComponent<TextMesh>().text = appRef.appName;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetDisplayProperties();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
