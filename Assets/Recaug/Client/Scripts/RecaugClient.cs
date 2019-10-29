using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using Recaug.Networking;

namespace Recaug.Client
{
    public class ContextEventData
    {
        public string objectType;
        public string eventType;
        public Vector3 position;
    }

    /*
        The RecaugClient is the primary interface between applications and the
        ContextService.

        RecaugClient calculates object event boundaries based on prediction
        messages received from the server. To register for these events, add a
        listener to the relevant callbacks.
    */
    public class RecaugClient : Singleton<RecaugClient>
    {
        /* 
            Catch-all container to pass data about context events to recipients.
            Valid fields are dependent on the event type.
        */


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
        private SlidingWindowWPFilter filter = null;
        // private NaiveWPFilter filter = null;

        protected override void Awake()
        {
            base.Awake();
            
            objectInViewEvent = new ObjectInViewEvent();
            objectNearbyEvent = new ObjectNearbyEvent();

            messageClient = new MessageClient("12001");
            messageClient.predictionEvent.AddListener(OnPredictionReceived);
            messageFactory = new MessageFactory();
        }

        void Update()
        {
            // Calculate Nearby
            float nearbyBoundary = 1.0f;
            var nearby = ObjectRegistry.Instance.Nearby(
                Camera.main.transform.position, nearbyBoundary);
            foreach(var kv in nearby)
            {
                ContextEventData eventData = new ContextEventData();
                eventData.eventType = "ObjectNearby";
                eventData.objectType = kv.Value.className;
                eventData.position = kv.Value.position;
                objectNearbyEvent.Invoke(eventData);
            }

            // TODO: Calculate In View
            
        }

        public void Init(RecaugClientConfig config)
        {
            // Init Projector
            var resolution = new HoloLensCameraStream.Resolution(
                config.CameraWidth, config.CameraHeight);
            projector = new ImageToWorldProjector(hitmasks, resolution);
            
            // Init filter / object discovery algorithm
            filter = new SlidingWindowWPFilter(
                config.FilterWindowSize,
                config.FilterWindowMinCount,
                config.FilterMinDist);
            // filter = new NaiveWPFilter();
            HashSet<string> toExclude = config.KnownObjects;
            toExclude.ExceptWith(config.ValidObjects);
            filter.ExcludeObjects(toExclude);
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

            // Debug.LogFormat("[OnFrameReceived] Before Serialize Matrix: {0}", string.Join(", ", camMat));
            // Debug.LogFormat("[OnFrameReceived] Serialized Send Message: {0}", JsonUtility.ToJson(m));

            byte[] packet = MessageCodec.Pack(JsonUtility.ToJson(m));
            messageClient.SendBytes(packet, "192.168.100.233", "12000");
        }

        public void OnPredictionReceived(PredictionMessage message)
        {
            // Project predictions
            Matrix4x4 camMat =
                LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(
                    message.DecodedCameraMatrix());
            Matrix4x4 projMat = 
                LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(
                    message.DecodedProjectionMatrix());

            // Debug.LogFormat("Received Message: {0}", JsonUtility.ToJson(message));

            // Debug.Log("PM Predictions Length: " + message.predictions.Count.ToString());

            // foreach(var p in message.predictions)
            // {
            //     Debug.LogFormat("Predicted {0}, {1},{2}", p.className, p.xcen.ToString(), p.ycen.ToString());
            // }

            List<PredPoint2D> points2D = message.predictions.Select(p => 
                new PredPoint2D(
                    p.className,
                    p.confidence,
                    new Vector2(p.xcen, p.ycen),
                    new Color(p.cen_r, p.cen_g, p.cen_b))
            ).ToList();

            // Debug.Log("points2D size: " + points2D.Count.ToString());

            // foreach(var p in points2D)
            //     Debug.LogFormat("Predict {0} at {1}", p.className, p.position);



            ThreadUtils.Instance.InvokeOnMainThread(() =>
            {
                List<PredPoint3D> points3D;
                bool isHit = projector.ToWorld(
                    ref camMat, ref projMat, points2D, out points3D);

                if(isHit)
                {
                    // Debug.Log("Hit...");
                    // foreach(var p in points3D)
                    //     Debug.LogFormat("Hit {0} at {1}", p.className, p.position);

                    // Filter/aggregate prediction results, Object 'Discovery'
                    List<PredPoint3D> discovered = filter.Filter(points3D);
                    
                    // foreach(var p in discovered)
                    //     Debug.LogFormat("Discover {0} at {1}", p.className, p.position);

                    // Update registry with discovered objects
                    foreach(PredPoint3D p in discovered)
                        ObjectRegistry.Instance.Register(p.className, p.position);
                
                    // TODO: Tracking
                }
            });
        }
    }
}  // namespace Recaug.Client