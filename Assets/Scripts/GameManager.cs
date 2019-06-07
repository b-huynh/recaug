using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;

enum InspectorTab { CONFIG, LOG }

public class GameManager : Singleton<GameManager> {
    public string sessionID = "";
    public bool isInOverlay = false;
    public GameObject debugOverlay = null;
    private InspectorTab currentTab = InspectorTab.CONFIG;

    public bool debug = false;
    private bool isEnteringCommand = false;
    private string currentCommand = "";


    private GameObject evalOriginPoint = null;

    private int firstFrameLatency = 0; // Should be milliseconds

    private string configURL = "http://192.168.2.134:8080/config.json";

    protected override void Awake() {
        base.Awake();
        Config.LoadHTTP(configURL);
        sessionID = System.Guid.NewGuid().ToString();
    }

    public void Start() {
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Q))
            Toggle("Debug Overlay", ref isInOverlay);

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Perform Soft Reset:
            // Load new config
            // Delete all current annotations
            // Generate new session ID
            HologramManager.Instance.SaveLog();
            HologramManager.Instance.ClearLog();
            ObjectMemory.Instance.ClearMemory();
            Config.LoadHTTP(configURL);
            sessionID = System.Guid.NewGuid().ToString();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            HologramManager.Instance.SaveLog();
        }

        debugOverlay.SetActive(isInOverlay);
        if (isInOverlay) {
            DrawTextOverlay(debugOverlay);

            if (!isEnteringCommand && Input.GetKeyDown(KeyCode.Return)) {
                isEnteringCommand = true;
            } else if (isEnteringCommand) {
                foreach(char c in Input.inputString) {
                    if (c == '\b') { // backspace
                        Debug.Log("RECEIVED BACKSPACE " + currentCommand);
                        if (currentCommand.Length != 0) {
                            Debug.Log("CURRENT LENGTH " + currentCommand.Length.ToString());
                            currentCommand = currentCommand.Substring(0, currentCommand.Length - 1);
                        }
                        Debug.Log("CURRENT COMMAND: " + currentCommand);
                    } else if ((c == '\n') || (c == '\r')) { // enter or return
                        isEnteringCommand = false;
                        ExecuteCommand(currentCommand);
                        currentCommand = "";
                    } else {
                        currentCommand += c;
                    }
                }
            } else {
                // Switch inspector tabs
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    currentTab = InspectorTab.CONFIG;
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    currentTab = InspectorTab.LOG;

                // Toggle features
                if (Input.GetKeyDown(KeyCode.P))
                    Toggle("Streaming", ref WebcamStreaming.Instance.streaming);
                if (Input.GetKeyDown(KeyCode.D))
                    Toggle("Debug Mode", ref debug);
                // if (Input.GetKeyDown(KeyCode.R))
                //     Toggle("Raw Points Mode", ref HologramManager.Instance.rawPointsMode);
                if (Input.GetKeyDown(KeyCode.S))
                    HologramManager.Instance.SaveLog();
                if (Input.GetKeyDown(KeyCode.V))
                    SpatialMappingManager.Instance.DrawVisualMeshes = !SpatialMappingManager.Instance.DrawVisualMeshes;
            }
        }
    }

    private void ExecuteCommand(string command) {
        string[] tokens = command.Split();
        switch(tokens[0]) {
            case "setip":
                // Config.UpdateConfig("ServerIP", tokens[1]);
                break;
        }
    }

    private bool Toggle(string name, ref bool val) {
        val = !val;
        Debug.LogFormat("[GameManager] {0}: {1}", name, val);
        return val;
    }

    private void DrawTextOverlay(GameObject overlay) {
        string toDraw = "";
        int lineLength = 60;

        // Draw Command Input
        toDraw += "[COMMAND]: " + currentCommand + "\n";
        // toDraw += "\n" + new string('_', lineLength) + "\n";

        // Draw State Variables
        // toDraw += "[STREAMING] " + WebcamStreaming.Instance.streaming.ToString() + "\n";
        // toDraw += "[RAW POINTS] " + HologramManager.Instance.rawPointsMode.ToString() + "\n";
        // toDraw += "[LOGGING POINTS] " + HologramManager.Instance.isLogging.ToString() + "\n";
        // toDraw += "[DEBUG] " + debug.ToString() + "\n";
        
        // Latency
        if (WebcamStreaming.Instance.firstSent && WebcamStreaming.Instance.firstReceived)
        {
            firstFrameLatency = (WebcamStreaming.Instance.firstReceivedTime - WebcamStreaming.Instance.firstSentTime).Milliseconds;
        }
        toDraw += "[FRAME LATENCY] " + firstFrameLatency.ToString() + "ms \n";

        // Draw inspector tabs
        switch(currentTab) {
            case InspectorTab.CONFIG:
                toDraw += "|*config| log |\n>>\n";
                toDraw += JsonUtility.ToJson(Config.Params, true);
                break;
            case InspectorTab.LOG:
                toDraw += "| config |*log|\n>>\n";
                toDraw += "LOG NOT YET IMPLEMENTED";
                break;
        }
        toDraw += "\n" + new string('_', lineLength) + "\n";

        debugOverlay.GetComponentInChildren<UnityEngine.UI.Text>().text = toDraw;
    }
}
