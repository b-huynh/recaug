using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CameraFrameMessage
{
    public string type = "frame";
    public string sessionUUID = "";
    public int frameID = -1;
    public long frameCaptureTimestamp = 0;
    public long frameSendTimestamp = 0;
    public long frameReceiveTimestamp = 0;
    public long frameProcessedTimestamp = 0;
    public long resultsSendTimestamp = 0;
    public long resultsReceiveTimestamp = 0;
    public int payloadSize = 0;
    public float[] camera2World = new float[16];
    public float[] projection = new float[16];
    public ImagePredictions results = new ImagePredictions();
    public List<WorldPoint> projected = new List<WorldPoint>();
}