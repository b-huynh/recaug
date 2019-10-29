using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;

public class AppTabs : Singleton<AppTabs>
{
    public int currentApp  { get; private set; } = 1;

    // Indicator Animation State
    public float animateSpeed = 1.0f;
    private bool isAnimating = false;
    private Transform animateStart;
    private Transform animateEnd;
    private float animateStartTime;
    private float animateDistance;

    public GameObject leftTab, rightTab, activeIndicator;
    private Renderer[] renderers;

    // Start is called before the first frame update
    void Start()
    {
        animateDistance = Vector3.Distance(
            leftTab.transform.position, rightTab.transform.position);
        
        renderers = GetComponentsInChildren<MeshRenderer>();
        DisableRenderers();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAnimating)
        {
            float distCovered = (Time.time - animateStartTime) * animateSpeed;
            float distFraction = distCovered / animateDistance;
            activeIndicator.transform.position = Vector3.Lerp(
                animateStart.position, animateEnd.position, distFraction);
            if (distFraction >= 1.0f)
            {
                DisableRenderers();
                isAnimating = false;
            }
        }
    }

    public void SwitchApp(int appID)
    {
        Debug.Assert(appID >= 1 && appID <= 2);
        if (currentApp != appID)
        {
            currentApp = appID;   
            var endpoint = appID == 1 ? leftTab.transform : rightTab.transform;
            TriggerAnimation(endpoint);
        }
    }

    public void TriggerAnimation(Transform endpoint)
    {
        animateStart = activeIndicator.transform;
        animateEnd = endpoint;
        animateStartTime = Time.time;
        EnableRenderers();
        isAnimating = true;
    }

    private void DisableRenderers()
    {
        foreach(var renderer in renderers)
            renderer.enabled = false;
    }

    private void EnableRenderers()
    {
        foreach(var renderer in renderers)
            renderer.enabled = true;
    }
}
