﻿using UnityEngine;
using System;
using System.IO;
using System.Text;
using HoloToolkit.Unity;

public class RemoteDebug : Singleton<RemoteDebug> {
    private UdpClientUWP udpClient = null;

    protected override void Awake() {
        base.Awake();
        udpClient = new UdpClientUWP();
    }

    public void OnEnable() 
    {
        Application.logMessageReceived += HandlelogMessageReceived;
    }

    public void OnDisable() 
    {
        Application.logMessageReceived -= HandlelogMessageReceived;
    }

    public void HandlelogMessageReceived(string condition, string stackTrace,
        LogType type)
    {
        string msg = string.Format("[{0}] {1}{2}", type.ToString().ToUpper(), 
            condition, "\n    " + stackTrace.Replace ("\n", "\n    "));
#if WINDOWS_UWP
        if (udpClient != null) {
            udpClient.SendBytes(Encoding.UTF8.GetBytes(msg),
                Config.System.ServerIP, Config.System.DebugLogPort);
        }
#endif
    }
}