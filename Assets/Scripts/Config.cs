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

[System.Serializable]
public class UIParams {
	public float ConfirmTimeout = 20.0f; // Seconds. No confirmation timeout.
	public float FocusConfirmTime = 1.5f; // Seconds. Focus time to select.
	public Translator.TargetLanguage TargetLanguage = 
		Translator.TargetLanguage.Japanese;
}

public static class Config {
	public static string ORSendPort = "11000";
	public static string ORListenPort = "12000";
	public static string EyeTrackingPort = "13070";
	public static string DebugLogPort = "9999";
	public static string CurrentFileName = "default_config.json";
	public static string CurrentFilePath = null;
	public static ConfigParameters Params { get; private set; } = new ConfigParameters();
	public static UIParams UIParams { get; private set; } = new UIParams();
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

	public static HashSet<string> KnownObjects = new HashSet<string> {
		"backpack",
		"umbrella",
		"handbag",
		"tie",
		"frisbee",
		"sports ball",
		"baseball bat",
		"baseball glove",
		"skateboard",
		"tennis racket",
		"bottle",
		"wine glass",
		"cup",
		"fork",
		"knife",
		"spoon",
		"bowl",
		"mouse",
		"remote",
		"keyboard",
		"cell phone",
		"book",
		"clock",
		"vase",
		"scissors",
		"teddy bear",
		"hair drier",
		"toothbrush",
		"suitcase",
		"banana",
		"apple",
		"sandwich",
		"orange",
		"broccoli",
		"carrot",
		"hot dog",
		"pizza",
		"donut",
		"microwave",
		"airplane",
		"bus",
		"train",
		"truck",
		"boat",
		"bird",
		"cat",
		"dog",
		"horse",
		"sheep",
		"cow",
		"elephant",
		"bear",
		"zebra",
		"giraffe",
		"kite",
		"cake",
		"laptop",
		"toaster",
		"potted plant",
		"tv",
		"bicycle",
		"car",
		"motorcycle",
		"stop sign",
		"chair",
		"oven",
		"person",
		"traffic light",
		"fire hydrant",
		"parking meter",
		"bench",
		"skis",
		"snowboard",
		"surfboard",
		"couch",
		"bed",
		"dining table",
		"toilet",
		"sink",
		"refrigerator",
	};

	public static HashSet<string> GoodObjects = new HashSet<string> {
		"backpack",
		"umbrella",
		"handbag",
		"sports ball",
		"tennis racket",
		"bottle",
		"wine glass",
		"cup",
		"fork",
		"knife",
		"spoon",
		"bowl",
		"mouse",
		"remote",
		"keyboard",
		"cell phone",
		"book",
		"scissors",
		"toothbrush",
		// "banana",
		// "apple",
		// "orange",
		// "broccoli",
		// "carrot",
		"laptop",
		"tv",
	};
}
