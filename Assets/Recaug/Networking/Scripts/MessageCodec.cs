using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;

namespace Recaug.Networking
{
/*
    MessageCodec decodes bytes -> Messages and Messages -> bytes
*/
public static class MessageCodec
{

    // Take a string and pack it into a byte array with leading size header
    public static byte[] Pack(string s)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        byte[] size = BitConverter.GetBytes(bytes.Length);
        byte[] rv = new byte[size.Length + bytes.Length];
        Buffer.BlockCopy(size, 0, rv, 0, size.Length);
        Buffer.BlockCopy(bytes, 0, rv, size.Length, bytes.Length);
        return rv;
    }

    // Unpack byte array by stripping size header and decoding as UTF8
    public static string Unpack(byte[] b)
    {
        int size = BitConverter.ToInt32(b, 0);   
        byte[] rv = new byte[size];
        Buffer.BlockCopy(b, sizeof(int), rv, 0, size);
        return Encoding.UTF8.GetString(rv);
    }

    // public static byte[] Encode(Message m)
    // {
    //     Debug.Log(JsonUtility.ToJson(m));
    //     byte[] jsonBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(m));
    //     byte[] size = BitConverter.GetBytes(jsonBytes.Length);
    //     byte[] rv = new byte[size.Length + jsonBytes.Length];
    //     Buffer.BlockCopy(size, 0, rv, 0, size.Length);
    //     Buffer.BlockCopy(jsonBytes, 0, rv, size.Length, jsonBytes.Length);
    //     return rv;
    // } 

    public static Message Decode(string s)
	{
        Message m = JsonUtility.FromJson<Message>(s);
        Debug.Log(JsonUtility.ToJson(m));
        if (m.type == "predictions")
        {
            PredictionMessage pm = JsonUtility.FromJson<PredictionMessage>(s);
            return pm;
        }
        else if (m.type == "frame")
        {
            FrameMessage fm = JsonUtility.FromJson<FrameMessage>(s);
            return fm;
        }
        else
        {
            return m;
        }
	}
}

} // namespace Recaug.Networking