using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;

using Recaug;

public class SoftGazeManager : Singleton<SoftGazeManager>
{
    /*
        Manages user gaze taken from the GazeManager and determines currently
        gazed at object based on the object registry. Sends events when user
        has entered and exited the gaze of an object (with hysteresis).
     */

    public event Action<ObjectRegistration> OnSoftGazeEnter = delegate {};
    public event Action<ObjectRegistration> OnSoftGazeExit = delegate {};

    private float lastChangeTime = 0.0f;
    // private static float AllowedDelay = 0.1f;
    private static float AllowedDelay = 2.0f;
    private static float EngageRadius = 10.0f; // Angle between 0 - 180
    
    public static ObjectRegistration Current = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 gazeOrigin = GazeManager.Instance.GazeOrigin;
        Vector3 gazeDir = GazeManager.Instance.GazeNormal;

        // // Hysteresis delay to account for unintended movement
        // if (Time.time - lastEnterTime > AllowedDelay)
        // {
            
        // }

        // Get nearest object
        ObjectRegistration nearest = null;
        // float maxAngle = 95.0f;
        float closestAngle = EngageRadius;
        foreach(var kv in ObjectRegistry.Instance.registry)
        {
            var registration = kv.Value;
            Vector3 objectDir = (registration.position - gazeOrigin).normalized;
            float angle = Vector3.Angle(gazeDir, objectDir);
            if (angle < closestAngle)
            {
                closestAngle = angle;
                nearest = registration;
            }
        }

        if (nearest)
        {
            Debug.LogFormat("Nearest: {0}, Angle: {1}", nearest.className, closestAngle);
        }

        // Determine if there is a nearer object.
        if (closestAngle < EngageRadius)
        {
            if (Current == null)
            {
                Current = nearest;
            }
            else if (Current != nearest)
            {
                // Start counting for AllowedDelay
                lastChangeTime = Time.time;
            }

            if (Time.time - lastChangeTime > AllowedDelay)
            {
                Current = nearest;
                // OnSoftGazeEnter(nearest);
            }
        }


    }
}
