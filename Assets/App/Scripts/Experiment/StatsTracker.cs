using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Storage;
#endif

using HoloToolkit.Unity;

public class StatsTracker : Singleton<StatsTracker>
{
    private string SessionID = "";

    public static int TotalActivities = 36; // The real amount for final experiment;
    // public static int TotalActivities = 18;
    public int CompletedActivities { get; private set; } = 0;

    public static int TotalObjects = 21;
    public int DiscoveredObjects { get; private set; } = 0;

    // Time task was started
    float timeStarted;

    // Time task was completed
    float timeCompleted;

    // Key: timestamp
    // Value: (object class, position)
    // Dictionary<float, (string, Vector3)> objectDiscoveryLog =
    //     new Dictionary<float, (string, Vector3)>();
    SortedDictionary<float, (string, string)> objectDiscoveryLog =
        new SortedDictionary<float, (string, string)>();

    // Key: timestamp
    // Value: (object class, app name, response)
    SortedDictionary<float, (string, string, string)> activityLog =
        new SortedDictionary<float, (string, string, string)>();

    // Key: timestamp
    // Value: (object class, quiz response)
    SortedDictionary<float, (string, string)> quizLog =
        new SortedDictionary<float, (string, string)>();

    // Key: timestamp
    // Value: (app name, start/stop type)
    SortedDictionary<float, (string, string)> appTimeLog =
        new SortedDictionary<float, (string, string)>();

    // Key: timestamp
    // Value: (origin app, switch app, switch type)
    SortedDictionary<float, (string, string, string)> switchLog = 
        new SortedDictionary<float, (string, string, string)>();

    private List<string> lines = new List<string>();
    private List<string> cameraLog = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LogCamera();
        if (Input.GetButtonDown("Log"))
        {
            WriteLog();
        }
    }

    public void LogCamera()
    {
        cameraLog.Add(String.Format("{0} | {1} | {2}",
            Time.time.ToString(),
            Camera.main.transform.position.ToString("F3"),
            Camera.main.transform.rotation.ToString("F3"))
        );
    }

    public void LogStart(string sessionID)
    {
        timeStarted = Time.time;
        SessionID = sessionID;
        lines.Add(String.Format("{0}, {1}, {2}", 
            "start", timeStarted.ToString(), SessionID));
    }

    public void LogComplete()
    {
        timeCompleted = Time.time;
        lines.Add(String.Format("{0}, {1}, {2}",
            "complete", timeCompleted.ToString(), SessionID));
        WriteLog();
        // SessionID = "";
    }

    public void LogObjectDiscovered(string className, Vector3 position)
    {
        // string positionStr = position.ToString("F3");
        string positionStr = string.Format("({0}; {1}; {2})",
            position.x.ToString("F3"), position.y.ToString("F3"), 
            position.z.ToString("F3"));
        if (DiscoveredObjects < TotalObjects)
        {
            objectDiscoveryLog[Time.time] = (className, positionStr);
            DiscoveredObjects++;
        }

        lines.Add(String.Format("{0}, {1}, {2}, {3}", 
            "object", Time.time.ToString(), className, positionStr));

        Debug.LogFormat("Object Registered: {0}, {1} / {2}",
            className, DiscoveredObjects, TotalObjects);
    }

    public void LogActivity(string className, string appName, string response)
    {
        if (CompletedActivities < TotalActivities)
        {
            activityLog[Time.time] = (className, appName, response);
            CompletedActivities++;

            lines.Add(String.Format("{0}, {1}, {2}, {3}, {4}", 
                "activity", Time.time.ToString(), className, appName, response));

            Debug.LogFormat("Complete Activities: {0} / {1}",
                CompletedActivities, TotalActivities);

            if (CompletedActivities >= TotalActivities)
            {
                LogComplete();
            }
        }
    }

    public void LogQuiz(string className, string response)
    {
        quizLog[Time.time] = (className, response);
        lines.Add(String.Format("{0}, {1}, {2}, {3}", 
            "quiz", Time.time.ToString(), className, response));
    }

    public void LogAppStart(string appName)
    {
        LogAppUsage(appName, "start");
    }
    
    public void LogAppStop(string appName)
    {
        LogAppUsage(appName, "stop");
    }

    private void LogAppUsage(string appName, string eventType)
    {
        appTimeLog[Time.time] = (appName, eventType);
        lines.Add(String.Format("{0}, {1}, {2}, {3}", 
            "appUsage", Time.time.ToString(), appName, eventType));
    }

    public void LogAppSwitch(string sourceApp, string destApp, string eventType)
    {
        switchLog[Time.time] = (sourceApp, destApp, eventType);
        lines.Add(String.Format("{0}, {1}, {2}, {3}, {4}", 
            "appSwitch", Time.time.ToString(), sourceApp, destApp, eventType));
    }

    public void LogHoverOn(string className)
    {
        lines.Add(String.Format("{0}, {1}, {2}, {3}",
            "hover", Time.time.ToString(), className, "on"));
    }

    public void LogHoverOff(string className)
    {
        lines.Add(String.Format("{0}, {1}, {2}, {3}",
            "hover", Time.time.ToString(), className, "off"));
    }

    public void LogMenuOpen()
    {
        lines.Add(String.Format("{0}, {1}, {2}",
            "menu", Time.time.ToString(), "open"));
    }

    public void LogMenuClose()
    {
        lines.Add(String.Format("{0}, {1}, {2}",
            "menu", Time.time.ToString(), "close"));
    }

    public void LogContextMenuOpen(string className)
    {
        lines.Add(String.Format("{0}, {1}, {2}, {3}",
            "contextMenu", Time.time.ToString(), className, "open"));
    }

    public void LogContextMenuClose(string className)
    {
        lines.Add(String.Format("{0}, {1}, {2}, {3}",
            "contextMenu", Time.time.ToString(), className, "close"));
    }

    public void WriteLog()
    {
        // Log output location
        string fileName = SessionID + ".log";
        string cameraFileName = SessionID + ".camera.log";

#if WINDOWS_UWP
        string filePath =
            Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName);
        string cameraFilePath =
            Path.Combine(ApplicationData.Current.LocalFolder.Path, cameraFileName);
#else
        string filePath =
            Path.Combine(Application.persistentDataPath, fileName);
        string cameraFilePath =
            Path.Combine(Application.persistentDataPath, cameraFileName);
#endif

        Debug.Log("Writing to file: " + filePath);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
        File.WriteAllLines(filePath, lines);
        File.WriteAllLines(cameraFilePath, cameraLog);
    }
}
