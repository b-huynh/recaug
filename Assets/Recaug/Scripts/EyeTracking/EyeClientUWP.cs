using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HoloToolkit.Unity;
using UnityEngine;

#if !UNITY_EDITOR
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class EyeClientUWP : Singleton<EyeClientUWP> {
	public GameObject cameraHead = null;
    public GameObject pointPrefab = null;
    public GameObject cursor = null;
	private GameObject calibrationPointObj = null;
    private GameObject stereoGazePointObj = null;
	public bool connected { get; private set; } = false;
    public bool isCalibrated { get; private set; } = false;

    private bool useEyeCursor = false;


#if !UNITY_EDITOR
    public StreamSocket connection = null;
    private UdpClientUWP udpClient = null;

	// Use this for initialization
	void Start () {
        // Instantiate point objects
        calibrationPointObj = GameObject.Instantiate(pointPrefab);
        calibrationPointObj.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        calibrationPointObj.transform.SetParent(cameraHead.transform);
        calibrationPointObj.SetActive(false);

        stereoGazePointObj = GameObject.Instantiate(pointPrefab);
        stereoGazePointObj.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        stereoGazePointObj.transform.SetParent(cameraHead.transform);
        stereoGazePointObj.transform.position = cameraHead.transform.forward;
        stereoGazePointObj.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        stereoGazePointObj.SetActive(useEyeCursor);

        cursor.SetActive(!useEyeCursor);
        // cursor.transform.SetParent(cameraHead.transform);
        // cursor.GetComponent<ObjectCursor>().ParentTransform = cameraHead.transform;

        udpClient = new UdpClientUWP();
        udpClient.messageReceivedEvent.AddListener(OnUdpMessageReceived);
        udpClient.BindAny(Config.System.EyeTrackingPort);
    }

	// Update is called once per frame
	async void Update () {
        if (Input.GetKeyDown(KeyCode.M)) {
            // Begin eye tracking calibration
            Debug.Log("[EyeClientUWP] Connecting to Eye Server for Calibration...");
            ConnectToServer();
            await Read();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            useEyeCursor = !useEyeCursor;
            stereoGazePointObj.SetActive(useEyeCursor);
            cursor.SetActive(!useEyeCursor);
        }

        if (!isCalibrated) {
            VisualizeGaze(stereoGazePointObj.transform.position);
        }
    }

    public void OnUdpMessageReceived(string incomingIP, string incomingPort, byte[] data) {
        if (!isCalibrated) {
            // Data is junk. Return.
            return;
        }

        // Read Message Header
        int ind = 0;
        int[] jsonSize = new int[1];
        System.Buffer.BlockCopy(data, 0, jsonSize, 0, sizeof(int));
        ind += sizeof(int);

        // Read Json Message
        byte[] jsonBytes = new byte[jsonSize[0]];
        System.Buffer.BlockCopy(data, ind, jsonBytes, 0, jsonSize[0]);
        string jsonStr = System.Text.Encoding.UTF8.GetString(jsonBytes);

        if (DebugMode.active)
            Debug.LogFormat("Received Message ({0}): {1}", data.Length.ToString(), jsonStr);

        // Deserialize EyeMessage
        EyeCalibrationMessage eyeMessage = JsonUtility.FromJson<EyeCalibrationMessage>(jsonStr);
        if (eyeMessage.type == EyeMessageType.GAZE) {
            // Vector3 gazePoint = cameraHead.transform.TransformPoint(eyeMessage.stereoGazePoint);

            stereoGazePointObj.transform.localPosition = eyeMessage.stereoGazePoint;
            VisualizeGaze(stereoGazePointObj.transform.position);
        } else {
            Debug.LogFormat("Received Non-Gaze Message through UDP...");
        }
    }

    private void VisualizeGaze(Vector3 gazePoint) {
        RaycastHit hitInfo;
        Vector3 rayDirection = (gazePoint - cameraHead.transform.position).normalized;
        bool isHit = Physics.Raycast(cameraHead.transform.position, rayDirection, out hitInfo, Mathf.Infinity);
        
        if (isHit) {
            Vector3 userFacingDirection = (cameraHead.transform.position - hitInfo.point).normalized;
            Vector3 hitPosWithOffset = hitInfo.point + (userFacingDirection * 0.03f);
            stereoGazePointObj.transform.position = hitPosWithOffset;
            // stereoGazePointObj.SetActive(true);
        }
    }

    public async void ConnectToServer() {
        try {
            Debug.LogFormat("[EyeClientUWP] Attempt connect to {0}:{1}", Config.System.ServerIP, Config.System.EyeTrackingPort);
            HostName serverHost = new HostName(Config.System.ServerIP);
            connection = new StreamSocket();
            await connection.ConnectAsync(serverHost, Config.System.EyeTrackingPort);
            Debug.Log("[EyeClientUWP] Connected");
            connected = true;
        } catch (Exception ex) {
            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.LogFormat("[EyeClientUWP:ConnectToServer] Exception: {0}", webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
        }
	}

	public async Task Read() {
		Debug.Log("[EyeClientUWP] Waiting for packets...");
		DataReader reader;
	
		using (reader = new DataReader(connection.InputStream)) {
			// Set the DataReader to only wait for available data (so that we don't have to know the data size)
			reader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;
			reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
			reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

			await reader.LoadAsync(1024);

			// Keep reading until we consume the complete stream.
			while (reader.UnconsumedBufferLength > 0) {
				ReadPacket(ref reader);
				await reader.LoadAsync(1024); // Wait for more data
			}

			reader.DetachStream();
		}
	}

	private void ReadPacket(ref DataReader reader) {
		int messageSize = reader.ReadInt32();
		String jsonMessage = reader.ReadString((uint)messageSize);
		EyeCalibrationMessage message = JsonUtility.FromJson<EyeCalibrationMessage>(jsonMessage);

		if (message.type == EyeMessageType.CALIBRATE) {
            Debug.LogFormat("[EyeClientUWP] Received CALIBRATE message. {0}",
                message.calibrationPoint);
			calibrationPointObj.transform.localPosition = message.calibrationPoint;
            calibrationPointObj.SetActive(true);
		} else if (message.type == EyeMessageType.SUCCESS) {
            isCalibrated = true;
            Debug.Log("[EyeClientUWP] Eye Calibration Complete");
            calibrationPointObj.SetActive(false);
        }
	}

	// public async Task Send(string message) {
	// 	DataWriter writer;
	// 	using (writer = new DataWriter(connection.OutputStream)) {
	// 		Debug.Log("Writing...");
	// 		writer.WriteString(message);
	// 		Debug.Log("Storing...");
	// 		await writer.StoreAsync();
	// 		Debug.Log("Flsuhing...");
	// 		await writer.FlushAsync();
	// 		Debug.Log("Detaching...");
	// 		writer.DetachStream();
	// 	}
	// }

	// private async Task StartSendPoseAsync() {
	// 	while(true) {
	// 		EyeCalibrationMessage message = new EyeCalibrationMessage();
	// 		message.type = EyeMessageType.HEAD_POSE;
	// 		message.headPose = cameraHead.transform.localToWorldMatrix;
	// 	}

	// }

	public void CloseConnection() {
		if (connection != null) {
			connection.Dispose();
			connection = null;
		}
		connected = false;
	}

#else
	void Start() {
		Debug.Log("[EyeClientUWP] WARNING: Does not function in Unity Editor!");
	}

	void Update() {

	}
#endif
}