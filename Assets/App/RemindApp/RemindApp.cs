using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;

public class RemindApp : App
{
    public GameObject menuPrefab;

    private Dictionary<string, (string, string)> dialogue =
        new Dictionary<string, (string, string)>()
    {
        {"apple", ("Remember to buy more apples", "Rie")},
        {"cup", ("Remember to wash the dishes", "Rie")},
        {"bowl", ("Remember to wash the dishes", "Rie")},
        {"bottle", ("Remember to wash the dishes", "Rie")},
        {"donut", ("Wanna grab some donuts after work?", "Tom")},
        {"sports ball", ("Wanna play tennis this weekend?", "Tom")},
        {"potted plant", ("Remind me to water the plants", "Myself")},
        {"smartphone", ("Remind me to call mom", "Myself")},
        {"teddy bear", ("Remind me to buy gift for Rie", "Myself")},
        {"wine glass", ("Remind me to buy wine for Rie", "Myself")},
        {"cake", ("Remind me to buy cake for Rie", "Myself")},
        {"scissors", ("Remind me to get a haircut", "Myself")},
    };

    private List<string> knownObjects = new List<string> {
        "keyboard",
        "bowl",
        "bottle",
        "cup",
        "teddy bear"
    };

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        ObjectRegistry.Instance.OnRegistered += OnObjectRegistered;

    }

    void Start()
    {
        if (Config.Loaded)
        {
            knownObjects = new List<string>(Config.Experiment.AppObjects3);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnObjectRegistered(ObjectRegistration registration)
    {
        // // Do not care about words we don't know.
        // if (!knownObjects.Contains(registration.className))
        // {
        //     return;
        // }

        if (!dialogue.ContainsKey(registration.className))
        {
            return;
        }

        // Register available content
        registration.gameObject.GetComponentInChildren<ContextMenu>().AddApp(appID);

        // Create virtual content
        var menu = GameObject.Instantiate(menuPrefab);
        Link(menu);

        menu.transform.position = Utils.RandomMenuPosition(registration.position);

        menu.GetComponent<MenuElement>().AttachObject(registration);
        menu.GetComponent<MenuElement>().OpenMenu();
        menu.transform.Find("Dialogue").GetComponent<TextMesh>().text = 
            "MEMO\n" + Utils.WrapText(dialogue[registration.className].Item1, 15);
        menu.transform.Find("Byline").GetComponent<TextMesh>().text =
            "- " + dialogue[registration.className].Item2;
    
        foreach(var button in menu.GetComponentsInChildren<UIFocusable>())
        {
            button.OnSelect += delegate(GameObject caller) {
                var menuElement =
                    caller.transform.parent.transform.parent.GetComponent<MenuElement>();
                string item = menuElement.attachedObject.className;
                string response = caller.GetComponent<TextMesh>().text;

                Debug.LogFormat("Item {0} Selected {1}", item, response);
                StatsTracker.Instance.LogActivity(
                    registration.className, appName, response);

                menuElement.CloseMenu();
                registration.gameObject.GetComponentInChildren<ContextMenu>().RemoveApp(appID);
            };
        }
    }

    public void OnFocusSelect(GameObject caller)
    {
        var menu = caller.transform.parent.transform.parent.GetComponent<MenuElement>();
        string item = menu.attachedObject.className;
        string response = caller.GetComponent<TextMesh>().text;
        Debug.LogFormat("Item {0} Selected {1}", 
            item, response);
        menu.CloseMenu();
    }

}
