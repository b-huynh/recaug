using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using UnityEngine;

using Recaug;
using Recaug.Client;

public class GameManager : Singleton<GameManager>
{
    public string sessionID = "";

    // App Switching
    public int startAppID;
    public int currAppID { get; private set; } = -1;
    private Dictionary<int, App> apps;

    // Location of config file
    private string configHost = "192.168.100.233 ";
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
        Config.LoadHTTP(configURL);
        
        RecaugClient.Instance.Init(Config.Params.Recaug);
        sessionID = System.Guid.NewGuid().ToString();
        
        apps = new Dictionary<int, App>();
        currAppID = startAppID;
    }

    public void Start() {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchApp(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchApp(2);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SoftReset();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            HologramManager.Instance.SaveLog();
        }   
    }

    private void SoftReset()
    {
        /*
            - Load new config
            - Delete all current annotations
            - Generate new session ID
        */
        HologramManager.Instance.SaveLog();
        HologramManager.Instance.ClearLog();
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
            case "setip":
                // Config.UpdateConfig("ServerIP", tokens[1]);
                configHost = tokens[1];
                SoftReset();
                break;
        }
    }
    
    public bool SwitchApp(int appID)
    {
        if (currAppID == appID)
            return false;

        apps[currAppID].Suspend();
        apps[appID].Resume();
        AppTabs.Instance.SwitchApp(appID);
        currAppID = appID;
        return true;
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
}
