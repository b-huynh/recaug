using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using HoloToolkit.Unity;

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



        // All available event types...
        public event Action<ObjectRegistration> OnNearIn = delegate {};
        public event Action<ObjectRegistration> OnNearOut = delegate {};
        // public event Action<ObjectRegistration> OnViewIn;
        // public event Action<ObjectRegistration> OnViewOut;
        
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

            messageClient = new MessageClient("12001");
            messageClient.predictionEvent.AddListener(OnPredictionReceived);
            messageFactory = new MessageFactory();
        }

        void Update()
        {
            UpdateEvents();
            // TODO: Calculate In View
            
        }

        void UpdateEvents()
        {
            UpdateNearIn();
            UpdateNearOut();
            UpdateViewIn();
            UpdateViewOut();
        }

        /* Calculate Events */
        private float nearbyBoundary = 1.75f;
        private HashSet<ObjectRegistration> near = 
            new HashSet<ObjectRegistration>();
        void UpdateNearIn()
        {
            // Calculate Nearby
            var nearby = ObjectRegistry.Instance.Nearby(
                Camera.main.transform.position, nearbyBoundary);
            foreach(var kv in nearby)
            {
                ObjectRegistration registration = kv.Value;
                // If object is nearby AND it is in view
                if (!near.Contains(registration) && InView(registration))
                {
                    near.Add(registration);
                    OnNearIn(registration);
                }
            }
        }


        void UpdateNearOut()
        {
            HashSet<ObjectRegistration> toRemove = 
                new HashSet<ObjectRegistration>();
            foreach(var registration in near)
            {
                float distance = Vector3.Distance(registration.position,
                    Camera.main.transform.position);
                // If object no longer nearby OR it is not in view
                if (distance > nearbyBoundary || !InView(registration))
                {
                    OnNearOut(registration);
                    toRemove.Add(registration);
                }
            }

            foreach(var registration in toRemove)
            {
                near.Remove(registration);
            }
        }

        void UpdateViewIn()
        {

        }

        void UpdateViewOut()
        {

        }

        // Helper function for detecting if an object is in view.
        private bool InView(ObjectRegistration reg)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(reg.position);
            if (vp.x >= 0.0f && vp.x <= 1.0f && 
                vp.y >= 0.0f && vp.y <= 1.0f && 
                vp.z > 0.0f)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            messageClient.SendBytes(packet, GameManager.Instance.configHost, "12000");
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
            messageClient.SendBytes(packet, GameManager.Instance.configHost, "12000");
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

            List<PredPoint2D> points2D = message.predictions.Select(p => 
                new PredPoint2D(
                    p.className,
                    p.confidence,
                    new Vector2(p.xcen, p.ycen),
                    new Color(p.cen_r, p.cen_g, p.cen_b))
            ).ToList();

            ThreadUtils.Instance.InvokeOnMainThread(() =>
            {
                List<PredPoint3D> points3D;
                bool isHit = projector.ToWorld(
                    ref camMat, ref projMat, points2D, out points3D);

                // Debug.Log("Step 1");

                // List<PredBox3D> boxes3D;
                // bool isHit = projector.ToWorld(
                //     ref camMat, ref projMat, message.predictions, out boxes3D);

                // foreach(var bbb in boxes3D)
                // {
                //     Debug.LogFormat("box: {0}, {1}, {2}", bbb.xcen, bbb.ycen, bbb.zcen);
                // }


                if(isHit)
                {
                    // Debug.Log("Step 2");
                    // List<PredPoint3D> points3D = new List<PredPoint3D>();
                    // foreach(var box in boxes3D)
                    // {
                    //     points3D.Add(
                    //         new PredPoint3D(
                    //             box, new Vector3(box.xcen, box.ycen, box.zcen)));
                    // }

                    // Debug.Log("Step 3");

                    // Filter/aggregate prediction results, Object 'Discovery'
                    List<PredPoint3D> discovered = filter.Filter(points3D);

                    // Debug.Log("Step 4");

                    // Update registry with discovered objects
                    foreach(PredPoint3D point in discovered)
                    {
                        
                        // Debug.LogFormat("Points3D: {0}, {1}, {2}", point.x, point.y, point.z);
                        // Debug.Log("Step 5");
                        // int idx = points3D.FindIndex(p => 
                        //     p.x == point.x && p.y == point.y && p.z == point.z
                        // );
                        // var b = boxes3D[idx];

                        // Debug.LogFormat("Index: {0}", idx);

                        // Debug.Log("Step 6");
                        var registration = ObjectRegistry.Instance.Register(
                            point.className, point.position);
                        
                        // if (registration != null)
                        // {
                        //     registration.UpdateExtents(b);
                        // }
                    }

                    // TODO: Tracking
                }
            });
        }
    }
}  // namespace Recaug.Client