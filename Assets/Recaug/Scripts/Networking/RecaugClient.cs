using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Recaug.Networking;

namespace Recaug
{
    /*
        The RecaugClient is the primary interface between applications and the
        ContextService.

        RecaugClient calculates object event boundaries based on prediction
        messages received from the server. To register for these events, add a
        listener to the relevant callbacks.
    */
    public class RecaugClient : MonoBehaviour
    {
        /* 
            Catch-all container to pass data about context events to recipients.
            Valid fields are dependent on the event type.
        */
        public class ContextEventData
        {
            string objectType;
            string eventType;
            Vector3 position;
        }

        /*
            All available event types...
        */
        public class ObjectInViewEvent : UnityEvent<ContextEventData> {}
        public class ObjectNearbyEvent : UnityEvent<ContextEventData> {}

        // Context Events
        public ObjectInViewEvent objectInViewEvent;
        public ObjectNearbyEvent objectNearbyEvent;

        // Recaug Server Connection Information
        public string RecaugServerHost;
        public string RecaugServerPort;

        // MessageClient is used to communicate our protocol with the server.
        private MessageClient messageClient;
        private MessageFactory messageFactory;

        // Helpers for calculating event boundaries
        [Tooltip("Defines which layers are valid objects")]
        public LayerMask hitmasks = Physics.DefaultRaycastLayers;
        private ImageToWorldProjector projector = null;

        void Start()
        {
            objectInViewEvent = new ObjectInViewEvent();
            objectNearbyEvent = new ObjectNearbyEvent();

            messageClient = new MessageClient("12001");
            messageClient.predictionEvent.AddListener(PredictionReceivedCallback);
            messageFactory = new MessageFactory();

            var resolution = new HoloLensCameraStream.Resolution(896, 504);
            projector = new ImageToWorldProjector(hitmasks, resolution);
        }

        void Update()
        {
            
        }

        // Called when new frame is received from a frame source
        public void OnFrameReceived(Texture2D frame, float[] camMat, 
            float[] projMat)
        {
            byte[] f = frame.EncodeToJPG(30);

            // Encode as base64 string
            FrameMessage m = messageFactory.GetFrameMessage(f, camMat, projMat);
            byte[] packet = MessageCodec.Pack(JsonUtility.ToJson(m));
            messageClient.SendBytes(packet, "192.168.100.233", "12000");
        }

        // OnFrameReceived for Hololens
        public void OnFrameReceived(byte[] frame, int width, int height,
            float[] camMat, float[] projMat)
        {
            byte[] f = TurboJpegEncoder.EncodeImage(width, height, frame);

            // Encode as base64 string
            FrameMessage m = messageFactory.GetFrameMessage(f, camMat, projMat);
            byte[] packet = MessageCodec.Pack(JsonUtility.ToJson(m));
            messageClient.SendBytes(packet, "192.168.100.233", "12000");
        }

        public void PredictionReceivedCallback(PredictionMessage message)
        {
            // string s = JsonUtility.ToJson(message);
            // Debug.Log(s);

            // TODO: Project predictions
            WorldPredictions worldPreds = projector.ToWorldPredictions(
                ref message.cameraMatrix, ref message.projectionMatrix, imgPreds);

            // TODO: Filter/aggregate prediction results

            // TODO: Calculate object event boundaries and fire events.
        }
    }



}  // namespace Recaug