using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;

// #if WINDOWS_UWP
// using Windows.Networking.Sockets;
// using Windows.Networking.Connectivity;
// using Windows.Networking;
// #endif

public class DebugLogBroadcaster : MonoBehaviour 
{
	private string debugServerIP = "192.168.2.134";
	private string broadcastPort = "9999";

    private UdpClientUWP udpClient = null;

    void Start() {
#if !UNITY_EDITOR
        udpClient = new UdpClientUWP();
        // udpClient.messageReceivedEvent.AddListener(OnUdpMessageReceived);
        // udpClient.BindAny(Config.ORListenPort);
#endif
    }

    void OnEnable() 
    {
        Application.logMessageReceived += HandlelogMessageReceived;
        Debug.Log("DebugLogBroadcaster started on port:" + broadcastPort);
    }

    void OnDisable() 
    {
        Application.logMessageReceived -= HandlelogMessageReceived;
    }

    void HandlelogMessageReceived (string condition, string stackTrace, LogType type)
    {
        string msg = string.Format ("[{0}] {1}{2}", 
                                   type.ToString ().ToUpper (), 
                                   condition,
                                   "\n    " + stackTrace.Replace ("\n", "\n    "));
#if !UNITY_EDITOR
        udpClient.SendBytes(Encoding.UTF8.GetBytes(msg), debugServerIP, broadcastPort);
        // UDPCommunication comm = UDPCommunication.Instance;
        // if (comm.isReady)
        //     comm.SendUDPMessage(debugServerIP, broadcastPort.ToString(), Encoding.UTF8.GetBytes(msg));
#endif
    }
}