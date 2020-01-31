using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using File = UnityEngine.Windows.File;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
#else
using System.Net.Http;
#endif

using Recaug.Client;

[System.Serializable]
public class SystemParams
{
    public string ServerIP = "192.168.2.251";
    public string ClientIP = "192.168.2.238";
    public string ObjectTrackingPort = "12000";
	public string EyeTrackingPort = "13000";
	public string DebugLogPort = "9999";
    public float ConfidenceThreshold = 0.0f;
    public float MaxObjectRadius = 0.4f;
    public int GeometryCapacity = 60;

    // public int FilterWindowSize = 120;
    // public int FilterWindowMinCount = 5;
    // public float FilterMinDist = 0.2f;
}

[System.Serializable]
public class UIParams
{
	// How long till we delete unconfirmed objects. 0.0f for never delete.
    public float ConfirmTimeout = 0.0f; // Seconds.
    // How long to focus on object label before select.
	public float FocusConfirmTime = 1.0f; // Seconds.
    public float PositionUpdateRate = 60.0f; // Seconds.
    public string DeterminePoseMethod = "average"; // "average" or "median".
    public int MedianAverageWindow = 5;
}



[System.Serializable]
public class ExperimentParams : ISerializationCallbackReceiver
{
    public bool SaveLogs = false;
    public Translator.TargetLanguage TargetLanguage;
    public MultitaskingType Multitasking;
    // public HashSet<string> KnownObjects = new HashSet<string>();
    // public HashSet<string> ValidObjects = new HashSet<string>();

	// INTERNAL IMPLEMENTATION. DO NOT USE.
	// Serializable types to convert to above member variables.
	public string _targetLanguage = "English";
    public string _multitasking = "InSitu";
    // public List<string> _knownObjects = new List<string>();
    // public List<string> _validObjects = new List<string>();
    
    public HashSet<string> AppObjects1 = new HashSet<string>();
    public List<string> _appObjects1 = new List<string>();

    public HashSet<string> AppObjects2 = new HashSet<string>();
    public List<string> _appObjects2 = new List<string>();

    public HashSet<string> AppObjects3 = new HashSet<string>();
    public List<string> _appObjects3 = new List<string>();

    public void OnBeforeSerialize() {
		_targetLanguage = TargetLanguage.ToString();
        _multitasking = Multitasking.ToString();
        // _knownObjects = new List<string>(KnownObjects);
        // _validObjects = new List<string>(ValidObjects);

        _appObjects1 = new List<string>(AppObjects1);
        _appObjects2 = new List<string>(AppObjects2);
        _appObjects3 = new List<string>(AppObjects3);
    }

    public void OnAfterDeserialize() {
		TargetLanguage = (Translator.TargetLanguage)System.Enum.Parse(
			typeof(Translator.TargetLanguage), _targetLanguage);
        Multitasking = (MultitaskingType)System.Enum.Parse(
            typeof(MultitaskingType), _multitasking);
        // KnownObjects = new HashSet<string>(_knownObjects);
        // ValidObjects = new HashSet<string>(_validObjects);

        AppObjects1 = new HashSet<string>(_appObjects1);
        AppObjects2 = new HashSet<string>(_appObjects2);
        AppObjects3 = new HashSet<string>(_appObjects3);
    }
}

[System.Serializable]
public class ConfigParams
{
	public SystemParams System = new SystemParams();
	public UIParams UI = new UIParams();
	public ExperimentParams Experiment = new ExperimentParams();
    public RecaugClientConfig Recaug = new RecaugClientConfig();
}

public static class Config {
    public static bool Loaded = false;
    public static ConfigParams Params = new ConfigParams();
    public static SystemParams System {
        get { return Params.System; }
        private set {}
    }
    public static UIParams UI {
        get { return Params.UI; }
        private set {}
    }
    public static ExperimentParams Experiment {
        get { return Params.Experiment; }
        private set {}
    }

	public static bool LoadHTTP(string url) {
        Debug.Log("Loading Config from: " + url);
        return LoadHTTPImpl(url);
	}


#if WINDOWS_UWP
    private static bool LoadHTTPImpl(string url)
    {
        // Set non-caching behavior
        HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
        filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
        filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
        HttpClient httpClient = new HttpClient(filter);

        Uri requestUri = new Uri(url);
        HttpResponseMessage httpResponse = new HttpResponseMessage();
        string httpResponseBody = "";
        try {
            //Send the GET request
            httpResponse = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
            httpResponse.EnsureSuccessStatusCode();
            httpResponseBody = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            LoadJson(httpResponseBody);
            return true;
        }
        catch (Exception ex) {
            httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            Debug.Log(httpResponseBody);
            return false;
        }
    }
#else
    private static bool LoadHTTPImpl(string url)
    {
        // Debug.Log("[Config] LoadHTTP not implemented.");
        HttpClient httpClient = new HttpClient();
        try
        {
            HttpResponseMessage response = 
                httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            string body = 
                response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            LoadJson(body);
            return true;
        }
        catch (Exception ex)
        {
            Debug.Log(
                "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message);
            return false;
        }
    }
#endif
  
    public static void LoadPersistantDataPathFile(string filename) {
		string filepath = Path.Combine(Application.persistentDataPath, filename);
        LoadFile(filepath);
    }

    public static void LoadFile(string filepath) {
        string json = Encoding.UTF8.GetString(File.ReadAllBytes(filepath));
        LoadJson(json);
    }

    public static void LoadJson(string json) {
        Params = JsonUtility.FromJson<ConfigParams>(json);
        Loaded = true;
    }

    public static string ToJson() {
       return JsonUtility.ToJson(Params, true); 
    }
}

// public HashSet<string> KnownObjects = new HashSet<string> {
// 	"backpack",
// 	"umbrella",
// 	"handbag",
// 	"tie",
// 	"frisbee",
// 	"sports ball",
// 	"baseball bat",
// 	"baseball glove",
// 	"skateboard",
// 	"tennis racket",
// 	"bottle",
// 	"wine glass",
// 	"cup",
// 	"fork",
// 	"knife",
// 	"spoon",
// 	"bowl",
// 	"mouse",
// 	"remote",
// 	"keyboard",
// 	"cell phone",
// 	"book",
// 	"clock",
// 	"vase",
// 	"scissors",
// 	"teddy bear",
// 	"hair drier",
// 	"toothbrush",
// 	"suitcase",
// 	"banana",
// 	"apple",
// 	"sandwich",
// 	"orange",
// 	"broccoli",
// 	"carrot",
// 	"hot dog",
// 	"pizza",
// 	"donut",
// 	"microwave",
// 	"airplane",
// 	"bus",
// 	"train",
// 	"truck",
// 	"boat",
// 	"bird",
// 	"cat",
// 	"dog",
// 	"horse",
// 	"sheep",
// 	"cow",
// 	"elephant",
// 	"bear",
// 	"zebra",
// 	"giraffe",
// 	"kite",
// 	"cake",
// 	"laptop",
// 	"toaster",
// 	"potted plant",
// 	"tv",
// 	"bicycle",
// 	"car",
// 	"motorcycle",
// 	"stop sign",
// 	"chair",
// 	"oven",
// 	"person",
// 	"traffic light",
// 	"fire hydrant",
// 	"parking meter",
// 	"bench",
// 	"skis",
// 	"snowboard",
// 	"surfboard",
// 	"couch",
// 	"bed",
// 	"dining table",
// 	"toilet",
// 	"sink",
// 	"refrigerator",
// };

// public HashSet<string> GoodObjects = new HashSet<string> {
// 	"backpack",
// 	"umbrella",
// 	"handbag",
// 	"sports ball",
// 	"tennis racket",
// 	"bottle",
// 	"wine glass",
// 	"cup",
// 	"fork",
// 	"knife",
// 	"spoon",
// 	"bowl",
// 	"mouse",
// 	"remote",
// 	"keyboard",
// 	"cell phone",
// 	"book",
// 	"scissors",
// 	"toothbrush",
// 	// "banana",
// 	// "apple",
// 	// "orange",
// 	// "broccoli",
// 	// "carrot",
// 	"laptop",
// 	"tv",
// };