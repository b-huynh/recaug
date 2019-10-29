using UnityEngine;
using HoloLensCameraStream;
using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

using Recaug;
using Recaug.Client;


public class WebcamStreaming : Singleton<WebcamStreaming> {
    // HoloLens Camera Stream
    private HoloLensCameraStream.Resolution _resolution;
    private VideoCapture _videoCapture;
    private IntPtr _spatialCoordinateSystemPtr;
    private byte[] _latestImageBytes;

    // Streaming over network
    // public bool streaming = false;

    // This struct store frame related data
    public class SampleStruct
    {
        public float[] camera2WorldMatrix, projectionMatrix;
        public byte[] frameData;
    }

    void Start()
    {
        //Fetch a pointer to Unity's spatial coordinate system if you need pixel mapping
        _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
	}

    // TODO: Remove this. Its debug only;
    private SampleStruct currentSample;
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     RecaugClient.Instance.OnFrameReceived(
        //         currentSample.frameData,
        //         _resolution.width,
        //         _resolution.height,
        //         currentSample.camera2WorldMatrix,
        //         currentSample.projectionMatrix);

        //     Debug.LogFormat("[SpaceSend] Resolution: W:{0}, H:{1}", 
        //         _resolution.width, _resolution.height);

        //     var unityCamMat =
        //         LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(
        //             currentSample.camera2WorldMatrix);
        //     var unityProjMat =
        //         LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(
        //             currentSample.projectionMatrix);

        //     Debug.LogFormat("[SpaceSend] Before Serialize Matrix: {0}",
        //         string.Join(", ", currentSample.camera2WorldMatrix));

        //     Debug.LogFormat("[SpaceSend] CamMat:\n{0} \n ProjMat:\n{1}", 
        //         unityCamMat.ToString(), unityProjMat.ToString());
        // }

        // if (Input.GetKeyDown(KeyCode.L))
        // {
        //     var projector = new ImageToWorldProjector(
        //         RecaugClient.Instance.hitmasks,
        //         new HoloLensCameraStream.Resolution(896, 504));
        //     var unityCamMat =
        //         LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(
        //             currentSample.camera2WorldMatrix);
        //     var unityProjMat =
        //         LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(
        //             currentSample.projectionMatrix);
            
        //     Vector3 outPoint;
        //     bool isHit = projector.ToWorld(
        //         ref unityCamMat,
        //         ref unityProjMat,
        //         new Vector2(448, 252),
        //         out outPoint);

        //     Debug.LogFormat("Local Hit Point: {0}", outPoint.ToString());
        // }
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
        if(v == null)
        {
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
        if(result.success == false) 
        {
            Debug.LogWarning("Could not start video mode.");
            return;
        }
        Debug.Log("Video capture started.");
    }

    private void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
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

        currentSample = s;

        sample.Dispose();

        // Pass frame to Recaug Client
        RecaugClient.Instance.OnFrameReceived(s.frameData, _resolution.width,
            _resolution.height, s.camera2WorldMatrix, s.projectionMatrix);
    }
}
