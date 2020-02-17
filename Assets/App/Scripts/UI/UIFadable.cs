using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.InputModule;

public class UIFadable : MonoBehaviour, IFocusable
{
    public float fadeOutTime = 1.0f; // seconds

    private float exitTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        SetAlpha(0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (exitTimer > 0.0f)
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
            Color currCol = rend.material.color;
            currCol.a = val;
            rend.material.color = currCol;
        }
    }

    private bool IntersectGaze()
    {
        Ray gazeRay = new Ray(GazeManager.Instance.GazeOrigin, GazeManager.Instance.GazeNormal);
        return GetComponent<BoxCollider>().bounds.IntersectRay(gazeRay);
    }

    public void OnFocusEnter()
    {
        Debug.Log("Focus Enter");
        exitTimer = 0.0f;
        SetAlpha(1.0f);
    }

    public void OnFocusExit()
    {
        if (!IntersectGaze())
        {
            Debug.Log("Fade Exit");
            exitTimer = fadeOutTime;
        }
        Debug.Log("Focus Exit");
    }
}
