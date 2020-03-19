using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

using Recaug;
using Recaug.Client;

public struct AppStackFrame
{
    public App app;
    public ObjectRegistration callerObject;
}

public enum MultitaskingType
{
    Manual, InSitu, ManualInSitu, Automatic
}


public class GameManager : Singleton<GameManager>
{
    public GameObject openWithPrefab;

    public string sessionID = "";

    // App Switching
    public int startAppID;
    
    private Stack<AppStackFrame> appStack = new Stack<AppStackFrame>();
    public string stackHistory = "";
    public int currAppID
    { 
        get { return appStack.Count > 0 ? appStack.Peek().app.appID : -1; }
        private set {}
    }
    private Dictionary<int, App> apps = new Dictionary<int, App>();
    private Dictionary<int, Action<ObjectRegistration>> callMap =
        new Dictionary<int, Action<ObjectRegistration>>();


    private bool showAllContextMenus = false;

    // Location of config file
    private List<string> configHostList = new List<string>() {
        // "0.0.0.0",
        // "192.168.10.19",
        // // "192.168.10.3",
        // // "192.168.10.25",
        "192.168.100.233",
        "192.168.100.244",
        "192.168.100.169",
    };
    public string configHost;

    private string configPort = "8080";
    private string configURL
    {
        get
        { 
            return string.Format(
                "http://{0}:{1}/config.json", configHost, configPort);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        foreach(string host in configHostList)
        {
            string url = string.Format(
                "http://{0}:{1}/config.json", host, configPort);
            Debug.Log("Trying to fetch config from: " + url);
            if (Config.LoadHTTP(url))
            {
                configHost = host;
                break;
            }
        }
        
        RecaugClient.Instance.Init(Config.Params.Recaug);
        RecaugClient.Instance.OnNearIn += OnNearIn;
        RecaugClient.Instance.OnNearOut += OnNearOut;

        sessionID = System.Guid.NewGuid().ToString();
        StatsTracker.Instance.LogStart(sessionID);

        // currAppID = startAppID;
        RequestAppFocus(0);
    }

    public void Start()
    {
        
    }

    void Update()
    {

#if !WINDOWS_UWP
        List<string> debugWords = new List<string> {
            "bowl",
            "smartphone",
            "mouse",
            "bottle",
            "cup",
            "keyboard",
            "tv",
            "wine glass",
            "chair"
        };
        var shuffled = debugWords.OrderBy(x => System.Guid.NewGuid()).ToList();
        Queue<string> wordQ = new Queue<string> (shuffled);

        int numericKey;
        if (Utils.GetKeyDownNumeric(out numericKey))
        {
            for(int i = 0; i < numericKey; ++i)
            {
                ObjectRegistry.Instance.Register(
                    wordQ.Dequeue(), Utils.RandomPosition());
            }
        }
#endif

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            showAllContextMenus = !showAllContextMenus;
            if (showAllContextMenus)
            {
                foreach(var kv in ObjectRegistry.Instance.registry)
                {
                    // kv.Value.GetComponentInChildren<ContextMenu>().Open();
                    ContextMenu.OpenAll();
                }
            }
            else
            {
                foreach(var kv in ObjectRegistry.Instance.registry)
                {
                    // kv.Value.GetComponentInChildren<ContextMenu>().Close();
                    ContextMenu.CloseAll();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SoftReset();
        }

        UpdateClickInputState();
    }

    static public float LongPressWindow = 0.35f;
    private float holdTime = 0.0f;
    private bool longPressTriggered = false;

    public bool Clicked = false;
    public bool LongPressed = false;

    private void UpdateClickInputState()
    {
        Clicked = false;
        LongPressed = false;
        
        if (Input.GetButtonDown("Submit") ||
            Input.GetMouseButtonDown(0))
        {
            holdTime = 0.0f;
        }

        // if (Input.GetKey(key))
        if (Input.GetButton("Submit") ||
            Input.GetMouseButton(0))
        {
            holdTime += Time.deltaTime;
        }

        // if (Input.GetKeyUp(key))
        if (Input.GetButtonUp("Submit") ||
            Input.GetMouseButtonUp(0))
        {
            if (holdTime < LongPressWindow)
            {
                Clicked = true;
            }

            // Reset click state
            holdTime = 0.0f;
            longPressTriggered = false;
        }

        if (!longPressTriggered)
        {
            if (holdTime > LongPressWindow)
            {
                longPressTriggered = true;
                LongPressed = true;
            }
        }
    }


    private void SoftReset()
    {
        /*
            - Load new config
            - Delete all current annotations
            - Generate new session ID
        */
        foreach(var kv in apps)
        {
            kv.Value.UnlinkAll();
        }
        ObjectRegistry.Instance.Clear();
        Config.LoadHTTP(configURL);
        RecaugClient.Instance.Init(Config.Params.Recaug);
        sessionID = System.Guid.NewGuid().ToString();
    }

    // Execute developer commands
    public void DevCommand(string command)
    {
        string[] tokens = command.Split();
        switch(tokens[0])
        {
            case "ip":
                // Config.UpdateConfig("ServerIP", tokens[1]);
                configHost = tokens[1];
                SoftReset();
                break;
        }
    }
    
    public void DebugStackFrame()
    {
        string log = "[TOP]";
        foreach(var sf in appStack)
        {
            log += "[" + sf.app.name + "]";
        }
        log += "[END]";
        stackHistory = log;
    }

    public void RequestAppFocus(int appID, 
        ObjectRegistration callerObject = null, bool animate = false)
    {
        if (appStack.Count > 0 && appID == appStack.Peek().app.appID)
        {
            return;
        }

        string source = "null";
        string destination = "null";

        // Stop current app
        if (appStack.Count > 0)
        {
            source = appStack.Peek().app.appName;
            appStack.Peek().app.Suspend();  
        }

        // Switch to new app by placing on stack
        var stackFrame = new AppStackFrame();
        stackFrame.app = apps[appID];
        stackFrame.callerObject = callerObject;
        appStack.Push(stackFrame);
        appStack.Peek().app.Resume();

        destination = appStack.Peek().app.appName;

        // if (animate)
        // {
        //     AppTabs.Instance.SetRenderActive(true);
        // }
        // AppTabs.Instance.RequestAppSwitch(appID, animate, delegate {
        //     AppTabs.Instance.RequestDrawerClose();
        // });

        if (stackFrame.app.name == "HomeApp")
        {
            appStack.Clear();
            appStack.Push(stackFrame);
        }

        DebugStackFrame();

        string switchType = callerObject == null ? "manual" : "contextMenu";
        StatsTracker.Instance.LogAppSwitch(source, destination, switchType);

        appStack.Peek().app.BroadcastMessage("OnAppSwitch", stackFrame,
            SendMessageOptions.DontRequireReceiver);
    }

    public void ReleaseAppFocus(bool animate = false)
    {
        if (appStack.Count <= 1)
        {
            return;
        }

        // Stop current app
        appStack.Peek().app.Suspend();

        // Release current app and return to previous app
        appStack.Pop();
        var stackFrame = appStack.Peek();
        stackFrame.app.Resume();

        // if (animate)
        // {
        //     // App drawer animation shows moving back to previous app
        //     AppTabs.Instance.SetRenderActive(true);
        // }
        // AppTabs.Instance.RequestAppSwitch(appStack.Peek().app.appID, animate, 
        //     delegate {
        //         AppTabs.Instance.RequestDrawerClose();
        //     }
        // );

        if (stackFrame.app.name == "HomeApp")
        {
            appStack.Clear();
            appStack.Push(stackFrame);
        }

        DebugStackFrame();

        appStack.Peek().app.BroadcastMessage("OnAppSwitch", stackFrame, 
            SendMessageOptions.DontRequireReceiver);
    }

    public bool SwitchApp(int appID, bool animate = false, 
        ObjectRegistration registration = null)
    {
        // if (currAppID == appID)
        //     return false;

        apps[currAppID].Suspend();
        apps[appID].Resume();

        if (animate)
        {
            AppTabs.Instance.SetRenderActive(true);
        }
        AppTabs.Instance.RequestAppSwitch(appID, animate, delegate {
            AppTabs.Instance.RequestDrawerClose();
        });

        currAppID = appID;

        if (callMap.ContainsKey(currAppID))
        {
            callMap[appID](registration);
        }
        return true;
    }

    public void AddAppSwitchCallback(int appID, Action<ObjectRegistration> cb)
    {
        callMap[appID] = cb;
    }

    public int RegisterApp(int appID, App app)
    {
        apps.Add(appID, app);
        if (startAppID == appID)
            app.Resume();
        else
            app.Suspend();
        // app.appID = apps.Count - 1;
        return app.appID;
    }

    public App GetApp(int appID)
    {
        return apps[appID];
    }

    public App GetCurrentApp()
    {
        return GetApp(currAppID);
    }

    public void OnNearIn(ObjectRegistration reg)
    {
        if (Config.Experiment.Multitasking == MultitaskingType.Automatic)
        {
            reg.gameObject.GetComponentInChildren<ContextMenu>().Open();
        }
    }

    public void OnNearOut(ObjectRegistration reg)
    {
        if (Config.Experiment.Multitasking == MultitaskingType.Automatic)
        {
            reg.gameObject.GetComponentInChildren<ContextMenu>().Close();
        }
    }
}
