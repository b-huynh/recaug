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
	public GameObject calibrationPointObj = null;
	public GameObject leftGazePointObj = null;
	public GameObject rightGazePointObj = null;
	public bool connected { get; private set; } = false;

#if !UNITY_EDITOR
	public StreamSocket connection = null;

	// Use this for initialization
	void Start () {
		calibrationPointObj.SetActive(false);
		leftGazePointObj.SetActive(false);
		rightGazePointObj.SetActive(false);
    }

	public async void ConnectToServer() {
		Debug.LogFormat("[EyeClientUWP] Attempt connect to {0}:{1}", Config.Params.ServerIP, Config.EyeTrackingPort);
		HostName serverHost = new HostName(Config.Params.ServerIP);
		connection = new StreamSocket();
		await connection.ConnectAsync(serverHost, Config.EyeTrackingPort);
		connected = true;
	}

	public async Task Read() {
		Debug.Log("[EyeClientUWP] Reading...");
		DataReader reader;
	
		using (reader = new DataReader(connection.InputStream))
		{
			// Set the DataReader to only wait for available data (so that we don't have to know the data size)
			reader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;
			reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
			reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

			await reader.LoadAsync(1024);

			// Keep reading until we consume the complete stream.
			while (reader.UnconsumedBufferLength > 0)
			{
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
			calibrationPointObj.SetActive(true);
			calibrationPointObj.transform.SetParent(cameraHead.transform);
			calibrationPointObj.transform.localPosition = message.calibrationPoint;
		} else if (message.type == EyeMessageType.GAZE) {
            Debug.LogFormat("[EyeClientUWP] Received GAZE message. {0} | {1}", message.leftGazePoint, message.rightGazePoint);
			leftGazePointObj.SetActive(true);
			leftGazePointObj.transform.SetParent(cameraHead.transform);
			leftGazePointObj.transform.localPosition = message.leftGazePoint;

			rightGazePointObj.SetActive(true);
			rightGazePointObj.transform.SetParent(cameraHead.transform);
			rightGazePointObj.transform.localPosition = message.rightGazePoint;
		}
	}

	public async Task Send(string message) {
		DataWriter writer;
		using (writer = new DataWriter(connection.OutputStream)) {
			Debug.Log("Writing...");
			writer.WriteString(message);
			Debug.Log("Storing...");
			await writer.StoreAsync();
			Debug.Log("Flsuhing...");
			await writer.FlushAsync();
			Debug.Log("Detaching...");
			writer.DetachStream();
		}
	}

	private async Task StartSendPoseAsync() {
		while(true) {
			EyeCalibrationMessage message = new EyeCalibrationMessage();
			message.type = EyeMessageType.HEAD_POSE;
			message.headPose = cameraHead.transform.localToWorldMatrix;
		}

	}

	public void CloseConnection() {
		if (connection != null) {
			connection.Dispose();
			connection = null;
		}
		connected = false;
	}

	// Update is called once per frame
	async void Update () {
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Begin eye tracking calibration
            Debug.Log("[CalibrateManager] Beginning...");
            ConnectToServer();
            Debug.Log("[CalibrateManager] Connected...");
            await Read();
        }
    }
#else
	void Start() {
		Debug.Log("[EyeClientUWP] WARNING: Does not function in Unity Editor!");
	}

	void Update() {

	}
#endif
}