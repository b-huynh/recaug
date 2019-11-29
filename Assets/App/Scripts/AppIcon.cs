using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class stores information about how to display AppIcons
public class AppIcon : MonoBehaviour
{
    public List<Material> AppIconMaterials;


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

    private void SetDisplayProperties()
    {
        GetComponent<MeshRenderer>().material = 
            AppIconMaterials[Math.Min(_appID, AppIconMaterials.Count - 1)];
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<UIFocusable>().OnSelect += delegate {
            GameManager.Instance.SwitchApp(this.AppID);
        };
        SetDisplayProperties();
    }

    // Update is called once per frame
    void Update()
    {
        SetDisplayProperties();
    }
}
