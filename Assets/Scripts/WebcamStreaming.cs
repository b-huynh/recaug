using UnityEngine;
using HoloLensCameraStream;
using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class WebcamStreaming : Singleton<WebcamStreaming> {
    // HoloLens Camera Stream
    private HoloLensCameraStream.Resolution _resolution;
    private VideoCapture _videoCapture;
    private IntPtr _spatialCoordinateSystemPtr;
    private byte[] _latestImageBytes;

    // Streaming over network
    private int _frameNum;
    public bool streaming = false;
    private UdpClientUWP udpClient = null;

    // This struct store frame related data
    public class SampleStruct {
        public float[] camera2WorldMatrix, projectionMatrix;
        public byte[] frameData;
    }

    public bool firstSent = false;
    public DateTime firstSentTime;

    public bool firstReceived = false;
    public DateTime firstReceivedTime;

    void Start() {
        //Fetch a pointer to Unity's spatial coordinate system if you need pixel mapping
        _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

        _frameNum = -1;
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        InitStreaming();
	}

    void InitStreaming() {
    #if !UNITY_EDITOR
        udpClient = new UdpClientUWP();
        udpClient.messageReceivedEvent.AddListener(OnUdpMessageReceived);
        udpClient.BindAny(Config.ORListenPort);
    #endif
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(_videoCapture == null)
            return;

        _videoCapture.FrameSampleAcquired += null;
        _videoCapture.Dispose();
    }

    private void OnVideoCaptureCreated(VideoCapture v)
    {
        if(v == null) {
            Debug.LogError("No VideoCapture found");
            return;
        }

        _videoCapture = v;

        //Request the spatial coordinate ptr if you want fetch the camera and set it if you need to 
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystemPtr(_spatialCoordinateSystemPtr);

        _resolution = CameraStreamHelper.Instance.GetLowestResolution();
        float frameRate = CameraStreamHelper.Instance.GetHighestFrameRate(_resolution);

        CameraParameters cameraParams = new CameraParameters();
        cameraParams.cameraResolutionHeight = _resolution.height;
        cameraParams.cameraResolutionWidth = _resolution.width;
        cameraParams.frameRate = Mathf.RoundToInt(frameRate);
        cameraParams.pixelFormat = CapturePixelFormat.BGRA32;

        _videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;
        _videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
    }

    private void OnVideoModeStarted(VideoCaptureResult result)
    {
        if(result.success == false) {
            Debug.LogWarning("Could not start video mode.");
            return;
        }

        Debug.Log("Video capture started.");
    }

    private void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        // Allocate byteBuffer
        if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
            _latestImageBytes = new byte[sample.dataLength];

        // Fill frame struct 
        SampleStruct s = new SampleStruct();
        sample.CopyRawImageDataIntoBuffer(_latestImageBytes);
        s.frameData = _latestImageBytes;

        // Get the cameraToWorldMatrix and projectionMatrix
        if (!sample.TryGetCameraToWorldMatrix(out s.camera2WorldMatrix) || 
            !sample.TryGetProjectionMatrix(out s.projectionMatrix))
            return;

        sample.Dispose();

        // Update Frame Counter
        _frameNum++;

        if (streaming) {
            Task.Run(() => {
                Stopwatch sw = Stopwatch.StartNew();
                byte[] jpegBytes = TurboJpegEncoder.EncodeImage(_resolution.width, _resolution.height, s.frameData);
                byte[] framePacket = ConstructFramePacket(jpegBytes, s.camera2WorldMatrix, s.projectionMatrix);
                sw.Stop();
                if (DebugMode.active && ((_frameNum % 60) == 0))
                    Debug.LogFormat("Frame {0} Time to encode: {1}", _frameNum, sw.ElapsedMilliseconds);

            #if !UNITY_EDITOR
                udpClient.SendBytes(framePacket, Config.Params.ServerIP, Config.ORListenPort);
                if (!firstSent)
                {
                    firstSentTime = DateTime.Now;
                    firstSent = true;
                }
                //Debug.LogFormat("Sent to: {0}:{1}", Config.Params.ServerIP, Config.ORListenPort);
            #endif
            });
        }
    }

    private void onVideoModeStopped(VideoCaptureResult result)
    {
        Debug.Log("Video Mode Stopped");
    }

    public void OnUdpMessageReceived(string incomingIP, string incomingPort, byte[] data) {
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
        Matrix4x4 camera2WorldMatrix = 
            LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(camera2WorldBuf);
        Matrix4x4 projectionMatrix =
            LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(projectionBuf);

        // Deserialize predictions
        ImagePredictions pred = JsonUtility.FromJson<ImagePredictions>(jsonStr);

        if (!firstReceived)
        {
            firstReceivedTime = DateTime.Now;
            firstReceived = true;
        }

        HologramManager.Instance.OnPredictionsReceived(
            ref camera2WorldMatrix, ref projectionMatrix, pred);
    }
    
    private byte[] ConstructFramePacket(byte[] frameData, float[] camera2Transform, float[] projection) {
		// Allocate buffer for packet data
		int transformEncodeSize = sizeof(float) * camera2Transform.Length;
		int frameEncodeSize = sizeof(int) + frameData.Length;
		byte[] data = new byte[(transformEncodeSize * 2) + frameEncodeSize];

		// Write transforms. Each transform is a 4x4 matrix of floats.
		int dstOffset = 0;
		System.Buffer.BlockCopy(camera2Transform, 0, data, dstOffset, transformEncodeSize);
		dstOffset += transformEncodeSize;
		System.Buffer.BlockCopy(projection, 0, data, dstOffset, transformEncodeSize);
		dstOffset += transformEncodeSize;

		// Write frame size and frame.
		byte[] frameSize = BitConverter.GetBytes(frameData.Length);
		System.Buffer.BlockCopy(frameSize, 0, data, dstOffset, frameSize.Length);
		dstOffset += frameSize.Length;
		System.Buffer.BlockCopy(frameData, 0, data, dstOffset, frameData.Length);
		return data;
	}
}
