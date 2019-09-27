using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

namespace Recaug.Networking
{
    // Subscribe to events for specific received message types
    public class PredictionEvent : UnityEvent<PredictionMessage> {}

    public class MessageClient
    {
        public PredictionEvent predictionEvent;

        public MessageClient(string receivePort, string sessionID = null)
        {
            predictionEvent = new PredictionEvent();

    #if WINDOWS_UWP
            Init();
            BindAny(receivePort);
    #else
            Debug.LogWarning("[MessageClient] Only supported on UWP.");
    #endif
        }

    #if WINDOWS_UWP
        public async void SendBytes(byte[] data, string sendIP, string sendPort)
        {

            await SendBytesAsync(data, sendIP, sendPort);
        }
    #else
        public void SendBytes(byte[] data, string sendIP, string sendPort)
        {
            Debug.LogWarning("[MessageClient] Only supported on UWP.");
        }
    #endif

    // Windows UWP specific helper functions
    #if WINDOWS_UWP
        private DatagramSocket listener;

        private void Init ()
        {
            // messageReceivedEvent = new UdpMessageReceivedEvent();
            listener = new DatagramSocket();
            listener.MessageReceived += MessageReceivedCallback;
        }

        private async void BindAny(string port)
        {
            try
            {
                await listener.BindEndpointAsync(null, port);
                // HostName localHost = new HostName("192.168.100.233");
                // HostName localHost = new HostName("localhost");
                // await listener.BindEndpointAsync(localHost, port);
                Debug.LogFormat("[UdpClientUWP] Bind on {0}:{1}\n",
                    listener.Information.LocalAddress,
                    listener.Information.LocalPort);
            } 
            catch (Exception e)
            {
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

            string str = MessageCodec.Unpack(msgData);
            // Debug.Log("Original Message: " + str);
            Message m = JsonUtility.FromJson<Message>(str);
            if (m.type == "prediction")
            {
                PredictionMessage pm = 
                    JsonUtility.FromJson<PredictionMessage>(str);
                predictionEvent.Invoke(pm);
                // ThreadUtils.Instance.InvokeOnMainThread(() =>
                // {
                //     predictionEvent.Invoke(pm);
                // });
            }
        }

        private async Task SendBytesAsync(byte[] data, string sendIP,
            string sendPort)
        {
            // Debug.Log("Sending to: " + sendIP + " " + sendPort + "\n");
            HostName receiverName = new HostName(sendIP);
            string receiverPort = sendPort;
            using (var stream =
                await listener.GetOutputStreamAsync(receiverName, receiverPort))
            {
                using (var writer = new DataWriter(stream))
                {
                    writer.WriteBytes(data);
                    await writer.StoreAsync();
                }
            }
        }
    #endif

        private static MemoryStream ToMemoryStream(Stream input)
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
    }
} // namespace Recaug.Networking