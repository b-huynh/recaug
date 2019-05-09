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
	public string ServerIP = "192.168.2.134";
}

public static class Config {
	public static string ORSendPort = "11000";
	public static string ORListenPort = "12000";
	public static string EyeTrackingPort = "13070";
	public static string DebugLogPort = "9999";
	public static string CurrentFileName = "default_config.json";
	public static string CurrentFilePath = null;
	public static ConfigParameters Params { get; private set; } = new ConfigParameters();

	public static void LoadConfig(string filename) {
		CurrentFileName = filename;
		CurrentFilePath = Path.Combine(Application.persistentDataPath, CurrentFileName); 

		// Check if file exists, create if it doesn't
		if (File.Exists(CurrentFilePath)) {
			string configJson = Encoding.UTF8.GetString(File.ReadAllBytes(CurrentFilePath));
			Params = JsonUtility.FromJson<ConfigParameters>(configJson);
			Debug.Log("Loaded config file.");
		} else {
			Debug.Log("No config file found, creating with default params.");
			WriteConfig();
		}
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
		File.WriteAllBytes(CurrentFilePath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(Params)));
	}
}
