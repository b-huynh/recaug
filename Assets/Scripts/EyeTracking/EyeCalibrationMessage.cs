using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EyeMessageType
{
    DEBUG, HEAD_POSE, CALIBRATE, GAZE, CONFIRM, SUCCESS
}

[System.Serializable]
public class EyeCalibrationMessage
{
    public static int bufSize = 1024;
    public EyeMessageType type = EyeMessageType.DEBUG;
    public Matrix4x4 headPose = new Matrix4x4();
    public Vector3 calibrationPoint = new Vector3();
    public Vector3 rightGazePoint = new Vector3();
    public Vector3 leftGazePoint = new Vector3();
    public Vector3 stereoGazePoint = new Vector3();

    byte[] data = new byte[bufSize];

    public static void OverwriteData(byte[] src, byte[] dst)
    {
        if (src.Length > bufSize)
        {
            throw new ArgumentException(String.Format(
                "src array is too long, must be less than {0}", bufSize), "src");
        }
        System.Buffer.BlockCopy(src, 0, dst, 0, src.Length);
    }
}