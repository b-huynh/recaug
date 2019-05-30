using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HoloLensCameraStream;
using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.Events;

#if !UNITY_EDITOR
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class UdpMessageReceivedEvent : UnityEvent<string, string, byte[]> {}
	
public class UdpClientUWP {
	public UdpMessageReceivedEvent messageReceivedEvent;

    public UdpClientUWP() {
#if !UNITY_EDITOR
        Init();
#else
        Debug.LogWarning("[UdpClientUWP] Not implemented in Unity Editor!");
#endif
    }

#if !UNITY_EDITOR
	private DatagramSocket listener;

	void Init () {
        messageReceivedEvent = new UdpMessageReceivedEvent();
        listener = new DatagramSocket();
        listener.MessageReceived += MessageReceivedCallback;
	}

	public async void BindAny(string port) {
		try {
			await listener.BindEndpointAsync(null, port);
			Debug.LogFormat("[UdpClientUWP] Bind on {0}:{1}",
                listener.Information.LocalAddress,
				listener.Information.LocalPort);
		} catch (Exception e) {
			Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
		}
	}

	private void MessageReceivedCallback(DatagramSocket sender,
        DatagramSocketMessageReceivedEventArgs args)
    {
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        MemoryStream ms = ToMemoryStream(streamIn);
        byte[] msgData = ms.ToArray();
		
        ThreadUtils.Instance.InvokeOnMainThread(() => {
			messageReceivedEvent.Invoke(args.RemoteAddress.DisplayName,
                listener.Information.LocalPort, msgData);
        });
	}

    private async Task SendBytesAsync(byte[] data, string sendIP,
        string sendPort)
    {
		HostName receiverName = new HostName(sendIP);
		string receiverPort = sendPort;
        using (var stream =
            await listener.GetOutputStreamAsync(receiverName, receiverPort))
        {
            using (var writer = new DataWriter(stream)) {
                writer.WriteBytes(data);
                await writer.StoreAsync();
            }
        }
    }

	public async void SendBytes(byte[] data, string sendIP, string sendPort) {
		await SendBytesAsync(data, sendIP, sendPort);
	}

    static MemoryStream ToMemoryStream(Stream input)
    {
        try
        {                                         // Read and write in
            byte[] block = new byte[0x1000];       // blocks of 4K.
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                int bytesRead = input.Read(block, 0, block.Length);
                if (bytesRead == 0) return ms;
                ms.Write(block, 0, bytesRead);
            }
        }
        finally { }
    }
#endif
}