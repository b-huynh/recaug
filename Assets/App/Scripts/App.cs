using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
    public int appID = -1;
    public string appName;
    public string appDescription;
    public Material appLogo;
    public bool running { get; private set; } = false;
    private List<GameObject> linked;

    protected virtual void Awake()
    {
        linked = new List<GameObject>();
        appID = GameManager.Instance.RegisterApp(appID, this);
    }

    void Update() {}

    // Links an object as associated with this app. Object will be disabled when
    // this app is disabled.
    public void Link(GameObject obj)
    {
        linked.Add(obj);
        obj.SetActive(running);
    }

    public void Unlink(GameObject obj)
    {
        obj.SetActive(false);
        linked.Remove(obj);
    }

    public void UnlinkAll()
    {
        foreach(var l in linked)
        {
            // Unlink(l);
            l.SetActive(false);
        }
        linked.Clear();
    }

    public void Resume()
    {
        SetActiveState(true);
        running = true;
    }

    public void Suspend()
    {
        SetActiveState(false);
        running = false;
    }

    public void SetActiveState(bool active)
    {
        foreach(var obj in linked)
        {
            obj.SetActive(active);
        }
    }

    public void SetRenderState(bool enabled)
    {
        foreach(var obj in linked)
        {
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = enabled;
            }
        }
    }
}
