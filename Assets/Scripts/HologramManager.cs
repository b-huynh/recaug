using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HoloToolkit.Unity;
using UnityEngine;
using File = UnityEngine.Windows.File;

public class HologramManager : Singleton<HologramManager> {
    public LayerMask visualizationMasks = Physics.DefaultRaycastLayers;
    public GameObject labelPrefab = null;
    private HoloLensCameraStream.Resolution resolution = new HoloLensCameraStream.Resolution(896, 504);
    private Dictionary<string, GameObject> objectMemory = new Dictionary<string, GameObject>();

    // Object Permanence Algorithm Params
    private float distThresh = 0.05f; // 0.05 meters

    // Test data for object permanence algorithm
    public bool isLogging = false;
    // private string debug_filename = "debug_3d_points.csv";
    private string debug_filename = null;
    private List<string> point_log = new List<string>();

    private ObjectMemory objMem = new ObjectMemory();

    public bool rawPointsMode = false;

    // Following code for IEEE VR evaluation purposes only.
    public GameObject evalOriginPoint = null;

    public void Start() {
        if (debug_filename != null) {
            point_log.Add("Timestamp, label, x, y, z"); // CSV Header
            isLogging = true;
        }

        objMem.pointPrefab = labelPrefab;

        evalOriginPoint = GameObject.Instantiate(labelPrefab);
        evalOriginPoint.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        evalOriginPoint.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        evalOriginPoint.SetActive(false);
    }

	public void Update() {
        // evalOriginPoint.SetActive(GameManager.Instance.debug);
        
        if (Input.GetKeyDown(KeyCode.O)) {
            GameObject cursor = GameObject.Find("Cursor");
            evalOriginPoint.transform.position = cursor.transform.position;
            evalOriginPoint.SetActive(true);

            foreach (GameObject cp in objMem.convergedPoints) {
                string label = cp.GetComponentInChildren<TextMesh>().text.Split('|')[0]; 
                float dist = Vector3.Distance(cp.transform.position, evalOriginPoint.transform.position);
                cp.GetComponentInChildren<TextMesh>().text = label + "| " + dist.ToString("0.0000") + "m";
            }
        }
	}

    public void VisualizeObjectLabels(ref Matrix4x4 camera2World, ref Matrix4x4 projection, Predictions pred) {
        // Create single frame timestamp to aggregate all predictions 
        string timestamp = System.DateTime.Now.Ticks.ToString();
        
        FramePredictions fp = new FramePredictions(timestamp);

        Vector3 from = camera2World.GetColumn(3);
        foreach(Label la in pred.labels) {
            Vector3 rayDirection = LocatableCameraUtils.PixelCoordToWorldCoord(
                camera2World, projection, resolution, new Vector2(la.xcen, la.ycen));
            RaycastHit hitInfo;
            bool isHit = Physics.Raycast(from, rayDirection, out hitInfo, Mathf.Infinity, visualizationMasks);
            if (isHit) {
                if (rawPointsMode) {
                    UpdateObjectMemory(timestamp, la, hitInfo);
                }

                Prediction p = new Prediction(la.className, hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
                fp.predictions.Add(p);
            }
        }

        if (!rawPointsMode) {
            objMem.ConvergePointsPerFrame(fp);
        }
    }

    public void UpdateObjectMemory(string timestamp, Label label, RaycastHit hitInfo) {
        if (hitInfo.transform.tag == "Annotation") {
            // Don't update if you hit the annotation object...
            Debug.Log("WARNING: Hitting annotation objects!!!");
            return;
        }

        // if (!objectMemory.ContainsKey(label.className)) {
        //     objectMemory.Add(label.className, GameObject.Instantiate(labelPrefab));
        // }
        // GameObject labelObj = objectMemory[label.className];
        GameObject labelObj = GameObject.Instantiate(labelPrefab);
        labelObj.transform.position = hitInfo.point;
        labelObj.GetComponentInChildren<TextMesh>().text = label.className;

        
        // Log point for debug test data
        if (debug_filename != null) {
            string log_line = System.String.Format("{0}, {1}, {2}, {3}, {4}", 
                timestamp,
                label.className,
                hitInfo.point.x.ToString(),
                hitInfo.point.y.ToString(),
                hitInfo.point.z.ToString());
            point_log.Add(log_line);
        }
    }

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
