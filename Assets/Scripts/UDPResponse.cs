using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensCameraStream;

public class UDPResponse : MonoBehaviour {
    public TextMesh tm;
    public LayerMask visualizationMasks = Physics.DefaultRaycastLayers;
    public GameObject labelPrefab = null;
    private HoloLensCameraStream.Resolution resolution = new HoloLensCameraStream.Resolution(896, 504);
    private Dictionary<string, GameObject> objectMemory = new Dictionary<string, GameObject>();

    public void Start() {
        if (tm == null) {
            tm = gameObject.AddComponent<TextMesh>();
        }
    }

	public void Update() {
        if (DebugMode.active)
            tm.gameObject.SetActive(true);
        else
            tm.gameObject.SetActive(false);
		transform.position = Camera.main.transform.position + Camera.main.transform.forward * 9.0f;
	}

    public void ResponseToUDPPacket(string incomingIP, string incomingPort, byte[] data)
    {
        // Read matrices
        int ind = 0;
        int matrixSize = sizeof(float) * 16;
        float[] camera2WorldBuf = new float[16];
        System.Buffer.BlockCopy(data, ind, camera2WorldBuf, 0, matrixSize);
        ind += matrixSize;
        float[] projectionBuf = new float[16];
        System.Buffer.BlockCopy(data, ind, projectionBuf, 0, matrixSize);
        ind += matrixSize;
        
        // Read Predictions
        int[] jsonSize = new int[1];
        System.Buffer.BlockCopy(data, ind, jsonSize, 0, sizeof(int));
        ind += sizeof(int);
        byte[] jsonBytes = new byte[jsonSize[0]];
        System.Buffer.BlockCopy(data, ind, jsonBytes, 0, jsonSize[0]);
        string jsonStr = System.Text.Encoding.UTF8.GetString(jsonBytes);

        if (DebugMode.active) {
            Debug.LogFormat("Received Message ({0}): {1}", data.Length.ToString(), jsonStr);
        }

        // Convert to unity matrices
        Matrix4x4 camera2WorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(camera2WorldBuf);
        Matrix4x4 projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(projectionBuf);

        // Deserialize predictions
        Predictions pred = JsonUtility.FromJson<Predictions>(jsonStr);

        VisualizeObjectLabels(ref camera2WorldMatrix, ref projectionMatrix, pred);
    }

    public void VisualizeObjectLabels(ref Matrix4x4 camera2World, ref Matrix4x4 projection, Predictions pred)
    {
        Vector3 from = camera2World.GetColumn(3);
        foreach(Label la in pred.labels) {
            Vector3 rayDirection = LocatableCameraUtils.PixelCoordToWorldCoord(
                camera2World, projection, resolution, new Vector2(la.xcen, la.ycen));
            Debug.Log(la.className + ", " + la.xcen + ", " + la.ycen);

            RaycastHit hitInfo;
            bool isHit = Physics.Raycast(from, rayDirection, out hitInfo, Mathf.Infinity, visualizationMasks);

            if (isHit) {
                UpdateObjectMemory(la, hitInfo);
                Debug.LogFormat("[{0}] Pos: {1}.", la.className, hitInfo.point);
            } else {
                Debug.LogFormat("[{0}] No Hit.", la.className);
            }
        }
    }

    public void UpdateObjectMemory(Label label, RaycastHit hitInfo)
    {
        if (!objectMemory.ContainsKey(label.className)) {
            objectMemory.Add(label.className, Object.Instantiate(labelPrefab));
        }
        GameObject labelObj = objectMemory[label.className];
        labelObj.transform.position = hitInfo.point;
        labelObj.GetComponentInChildren<TextMesh>().text = label.className;
    }
}