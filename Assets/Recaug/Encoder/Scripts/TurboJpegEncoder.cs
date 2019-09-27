using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System;
using System.IO;

public static class TurboJpegEncoder {

	[DllImport ("turbojpeg")]
	private static extern unsafe void* tjInitCompress();

	[DllImport ("turbojpeg")]
	private static extern unsafe int tjCompress2(void* handle, byte[] srcBuf,
		int width, int pitch, int height, int pixelFormat, byte** jpegBuf, ulong* jpegSize,
		int jpegSubsamp, int jpegQual, int flags);
	
	// [DllImport ("turbojpeg")]
	// private static extern unsafe char* tjGetErrorStr2(void* handle);

	[DllImport ("turbojpeg")]
	private static extern unsafe int tjDestroy(void* handle);

	[DllImport ("turbojpeg")]
	private static extern unsafe void tjFree(byte* buffer);

	// private int TJSAMP_GRAY = 3;
	// private int TJSAMP_411 = 5;
	// private int TJPF_RGB = 0;
	// private int TJPF_GRAY = 6;
	// private int TJPF_RGBA = 7;
	enum TJPF
	{
		TJPF_RGB = 0,
		TJPF_BGR,
		TJPF_RGBX,
		TJPF_BGRX,
		TJPF_XBGR,
		TJPF_XRGB,
		TJPF_GRAY,
		TJPF_RGBA,
		TJPF_BGRA,
		TJPF_ABGR,
		TJPF_ARGB,
		TJPF_CMYK,
		TJPF_UNKNOWN = -1
	};

	enum TJSAMP
	{
		TJSAMP_444 = 0,
		TJSAMP_422,
		TJSAMP_420,
		TJSAMP_GRAY,
		TJSAMP_440,
		TJSAMP_411
	};

	public static unsafe byte[] EncodeImage(Texture2D tex)
	{
		return EncodeImage(tex.width, tex.height, tex.GetRawTextureData());
	}

	public static unsafe byte[] EncodeImage(int width, int height, byte[] img)
	{
		void* handle = tjInitCompress();

		// Input params
		int jpegQual = 35;
		int pitch = 0;
		int pixelFormat = (int)TJPF.TJPF_BGRA;
		int jpegSubsamp = (int)TJSAMP.TJSAMP_444;
		int flags = 0;

		// Outputs
		byte* jpegBuf = null;
		ulong jpegSize = 0;

		int tj_stat = tjCompress2(handle, img, width, pitch, height,
			pixelFormat, &(jpegBuf), &jpegSize, jpegSubsamp, jpegQual, flags);

		if(tj_stat != 0)
		{
			Debug.Log("Compress failed...");
		}

		byte[] result = new byte[jpegSize];
		Marshal.Copy((IntPtr)jpegBuf, result, 0, Convert.ToInt32(jpegSize));

		// File.WriteAllBytes("C:\\Users\\peter\\Projects\\UWPThreadingTest\\Build\\test.jpg", result);
		// string fullPath = Path.Combine(Application.persistentDataPath, "test.jpg");
		// Debug.Log("Writing to: " + fullPath);
		// File.WriteAllBytes(fullPath, result);

		tjFree(jpegBuf);
		tjDestroy(handle);

		return result;
	}
}
