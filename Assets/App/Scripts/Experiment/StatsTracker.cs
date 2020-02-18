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
    Dictionary<int, (string, Vector3)> objectDiscoveryLog =
        new Dictionary<int, (string, Vector3)>();

    // Key: timestamp
    // Value: (object class, app name, response)
    Dictionary<int, (string, string, string)> activityLog =
        new Dictionary<int, (string, string, string)>();

    // Key: timestamp
    // Value: (object class, quiz response)
    Dictionary<int, (string, string)> quizLog =
        new Dictionary<int, (string, string)>();

    // Key: timestamp
    // Value: (app name, start/stop type)
    Dictionary<int, (string, string)> appTimeLog =
        new Dictionary<int, (string, string)>();

    // Key: timestamp
    // Value: (origin app, switch app, switch type)
    Dictionary<int, (string, string, string)> switchLog = 
        new Dictionary<int, (string, string, string)>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
