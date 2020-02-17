using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Recaug;

public class WorkApp : App
{
    public GameObject menuPrefab;

    // private Dictionary<string, string> dialogue = new Dictionary<string, string>()
    // {
    //     {"mouse", "Does this {0}\nneed replacing"},
    //     {"remote", "Does this {0}\nneed replacing"},
    //     {"keyboard", "Does this {0}\nneed replacing"},
    //     {"cell phone", "Does this {0}\nneed replacing"},
    //     {"clock", "Does this {0}\nneed replacing"},
    //     {"vase", "Does this {0}\nneed replacing"},
    //     {"potted plant", "Does this {0}\nneed replacing"},
    //     {"cup", "Does this {0}\nneed replacing"},
    //     {"bowl", "Does this {0}\nneed replacing"},
    // };

    private Dictionary<string, (string, string)> dialogue =
        new Dictionary<string, (string, string)>()
    {
        {"tv", ("I want a bigger monitor", "Matt")},
        {"mouse", ("This {0}  is broken", "Jeff")},
        {"remote", ("{0} needs new batteries", "Jeff")},
        {"keyboard", ("This {0} is broken", "Jeff")},
        {"cell phone", ("This {0} needs repair", "Jeff")},
        {"clock", ("This {0} is wrong", "Jeff")},
        {"vase", ("This {0} is cracked", "Christine")},
        {"potted plant", ("The {0} needs water", "Christine")},
        {"cup", ("Not enough {0}s", "Christine")},
        {"bowl", ("Not enough {0}s", "Christine")},
        {"apple", ("I want better snacks", "Matt")},
        {"chair", ("I want a new {0}", "Matt")},
    };

    // private List<string> knownObjects = new List<string> {
    //     "keyboard",
    //     "bowl",
    //     "bottle",
    //     "cup",
    //     "teddy bear"
    // };

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        ObjectRegistry.Instance.OnRegistered += OnObjectRegistered;
    }

    void Start()
    {
        // if (Config.Loaded)
        // {
        //     knownObjects = new List<string>(Config.Experiment.AppObjects1);
        // }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnObjectRegistered(ObjectRegistration registration)
    {
        // Do not care about words we don't know.
        // if (!knownObjects.Contains(registration.className))
        if (!dialogue.ContainsKey(registration.className))
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
        menu.transform.position = RandomMenuPosition(registration.position);

        menu.GetComponent<MenuElement>().AttachObject(registration);
        menu.GetComponent<MenuElement>().OpenMenu();
        menu.transform.Find("Dialogue").GetComponent<TextMesh>().text = 
            GetDialogue(registration.className);
    
        foreach(var button in menu.GetComponentsInChildren<UIFocusable>())
        {
            button.OnSelect += OnFocusSelect;
        }
    }

    private Vector3 RandomMenuPosition(Vector3 objectPosition)
    {
        float delta = 0.25f;
        List<Vector3> deltas = new List<Vector3>() {
            new Vector3(delta, 0.0f, 0.0f),
            new Vector3(delta, delta, 0.0f),
            new Vector3(0.0f, delta, 0.0f),
            new Vector3(-delta, delta, 0.0f),
            new Vector3(-delta, 0.0f, 0.0f)
        };
        var menuPositions = deltas.Select(x => objectPosition + x).ToList();
        int rand = Random.Range(0, 5);
        return menuPositions[rand]; 
    }

    public void OnFocusSelect(GameObject caller)
    {
        var menu = caller.transform.parent.transform.parent.GetComponent<MenuElement>();
        string item = menu.attachedObject.className;
        Debug.LogFormat("Item {0} Selected {1}", 
            item, caller.GetComponent<TextMesh>().text);
        menu.CloseMenu();
    }

    public string GetDialogue(string type)
    {
        string retval;
        if (dialogue.ContainsKey(type))
        {
            try
            {
                retval = string.Format(dialogue[type].Item1, type);
            }
            catch
            {
                retval = dialogue[type].Item1;
            }
        }
        else
        {
            retval = string.Format("Pack {0}", type);
        }
        return Utils.WrapText(retval, 15);
    }
}
