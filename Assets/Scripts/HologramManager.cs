using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HoloToolkit.Unity;
using UnityEngine;
using File = UnityEngine.Windows.File;

public class HologramManager : Singleton<HologramManager> {
    public LayerMask hitmasks = Physics.DefaultRaycastLayers;
    private HoloLensCameraStream.Resolution resolution;
    public GameObject labelPrefab = null;

    // Test data for object permanence algorithm
    public bool isLogging = false;
    // private string debug_filename = "debug_3d_points.csv";
    private string debug_filename = null;
    private List<string> point_log = new List<string>();

    private ObjectMemory objMem = null;
    private WPFilter filter = null;
    private ImageToWorldProjector i2wProjector = null;

    public void Start() {
        if (debug_filename != null) {
            point_log.Add("Timestamp, label, x, y, z"); // CSV Header
            isLogging = true;
        }

        resolution = new HoloLensCameraStream.Resolution(896, 504);

        objMem = new ObjectMemory(labelPrefab);
        filter = new SlidingWindowWPFilter(objMem);
        i2wProjector = new ImageToWorldProjector(hitmasks, resolution);
    }

	public void Update() {
	}

    public void OnPredictionsReceived(ref Matrix4x4 camera2World, 
        ref Matrix4x4 projection, ImagePredictions imgPreds)
    {
        WorldPredictions worldPreds = i2wProjector.ToWorldPredictions(
            ref camera2World, ref projection, imgPreds);
        filter.AddPredictions(worldPreds);
    }

    // public void UpdateObjectMemory(string timestamp, Label label, RaycastHit hitInfo) {
    //     if (hitInfo.transform.tag == "Annotation") {
    //         // Don't update if you hit the annotation object...
    //         Debug.Log("WARNING: Hitting annotation objects!!!");
    //         return;
    //     }

    //     // if (!objectMemory.ContainsKey(label.className)) {
    //     //     objectMemory.Add(label.className, GameObject.Instantiate(labelPrefab));
    //     // }
    //     // GameObject labelObj = objectMemory[label.className];
    //     GameObject labelObj = GameObject.Instantiate(labelPrefab);
    //     labelObj.transform.position = hitInfo.point;
    //     labelObj.GetComponentInChildren<TextMesh>().text = label.className;

        
    //     // Log point for debug test data
    //     if (debug_filename != null) {
    //         string log_line = System.String.Format("{0}, {1}, {2}, {3}, {4}", 
    //             timestamp,
    //             label.className,
    //             hitInfo.point.x.ToString(),
    //             hitInfo.point.y.ToString(),
    //             hitInfo.point.z.ToString());
    //         point_log.Add(log_line);
    //     }
    // }

    public void SaveLog() {
        if (debug_filename == null)
            return;

        string log_string = System.String.Join("\n", point_log);
        string path = Path.Combine(Application.persistentDataPath, debug_filename);
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(log_string));
        Debug.Log("Saved point log to: " + path);
        isLogging = false;
    }
}
