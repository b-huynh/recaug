using UnityEngine;
using System;
using System.IO;
using System.Text;
using HoloToolkit.Unity;

public class RemoteDebug : Singleton<RemoteDebug> {
    private UdpClientUWP udpClient = null;

    public CircularBuffer<string> history = new CircularBuffer<string>(50);

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
        string msg = "";
        if (type == LogType.Log)
        {
            msg = string.Format("[{0}] {1}\n", type.ToString().ToUpper(), 
                condition);
        }
        else
        {
            msg = string.Format("[{0}] {1}{2}", type.ToString().ToUpper(), 
                condition, "\n    " + stackTrace.Replace ("\n", "\n    "));
        }

        history.PushFront(msg);

#if WINDOWS_UWP
        if (udpClient != null) {
            udpClient.SendBytes(Encoding.UTF8.GetBytes(msg),
                Config.System.ServerIP, Config.System.DebugLogPort);
        }
#endif
    }
}