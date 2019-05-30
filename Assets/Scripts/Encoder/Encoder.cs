using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Encoder {

	// public static void SendFrame(byte[] rawData, float[] camera2Transform, float[] projection, JPGEncoder.BitmapData data)
	// {
    //     Stopwatch sw = Stopwatch.StartNew();
	// 	JPGEncoder enc = new JPGEncoder(data, 25);
	// 	byte[] jpegBytes = null;
	// 	while (!enc.isDone) {}

	// 	jpegBytes = enc.GetBytes();
    //     sw.Stop();
    //     Debug.LogFormat("JPGEncoder Done. Length: {0}, ElapsedTime: {1}", jpegBytes.Length, sw.ElapsedMilliseconds);

	// 	// encodeTexture.LoadRawTextureData(rawData);
	// 	// byte[] jpegBytes = encodeTexture.EncodeToJPG(25);
	// 	byte[] packetBytes = CreateFrameAndTransformPacket(jpegBytes, camera2Transform, projection);

	// 	UDPCommunication comm = UDPCommunication.Instance;
	// 	if (comm.isReady)
	// 		comm.SendUDPMessage(comm.externalIP, comm.externalPort, packetBytes);
	// }

	public static byte[] CreateFrameAndTransformPacket(byte[] frameData, float[] camera2Transform, float[] projection)
	{
		// Allocate buffer for packet data
		int transformEncodeSize = sizeof(float) * camera2Transform.Length;
		int frameEncodeSize = sizeof(int) + frameData.Length;
		byte[] data = new byte[(transformEncodeSize * 2) + frameEncodeSize];


		// Write transforms. Each transform is a 4x4 matrix of floats.
		int dstOffset = 0;
		System.Buffer.BlockCopy(camera2Transform, 0, data, dstOffset, transformEncodeSize);
		dstOffset += transformEncodeSize;
		System.Buffer.BlockCopy(projection, 0, data, dstOffset, transformEncodeSize);
		dstOffset += transformEncodeSize;

		// Write frame size and frame.
		byte[] frameSize = BitConverter.GetBytes(frameData.Length);
		System.Buffer.BlockCopy(frameSize, 0, data, dstOffset, frameSize.Length);
		dstOffset += frameSize.Length;
		System.Buffer.BlockCopy(frameData, 0, data, dstOffset, frameData.Length);
		return data;
	}
}
