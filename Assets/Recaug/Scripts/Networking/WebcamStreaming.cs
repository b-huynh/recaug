﻿using UnityEngine;
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
using Recaug.Networking;

public class WebcamStreaming : Singleton<WebcamStreaming> {
    // HoloLens Camera Stream
    private HoloLensCameraStream.Resolution _resolution;
    private VideoCapture _videoCapture;
    private IntPtr _spatialCoordinateSystemPtr;
    private byte[] _latestImageBytes;

    // Streaming over network
    // private int _frameNum;
    // public bool streaming = false;
    // private UdpClientUWP udpClient = null;
    public RecaugClient recaugClient = null;

    // This struct store frame related data
    public class SampleStruct {
        public float[] camera2WorldMatrix, projectionMatrix;
        public byte[] frameData;
    }

    void Start() {
        //Fetch a pointer to Unity's spatial coordinate system if you need pixel mapping
        _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

        // _frameNum = -1;
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        // InitStreaming();
	}

    // void InitStreaming() {
    // #if !UNITY_EDITOR
    //     udpClient = new UdpClientUWP();
    //     udpClient.messageReceivedEvent.AddListener(OnMessageReceived);
    //     udpClient.BindAny(Config.System.ObjectTrackingPort);
    // #endif
    // }

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

        recaugClient.OnFrameReceived(s.frameData, _resolution.width,
            _resolution.height, s.camera2WorldMatrix, s.projectionMatrix);


        // OLLLDDDDD

        // long frameCaptureTimestamp = Utils.UnixTimestampMilliseconds();

        // // Allocate byteBuffer
        // if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
        //     _latestImageBytes = new byte[sample.dataLength];

        // // Fill frame struct 
        // SampleStruct s = new SampleStruct();
        // sample.CopyRawImageDataIntoBuffer(_latestImageBytes);
        // s.frameData = _latestImageBytes;

        // // Get the cameraToWorldMatrix and projectionMatrix
        // if (!sample.TryGetCameraToWorldMatrix(out s.camera2WorldMatrix) || 
        //     !sample.TryGetProjectionMatrix(out s.projectionMatrix))
        //     return;

        // sample.Dispose();

        // // Update Frame Counter
        // _frameNum++;

        // if (streaming) {
        //     Task.Run(() => {
        //         byte[] jpegBytes = TurboJpegEncoder.EncodeImage(_resolution.width, _resolution.height, s.frameData);
        //         // byte[] framePacket = ConstructFramePacket(jpegBytes, s.camera2WorldMatrix, s.projectionMatrix);
                
        //         CameraFrameMessage message = GetCameraFrameMessage(_frameNum,
        //             jpegBytes, s.camera2WorldMatrix, s.projectionMatrix);
        //         message.frameCaptureTimestamp = frameCaptureTimestamp;
        //         message.frameSendTimestamp = Utils.UnixTimestampMilliseconds();
        //         byte[] framePacket = GetCameraFramePacket(message, jpegBytes);

        //     #if !UNITY_EDITOR
        //         udpClient.SendBytes(framePacket, Config.System.ServerIP,
        //             Config.System.ObjectTrackingPort);
        //         if (!firstSent)
        //         {
        //             firstSentTime = DateTime.Now;
        //             firstSent = true;
        //         }
        //     #endif
        //     });
        // }
    }

    private void onVideoModeStopped(VideoCaptureResult result)
    {
        Debug.Log("Video Mode Stopped");
    }

    public void OnMessageReceived(string fromHost, string fromPort, byte[] data)
    {
        // int idx = 0;

        // // Get header size
        // int[] headerSizeBytes = new int[1];
        // System.Buffer.BlockCopy(data, idx, headerSizeBytes, 0, sizeof(int));
        // idx += sizeof(int);
        // int headerSize = headerSizeBytes[0];

        // // Get header
        // byte[] headerBytes = new byte[headerSize];
        // System.Buffer.BlockCopy(data, idx, headerBytes, 0, headerSize);
        // idx += headerSize;
        // string headerStr = System.Text.Encoding.UTF8.GetString(headerBytes);
        // var message = JsonUtility.FromJson<CameraFrameMessage>(headerStr);
        // message.resultsReceiveTimestamp = Utils.UnixTimestampMilliseconds();

        // var predictions = message.results;


        // TODO: Convert data to appropriate message object.
        string json = MessageCodec.Unpack(data);
        Debug.Log("Received: " + json);
        PredictionMessage fm = JsonUtility.FromJson<PredictionMessage>(json);

        // HologramManager.Instance.OnPredictionsReceived(message, predictions);
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

    private CameraFrameMessage GetCameraFrameMessage(int frameNum,
        byte[] frameData, float[] camera2World, float[] projection)
    {
        var message = new CameraFrameMessage();
        message.type = "frame";
        message.sessionUUID = GameManager.Instance.sessionID;
        message.frameID = frameNum;
        message.payloadSize = frameData.Length;

        int mSz = sizeof(float) * 16;
        System.Buffer.BlockCopy(camera2World, 0, message.camera2World, 0, mSz);
        System.Buffer.BlockCopy(projection, 0, message.projection, 0, mSz);

        return message;
    }

    private byte[] GetCameraFramePacket(CameraFrameMessage message, 
        byte[] frameData)
    {
        string messageStr = JsonUtility.ToJson(message);
        byte[] messageData = Encoding.UTF8.GetBytes(messageStr);
        byte[] header = BitConverter.GetBytes(messageData.Length);

        byte[] data =
            new byte[header.Length + messageData.Length + frameData.Length];
        
        // Copy header that gives size of json message
        int idx = 0;
        System.Buffer.BlockCopy(header, 0, data, 0, header.Length);
        idx += header.Length;
    
        // Copy json message
        System.Buffer.BlockCopy(messageData, 0, data, idx, messageData.Length);
        idx += messageData.Length;

        // Copy payload
        System.Buffer.BlockCopy(frameData, 0, data, idx, frameData.Length);

        return data;
    }
}