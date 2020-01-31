using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;

public class WarehouseApp : App
{
    public UnityEngine.UI.Text display;
    public GameObject taskCompletedIndicator;

    public GameObject warehouseIndicator;

    private float timer = 0.0f;

    private List<string> toMove = new List<string> {
        "bowl",
        "smartphone",
        "mouse",
        "bottle",
        "cup"
    };

    private bool waitingForResponse = false;
    private Queue<ObjectRegistration> pending = new Queue<ObjectRegistration>(); 
    private List<string> completed = new List<string>();

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        Link(display.gameObject);
        Link(taskCompletedIndicator);
        taskCompletedIndicator.transform.position = Camera.main.transform.position;
        taskCompletedIndicator.GetComponentInChildren<UIFocusable>().OnSelect += delegate {
            var registration = pending.Dequeue();
            completed.Add(registration.className);
            taskCompletedIndicator.SetActive(false);
            waitingForResponse = false;
            // Remove self from object's available content list
            registration.gameObject.GetComponentInChildren<ContextMenu>().RemoveApp(appID);
        };
        taskCompletedIndicator.SetActive(false);

        ObjectRegistry.Instance.OnRegistered += OnObjectRegistered;
    }

    // Update is called once per frame
    void Update()
    {
        if (!waitingForResponse && pending.Count > 0)
        {
            var registration = pending.Peek();

            // Set up visual indicator
            var indicator = GameObject.Instantiate(warehouseIndicator);
            Link(indicator);
            indicator.transform.position = registration.position;
            indicator.GetComponentInChildren<TextMesh>().text += registration.className;

            // Set up interaction elements
            taskCompletedIndicator.SetActive(true);
            waitingForResponse = true;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            taskCompletedIndicator.transform.position = Camera.main.transform.position;
        }

        timer += Time.deltaTime;
        // string prompt = string.Format("\n   [EVEREST] Employee View\n   Work timer: {0:0.00}\n   Fetch:\n   -> #A-125 (Pillow)", timer);
        // display.text = timer.ToString();
        // string prompt = string.Format("\n   [{0}] Employee View", appName);
        string prompt = "";

        display.text = prompt;
    }

    void OnObjectRegistered(ObjectRegistration registration)
    {
        if (toMove.Contains(registration.className))
        {
            // Add self from object's available content list
            registration.gameObject.GetComponentInChildren<ContextMenu>().AddApp(appID);
            pending.Enqueue(registration);
            toMove.Remove(registration.className);
        }
    }

}
