using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.Events;
using File = UnityEngine.Windows.File;

#if !UNITY_EDITOR
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
#endif


[System.Serializable]
public class ConfigParameters {
	public string ServerIP = "192.168.100.108";
}

public static class Config {
	public static string ORSendPort = "11000";
	public static string ORListenPort = "12000";
	public static string EyeTrackingPort = "13000";
	public static string DebugLogPort = "9999";
	public static string FileName = "default";
	public static ConfigParameters Params { get; private set; } = new ConfigParameters();

	public static void LoadConfig(string filename) {
		// Check if file exists
		string path = Path.Combine(Application.persistentDataPath, filename);
		if (File.Exists(path)) {	
			string configJson = Encoding.UTF8.GetString(File.ReadAllBytes(path));
			Params = JsonUtility.FromJson<ConfigParameters>(configJson);
		} else {
			File.WriteAllBytes(path, Encoding.UTF8.GetBytes(JsonUtility.ToJson(Params)));
		}
		FileName = filename;
	}

	public static void UpdateConfig(string key, string value) {
		Debug.LogFormat("CONFIG RECEIVED: {0} : {1}", key, value);
		switch(key) {
			case "ServerIP":
				Params.ServerIP = value;
				break;
			default:
				Debug.LogFormat("[Config] WARNING: Param '{0}' does not exist", key);
				break;
		}
		WriteConfig();
	}

	private static void WriteConfig() {
		string path = Path.Combine(Application.persistentDataPath, Config.FileName);
		File.WriteAllBytes(path, Encoding.UTF8.GetBytes(JsonUtility.ToJson(Params)));
	}
}
