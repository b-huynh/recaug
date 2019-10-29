using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
    public int appID = -1;

    private List<GameObject> linked;

    protected virtual void Start()
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
    }

    public void Unlink(GameObject obj)
    {
        linked.Remove(obj);
    }

    public void Resume()
    {
        foreach(var obj in linked)
        {
            obj.SetActive(true);
        }
        // gameObject.SetActive(true);
    }

    public void Suspend()
    {
        foreach(var obj in linked)
        {
            obj.SetActive(false);
        }
        // gameObject.SetActive(false);      
    }
}
