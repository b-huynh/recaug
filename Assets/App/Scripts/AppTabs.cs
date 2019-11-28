using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;

public class AppTabs : Singleton<AppTabs>
{
    public int currentApp  { get; private set; } = 1;
    public List<GameObject> appIcons;
    public GameObject activeIndicator, focusIndicator;

    private bool appDrawerOpen = false;
    private Renderer[] renderers;

    // Start is called before the first frame update
    void Start()
    {
        // animateDistance = Vector3.Distance(
        //     leftTab.transform.position, rightTab.transform.position);

        renderers = GetComponentsInChildren<MeshRenderer>();
        SetRenderActive(false);
        focusIndicator.SetActive(false);

        
        foreach(GameObject icon in appIcons)
        {
            icon.GetComponent<UIFocusable>().OnSelect += delegate {
                int selectID =
                    appIcons.IndexOf(UIFocusable.current.gameObject) + 1;
                Debug.Log("Selected: " + selectID.ToString());
                if (appDrawerOpen)
                {
                    SwitchApp(selectID);
                }
            };
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (isAnimating)
        // {
        //     float distCovered = (Time.time - animateStartTime) * animateSpeed;
        //     float distFraction = distCovered / animateDistance;
        //     activeIndicator.transform.position = Vector3.Lerp(
        //         animateStart.position, animateEnd.position, distFraction);
        //     if (distFraction >= 1.0f)
        //     {
        //         SetRenderActive(false);
        //         isAnimating = false;
        //     }
        // }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!appDrawerOpen || UIFocusable.current == null)
            {
                appDrawerOpen = !appDrawerOpen;
                SetRenderActive(appDrawerOpen);
            }
        }

        if (UIFocusable.current != null && appDrawerOpen)
        {
            focusIndicator.SetActive(true);
            focusIndicator.transform.position = UIFocusable.current.transform.position;
        }
        else
        {
            focusIndicator.SetActive(false);
        }
    }

    public void SwitchApp(int appID)
    {
        if (currentApp != appID)
        {
            var start = appIcons[currentApp - 1].transform.position;
            var end = appIcons[appID - 1].transform.position;

            var animation = activeIndicator.GetComponent<SlideAnimation>();
            animation.animateStart = start;
            animation.animateEnd = end;
            animation.OnAnimationStop += delegate { 
                SetRenderActive(false);
                appDrawerOpen = false;
            };
            animation.StartAnimation();

            currentApp = appID;
        }
    }

    // private void TriggerAnimation(Transform endpoint)
    // {
    //     animateStart = activeIndicator.transform;
    //     animateEnd = endpoint;
    //     animateStartTime = Time.time;
    //     SetRenderActive(true);
    //     isAnimating = true;
    // }

    private void SetRenderActive(bool val)
    {
        foreach(var renderer in renderers)
            renderer.enabled = val;
    }
}
