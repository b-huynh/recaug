using UnityEngine;
using HoloLensCameraStream;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

/// <summary>
/// In this example, we back-project to the 3D world 5 pixels, which are the principal point and the image corners,
/// using the extrinsic parameters and projection matrices.
/// Whereas the app is running, if you tap on the image, this set of points is reprojected into the world.
/// </summary>
public class ProjectionExample : MonoBehaviour {
    // HoloLens Camera Stream
    private HoloLensCameraStream.Resolution _resolution;
    private VideoCapture _videoCapture;
    private IntPtr _spatialCoordinateSystemPtr;
    private byte[] _latestImageBytes;
    private bool stopVideo;
    private UnityEngine.XR.WSA.Input.GestureRecognizer _gestureRecognizer;

    // Streaming over network
    private int _frameNum;
    public bool streaming = false;

    // This struct store frame related data
    private class SampleStruct
    {
        public float[] camera2WorldMatrix, projectionMatrix;
        public byte[] data;
    }

    void Awake()
    {
        // Create and set the gesture recognizer
        _gestureRecognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        _gestureRecognizer.SetRecognizableGestures(UnityEngine.XR.WSA.Input.GestureSettings.Tap);
        _gestureRecognizer.StartCapturingGestures();
    }

	void Start() 
    {
        //Fetch a pointer to Unity's spatial coordinate system if you need pixel mapping
        _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
	    CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        _frameNum = -1;
	}

    void Update() {
		if (Input.GetKeyDown(KeyCode.P)) {
			streaming = !streaming;
		}
    }

    private void OnDestroy()
    {
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
        s.data = _latestImageBytes;

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
                byte[] jpegBytes = TurboJpegEncoder.EncodeImage(_resolution.width, _resolution.height, s.data);
                byte[] framePacket = Encoder.CreateFrameAndTransformPacket(jpegBytes, s.camera2WorldMatrix, s.projectionMatrix);
                sw.Stop();
                if (DebugMode.active && ((_frameNum % 60) == 0))
                    Debug.LogFormat("Frame {0} Time to encode: {1}", _frameNum, sw.ElapsedMilliseconds);

            #if !UNITY_EDITOR
                UDPCommunication comm = UDPCommunication.Instance;
                if (comm.isReady)
                    comm.SendUDPMessage(comm.externalIP, comm.externalPort, framePacket);
            #endif
            });
        }

        // ThreadUtils.Instance.InvokeOnMainThread(() =>
        // {
        //     // _pictureTexture.LoadRawTextureData(s.data);
        // });
    }

    private void onVideoModeStopped(VideoCaptureResult result)
    {
        Debug.Log("Video Mode Stopped");
    }
}
