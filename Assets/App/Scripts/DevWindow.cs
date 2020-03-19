using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class DevWindow : MonoBehaviour
{
    private Canvas canvas;
    private UnityEngine.UI.Text text1;
    private UnityEngine.UI.Text text2;

    // State
    enum InspectorTab { CONFIG = 0, LOG = 1 }
    private InspectorTab currentTab = InspectorTab.LOG;
    private bool inDebugHUD = false;
    private bool isEnteringCommand = false;
    private string currentCommand = "";

    // Useful constants
    private int maxRows = 15;
    private int maxCols = 30;

    // Stats
    private Camera cam;
    private GameObject cursor;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        text1 = transform.Find("Column 1").GetComponent<UnityEngine.UI.Text>();
        text2 = transform.Find("Column 2").GetComponent<UnityEngine.UI.Text>();

        cam = Camera.main;
        cursor = GameObject.Find("/Cursor");
    }




    // Update is called once per frame
    void Update()
    {
        var icon = GetComponentInChildren<AppIcon>();
        if (icon)
        {
            icon.AppID = GameManager.Instance.currAppID;
        }

        if (Input.GetButtonDown("DevWindow"))
            Toggle("Debug Overlay", ref inDebugHUD);

        canvas.enabled = inDebugHUD;

        if (inDebugHUD)
        {
            DrawCol1();
            DrawCol2();

            if (!isEnteringCommand && Input.GetKeyDown(KeyCode.Return))
            {
                isEnteringCommand = true;
            } 
            else if (isEnteringCommand)
            {
                foreach(char c in Input.inputString)
                {
                    if (c == '\b')
                    { // backspace
                        // Debug.Log("RECEIVED BACKSPACE " + currentCommand);
                        if (currentCommand.Length != 0)
                        {
                            // Debug.Log("CURRENT LENGTH " + currentCommand.Length.ToString());
                            currentCommand = currentCommand.Substring(0, currentCommand.Length - 1);
                        }
                        // Debug.Log("CURRENT COMMAND: " + currentCommand);
                    }
                    else if ((c == '\n') || (c == '\r'))
                    { // enter or return
                        isEnteringCommand = false;
                        GameManager.Instance.DevCommand(currentCommand);
                        currentCommand = "";
                    }
                    else
                    {
                        currentCommand += c;
                    }
                }
            }
            else
            {
                // Switch inspector tabs
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    currentTab = InspectorTab.CONFIG;
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    currentTab = InspectorTab.LOG;
                if (Input.GetKeyDown(KeyCode.V))
                    SpatialMappingManager.Instance.DrawVisualMeshes = !SpatialMappingManager.Instance.DrawVisualMeshes;
            }
        }
    }


    private bool Toggle(string name, ref bool val)
    {
        val = !val;
        return val;
    }

    private void DrawCol1()
    {
        // text1.text = string.Format("\n   App: [{0}]",
        //     GameManager.Instance.GetCurrentApp().appName);
        string toDraw = "";

        // Draw Command Input
        toDraw += "[COMMAND]: " + currentCommand + "\n";
        toDraw += string.Format("[# discovered]: {0} / {1}\n",
            StatsTracker.Instance.DiscoveredObjects.ToString(),
            StatsTracker.TotalObjects);
        toDraw += string.Format("[# completed]: {0} / {1}\n",
            StatsTracker.Instance.CompletedActivities.ToString(),
            StatsTracker.TotalActivities);
        toDraw += string.Format("[mesh]: {0}\n",
            SpatialMappingManager.Instance.DrawVisualMeshes.ToString());

        toDraw += string.Format("[cam]: {0}\n", 
            cam.transform.position.ToString());
           
        toDraw += string.Format("[crsr]: {0}]\n", 
            cursor.transform.position.ToString());
        toDraw += string.Format("[app stack]: {0}\n",
            GameManager.Instance.stackHistory);

        text1.text = toDraw;
    }

    private void DrawCol2()
    {
        text2.text = "";
        List<string> tabNames = new List<string> {"config", "log"};
        string currTabName = tabNames[(int)currentTab];
        tabNames[(int)currentTab] = "*" + currTabName + "*";
        string tabIndicator = "| " + string.Join(" | ", tabNames) + " |\n";

        string toDraw = tabIndicator;
        // Draw inspector tabs
        switch(currentTab)
        {
            case InspectorTab.CONFIG:
                toDraw += GetDevConfigContent();
                break;
            case InspectorTab.LOG:
                toDraw += GetDevLogContent();
                break;
        }
        text2.text = toDraw;
    }

    private string GetDevConfigContent()
    {
        return JsonUtility.ToJson(Config.Params, true);
    }

    private string GetDevLogContent()
    {
        return "-" + string.Join("-", RemoteDebug.Instance.history);
    }
}
