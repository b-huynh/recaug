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
    public LayerMask hitmasks = Physics.DefaultRaycastLayers;
    public GameObject labelPrefab = null;
    public UnityEngine.UI.Text hudAnnotation = null;
    public string defaultAnnotation = "[]";
    public SpatialMappingObserver observer = null;
    public Material debugObjectMaterial = null;
    private HoloLensCameraStream.Resolution resolution;


    // Test data for object permanence algorithm
    public bool isLogging = false;
    private List<string> point_log = new List<string>();


    private ObjectMemory objMem = null;
    private ObjectTracker objTracker = null;
    private WPFilter filter = null;
    private ImageToWorldProjector i2wProjector = null;

    public void Start() {
        point_log.Add("Timestamp, label, x, y, z"); // CSV Header

        resolution = new HoloLensCameraStream.Resolution(896, 504);

        objMem = new ObjectMemory(labelPrefab);
        objMem.debugMaterial = debugObjectMaterial;
        objTracker = new ObjectTracker(objMem);

        Debug.Log("Configured: " + Config.Loaded.ToString());

        filter = new SlidingWindowWPFilter(objMem);
        HashSet<string> toExclude = Config.Experiment.KnownObjects;
        toExclude.ExceptWith(Config.Experiment.ValidObjects);
        filter.ExcludeObjects(toExclude);

        i2wProjector = new ImageToWorldProjector(hitmasks, resolution);
        
        hudAnnotation.text = defaultAnnotation;
    }

	public void Update() {
	}

    public void OnPredictionsReceived(ref Matrix4x4 camera2World, 
        ref Matrix4x4 projection, ImagePredictions imgPreds)
    {
        WorldPredictions worldPreds = i2wProjector.ToWorldPredictions(
            ref camera2World, ref projection, imgPreds);

        if (isLogging) {
            foreach(WorldPrediction wp in worldPreds.predictions) {
                point_log.Add(System.String.Format("{0}, {1}, {2}, {3}, {4}", 
                    worldPreds.timestamp, wp.label, wp.position.x,
                    wp.position.y, wp.position.z));
            }
        }

        // // Determine if a known object has moved
        Dictionary<string, Vector3> moved = objTracker.TrackObjects(worldPreds);

        // Remove and transition
        // if (moved.Count > 0) {
        //     objMem.DisableObject(moved[0]);
        //     hudAnnotation.text = moved[0];
        // }
        foreach(var kv in moved) {
            objMem.MoveObject(kv.Key, kv.Value);
        }

        filter.AddPredictions(worldPreds);
    }

    public void SaveLog() {
        string ts = System.DateTime.Now.ToString("d_MMM_yyyy__HH-mm-ss");
        string filename = ts + ".points.log";
        string path = Path.Combine(Application.persistentDataPath, filename);

        string log_string = System.String.Join("\n", point_log);
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(log_string));
        Debug.Log("Saved point log to: " + path);
    }

    public void ResetHUD() {
        hudAnnotation.text = defaultAnnotation;
    }

}
