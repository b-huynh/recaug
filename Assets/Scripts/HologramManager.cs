using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class HologramManager : Singleton<HologramManager> {
    public LayerMask visualizationMasks = Physics.DefaultRaycastLayers;
    public GameObject labelPrefab = null;
    private HoloLensCameraStream.Resolution resolution = new HoloLensCameraStream.Resolution(896, 504);
    private Dictionary<string, GameObject> objectMemory = new Dictionary<string, GameObject>();

    public void Start() {
    }

	public void Update() {
	}

    public void VisualizeObjectLabels(ref Matrix4x4 camera2World, ref Matrix4x4 projection, Predictions pred) {
        Vector3 from = camera2World.GetColumn(3);
        foreach(Label la in pred.labels) {
            Vector3 rayDirection = LocatableCameraUtils.PixelCoordToWorldCoord(
                camera2World, projection, resolution, new Vector2(la.xcen, la.ycen));
            RaycastHit hitInfo;
            bool isHit = Physics.Raycast(from, rayDirection, out hitInfo, Mathf.Infinity, visualizationMasks);
            if (isHit)
                UpdateObjectMemory(la, hitInfo);
        }
    }

    public void UpdateObjectMemory(Label label, RaycastHit hitInfo) {
        if (!objectMemory.ContainsKey(label.className)) {
            objectMemory.Add(label.className, Object.Instantiate(labelPrefab));
        }
        GameObject labelObj = objectMemory[label.className];
        labelObj.transform.position = hitInfo.point;
        labelObj.GetComponentInChildren<TextMesh>().text = label.className;
    }
}
