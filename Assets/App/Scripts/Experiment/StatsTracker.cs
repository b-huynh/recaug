using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;

public class StatsTracker : Singleton<StatsTracker>
{
    public static int TotalActivities = 21;

    // Time task was started
    float timeStarted;

    // Time task was completed
    float timeCompleted;

    // Key: timestamp
    // Value: (object class, position)
    Dictionary<float, (string, Vector3)> objectDiscoveryLog =
        new Dictionary<float, (string, Vector3)>();

    // Key: timestamp
    // Value: (object class, app name, response)
    Dictionary<float, (string, string, string)> activityLog =
        new Dictionary<float, (string, string, string)>();

    // Key: timestamp
    // Value: (object class, quiz response)
    Dictionary<float, (string, string)> quizLog =
        new Dictionary<float, (string, string)>();

    // Key: timestamp
    // Value: (app name, start/stop type)
    Dictionary<float, (string, string)> appTimeLog =
        new Dictionary<float, (string, string)>();

    // Key: timestamp
    // Value: (origin app, switch app, switch type)
    Dictionary<float, (string, string, string)> switchLog = 
        new Dictionary<float, (string, string, string)>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LogStart()
    {
        timeStarted = Time.time;
    }

    public void LogComplete()
    {
        timeCompleted = Time.time;
    }

    public void LogObjectDiscovered(string className, Vector3 position)
    {
        objectDiscoveryLog[Time.time] = (className, position);
    }

    public void LogActivity(string className, string appName, string response)
    {
        activityLog[Time.time] = (className, appName, response);
    }

    public void LogQuiz(string className, string response)
    {
        quizLog[Time.time] = (className, response);
    }

    public void LogAppUsage(string appName, string eventType)
    {
        appTimeLog[Time.time] = (appName, eventType);
    }

    public void LogAppSwitch(string sourceApp, string destApp, string eventType)
    {
        switchLog[Time.time] = (sourceApp, destApp, eventType);
    }
}
