using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;
using File = UnityEngine.Windows.File;

public class HologramManager : Singleton<HologramManager> {
    // Test data for object permanence algorithm
    public bool isLogging = false;
    private List<string> point_log = new List<string>();
    public List<string> confirm_log = new List<string>();

    public void Start()
    {
        
    }

	public void Update()
    {
	}

    // public void OnPredictionsReceived(ref Matrix4x4 camera2World, 
    // //     ref Matrix4x4 projection, ImagePredictions imgPreds)
    // public void OnPredictionsReceived(CameraFrameMessage message,
    //     ImagePredictions imgPreds)
    // {
    //     Matrix4x4 camera2World =
    //         LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(message.camera2World);
    //     Matrix4x4 projection = 
    //         LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(message.projection);

    //     WorldPredictions worldPreds = i2wProjector.ToWorldPredictions(
    //         ref camera2World, ref projection, imgPreds);

    //     foreach(var wp in worldPreds.predictions)
    //     {
    //         var worldPoint = new WorldPoint();
    //         worldPoint.label = wp.label;
    //         worldPoint.x = wp.position.x;
    //         worldPoint.y = wp.position.y;
    //         worldPoint.z = wp.position.z;
    //         worldPoint.r = wp.color.r;
    //         worldPoint.g = wp.color.g;
    //         worldPoint.b = wp.color.b;
    //         message.projected.Add(worldPoint);
    //     }

    //     if (Config.Experiment.SaveLogs) {
    //         point_log.Add(JsonUtility.ToJson(message));
    //         // foreach(WorldPrediction wp in worldPreds.predictions) {
    //         //     point_log.Add(System.String.Format("{0}, {1}, {2}, {3}, {4}", 
    //         //         worldPreds.timestamp, wp.label, wp.position.x,
    //         //         wp.position.y, wp.position.z));
    //         // }
    //     }

    //     // // // Determine if a known object has moved
    //     // Dictionary<string, Vector3> moved = objTracker.TrackObjects(worldPreds);

    //     // // Remove and transition
    //     // // if (moved.Count > 0) {
    //     // //     objMem.DisableObject(moved[0]);
    //     // //     hudAnnotation.text = moved[0];
    //     // // }
    //     // foreach(var kv in moved) {
    //     //     objMem.MoveObject(kv.Key, kv.Value);
    //     // }

    //     filter.AddPredictions(worldPreds);

    //     objMem.AddPredictions(worldPreds);

    // }

    public void SaveLog() {
        // Save Log
        SavePointLog();

        // Confirm Log
        SaveConfirmLog();
    }

    private void SavePointLog()
    {
        string ts = System.DateTime.Now.ToString("d_MMM_yyyy__HH-mm-ss");
        string filename = System.String.Format(
            "{0}.{1}.points.log", ts, GameManager.Instance.sessionID);
        string path = Path.Combine(Application.persistentDataPath, filename);

        string log_string = System.String.Join("\n", point_log);
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(log_string));
        Debug.Log("Saved point log to: " + path);
    }

    private void SaveConfirmLog()
    {
        string ts = System.DateTime.Now.ToString("d_MMM_yyyy__HH-mm-ss");
        string filename = System.String.Format(
            "{0}.{1}.confirm.log", ts, GameManager.Instance.sessionID);
        string path = Path.Combine(Application.persistentDataPath, filename);

        string log_string = System.String.Join("\n", confirm_log);
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(log_string));
        Debug.Log("Saved confirm log to: " + path);
    }

    public void ClearLog()
    {
        point_log.Clear();
        confirm_log.Clear();
    }
}
