using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.InputModule;

// [RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(BoxCollider))]
public class UIFadable : MonoBehaviour, IFocusable
{
    public float fadeOutTime = 1.0f; // seconds

    public float stopVal = 0.1f;

    private float exitTimer = 0.0f;

    private float currentAlpha = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        SetAlpha(stopVal);
    }

    public event Action OnFadeFocusEnter = delegate {};
    public event Action OnFadeFocusExit = delegate {};

    // Update is called once per frame
    void Update()
    {
        if (currentAlpha == 1.0f)
        {
            // Is focusing, check if not focusing.
            if (!IntersectGaze())
            {
                exitTimer = fadeOutTime;
                OnFadeFocusExit();
            }
        }
        
        if (exitTimer > stopVal)
        {
            exitTimer -= Time.deltaTime;
            float percentage = exitTimer / fadeOutTime;
            SetAlpha(percentage);
        }
    }

    private void SetAlpha(float val)
    {
        foreach(var rend in GetComponentsInChildren<Renderer>())
        {
            if (rend.gameObject.layer == LayerMask.NameToLayer("IgnoreFadable"))
            {
                continue;
            }
            Color currCol = rend.material.color;
            currCol.a = val;
            rend.material.color = currCol;
        }
        currentAlpha = val;
    }
    private bool IntersectGaze()
    {
        Ray gazeRay = new Ray(GazeManager.Instance.GazeOrigin, GazeManager.Instance.GazeNormal);
        return GetComponent<BoxCollider>().bounds.IntersectRay(gazeRay);
    }

    public void OnFocusEnter()
    {
        exitTimer = 0.0f;
        SetAlpha(1.0f);

        OnFadeFocusEnter();
    }

    public void OnFocusExit()
    {
        // if (!IntersectGaze())
        // {
        //     Debug.Log("Fade Exit");
        //     exitTimer = fadeOutTime;
        // }
        // Debug.Log("Focus Exit");
    }
}
