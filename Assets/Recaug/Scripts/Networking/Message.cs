using System;
using System.Collections.Generic;

namespace Recaug.Networking
{
    /*
        Base message class contains fields common to all messgages.
        Metadata is optional.
        Example metadata: 
            (number) frameCaptureTimestamp 
            (number) frameSendTimestamp 
            (number) frameReceiveTimestamp 
            (number) frameProcessedTimestamp 
            (number) resultsSendTimestamp 
            (number) resultsReceiveTimestamp 
     */

    [System.Serializable]
    public class Message
    {
        public string type = "frame";
        public string sessionUUID = "";
        public List<string> metadataKeys = new List<string>();
        public List<string> metadataVals = new List<string>();

        public Message(string type, string sessionUUID, 
            List<string> metadataKeys = null, List<string> metadataVals = null)
        {
            this.type = type;
            this.sessionUUID = sessionUUID;
            this.metadataKeys = metadataKeys;
            this.metadataVals = metadataVals;
        }

        public string GetMetadata(string key)
        {
            return metadataVals[metadataKeys.IndexOf(key)];
        }

        public void SetMetadata(string key, string val)
        {
            int idx = metadataKeys.IndexOf(key);
            if (idx == -1)
            {
                metadataKeys.Add(key);
                metadataVals.Add(val);
            }
            else
            {
                metadataVals[idx] = val;
            }
        }
    }

    /*
        FrameMessage is a message sent by any recaug client to the recaug-server.
        It must contain a single image frame and frame id.
    */
    [System.Serializable]
    public class FrameMessage : Message
    {
        public int frameID = -1;
        public float[] cameraMatrix = new float[16];
        public float[] projectionMatrix = new float[16];
        public string frameBase64 = "";

        // public FrameMessage(string sessionID, int id, float[] cam, float[] proj,
        //     string frame) : base("frame", sessionID)
        // {
        //     this.frameID = id;
        //     this.cameraMatrix = cam;
        //     this.projectionMatrix = proj;
        //     this.frameBase64 = frame;
        // }

        public FrameMessage(string sessionID, int id, float[] cam, float[] proj,
            byte[] frame) : base("frame", sessionID)
        {
            this.frameID = id;
            this.cameraMatrix = cam;
            this.projectionMatrix = proj;
            this.frameBase64 = Convert.ToBase64String(frame);
        }
    }



    /*
        PredictionsMessage is a exclusively generated and sent by the recaug-server.
        It contains prediction information for a particular frame id.
    */
    [System.Serializable]
    public class PredictionMessage : Message
    {
        public int frameID = -1;
        public float[] cameraMatrix = new float[16];
        public float[] projectionMatrix = new float[16];
        public List<PredBox2D> predictions = new List<PredBox2D>();
        
        public PredictionMessage(string sessionID, int frameID, float[] cam,
            float[] proj, List<PredBox2D> predictions)
        : base("prediction", sessionID)
        {
            this.frameID = frameID;
            this.cameraMatrix = cam;
            this.projectionMatrix = proj;
            this.predictions = predictions;
        }
    }

    public class MessageFactory
    {
        private string sessionID;
        private int frameCounter = 0;
        
        public MessageFactory()
        {
            this.sessionID = System.Guid.NewGuid().ToString();
        }
        
        public MessageFactory(string sessionID)
        {
            this.sessionID = sessionID;
        }

        public FrameMessage GetFrameMessage(byte[] f, float[] cam, float[] proj)
        {
            frameCounter++;
            return new FrameMessage(sessionID, frameCounter, cam, proj, f);
        }
    }

} // namespace Recaug.Networking