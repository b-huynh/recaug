using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;

public class RemindApp : App
{
    public GameObject menuPrefab;

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
        // Do not care about words we don't know.
        if (!knownObjects.Contains(registration.className))
        {
            return;
        }

        // Register available content
        registration.gameObject.GetComponentInChildren<ContextMenu>().AddApp(appID);

        // Create virtual content
        var menu = GameObject.Instantiate(menuPrefab);
        Link(menu);

        Vector3 menuPos;
        menuPos = Random.Range(-1.0f, 1.0f) > 0 ? 
            registration.position + new Vector3(0.35f, 0.0f, 0.0f) : 
            registration.position + new Vector3(-0.35f, 0.0f, 0.0f);
        menu.transform.position = menuPos;

        menu.GetComponent<MenuElement>().AttachObject(registration);
        menu.GetComponent<MenuElement>().OpenMenu();
        menu.transform.Find("Dialogue").GetComponent<TextMesh>().text = 
            "Reminder:\n Buy " + registration.className;
    
        foreach(var button in menu.GetComponentsInChildren<UIFocusable>())
        {
            button.OnSelect += OnFocusSelect;
        }
    }

    public void OnFocusSelect(GameObject caller)
    {
        var menu = caller.transform.parent.transform.parent.GetComponent<MenuElement>();
        string item = menu.attachedObject.className;
        Debug.LogFormat("Item {0} Selected {1}", 
            item, caller.GetComponent<TextMesh>().text);
        menu.CloseMenu();
    }

}
