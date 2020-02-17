   using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;
using Recaug.Client;

public class LearnApp : App
{
    // Object Discovery Prefab
    public GameObject discoveryPrefab;

    // Learning Activity Prefab
    public GameObject objectTagPrefab;

    // Learning State
    public List<string> knownWords = new List<string> {
        "keyboard",
        "tv",
        "mouse",
        "bottle",
        "cup",
        "wine glass",
        "chair"
    };
    private Dictionary<string, GameObject> toLearn;
    private List<string> learned;

    // UI State
    private bool awaitingResponse = false;

    private bool autoTransition = false;
    private static bool releaseFocusAfterQuiz = false;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        
        toLearn = new Dictionary<string, GameObject>();
        learned = new List<string>();

        // Context Events
        RecaugClient.Instance.OnNearIn += OnNearIn;
        ObjectRegistry.Instance.OnRegistered += OnObjectRegistered;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            autoTransition = !autoTransition;
        }
    }

    void OnObjectRegistered(ObjectRegistration registration)
    {
        // Do not care about words we don't know.
        if (!knownWords.Contains(registration.className))
        {
            return;
        }

        // Register available content
        registration.gameObject.GetComponentInChildren<ContextMenu>().AddApp(appID);

        // Debug.LogFormat("Object Registered: {0}, min: {1}, max: {2}, cen: {3}",
        //     registration.className,
        //     registration.geometry.extentsMin,
        //     registration.geometry.extentsMax,
        //     registration.geometry.extentsCenter);

        // var geometry = registration.geometry;
        // float endScaleX = geometry.extentsMax.x - geometry.extentsMin.x;
        // float endScaleY = geometry.extentsMax.y - geometry.extentsMin.y;
        // float endScaleZ = geometry.extentsMax.z - geometry.extentsMin.z;
        // Debug.LogFormat("Desired Scale: {0}, {1}, {2}", endScaleX, endScaleY, endScaleZ);

        CreateQuizContent(registration);

        // var discovery = GameObject.Instantiate(discoveryPrefab,
        //     registration.position, transform.rotation);

        // var animation = discovery.GetComponent<ObjectDiscoveryAnimation>();
        // animation.OnAnimationStop += delegate { CreateQuizContent(registration); };
        // animation.startScale = new Vector3(0.04f, 0.04f, 0.04f);
    
        // var geometry = registration.geometry;
        // float endScaleX = geometry.extentsMax.x - geometry.extentsMin.x;
        // float endScaleY = geometry.extentsMax.y - geometry.extentsMin.y;
        // float endScaleZ = geometry.extentsMax.z - geometry.extentsMin.z;
        // animation.desiredScale = new Vector3(endScaleX, endScaleY, endScaleZ);

        // animation.StartAnimation();
    }

    void OnAppSwitch(AppStackFrame callFrame)
    {
        Debug.LogFormat("OnAppSwitch called with {0}", callFrame.callerObject);
        if (callFrame.callerObject == null)
        {
            // Manual app switch, display all.
            releaseFocusAfterQuiz = false;
        }
        else
        {
            // Object specific app switch, display object content only.
            SetActiveState(false);
            toLearn[callFrame.callerObject.className].SetActive(true);
            releaseFocusAfterQuiz = true;
        }
    }

    void CreateQuizContent(ObjectRegistration registration)
    {
        // Exit if already learned
        if (learned.Contains(registration.className))
        {
            return;
        }

        // Create object tag
        var objectTag = GameObject.Instantiate(objectTagPrefab,
            registration.position, transform.rotation);
        // objectTag.GetComponent<AnnotationVisualizer>().text = registration.className;


        // Create the Quiz
        var quiz = objectTag.GetComponent<MultipleChoiceQuiz>();
        int correctResponse = Random.Range(0, 4);
        var junk = new List<string> { "giraffe", "motorcycle", "airplane", "zebra" };
        for(int i = 0; i < 4; ++i)
        {
            if (i == correctResponse)
            {
                quiz.SetOption(i, Translator.Translate(
                    registration.className, Config.Experiment.TargetLanguage));
            }
            else
            {
                quiz.SetOption(i, Translator.Translate(
                    junk[i], Config.Experiment.TargetLanguage));
            }
        }
        quiz.SetAnswer(correctResponse);

        // Set response callback
        quiz.OnCorrectResponse += delegate {
            Debug.Log("Triggering correct response callback");
            // Remove Learning Event
            toLearn.Remove(registration.className);

            // Update learned words
            learned.Add(registration.className);

            // Unlink object
            // TODO: Think of smarter way to do this...
            Unlink(objectTag);

            // Destroy Quiz GameObject
            GameObject.Destroy(objectTag);

            // Reset response state
            awaitingResponse = false;

            // Remove self from object's available content list.
            registration.gameObject.GetComponentInChildren<ContextMenu>().RemoveApp(appID);

            // Release App Focus
            // GameManager.Instance.SwitchApp(1);
            if (releaseFocusAfterQuiz)
            {
                GameManager.Instance.ReleaseAppFocus();
            }

            // // Continue rendering learnables in case of manual switch
            // foreach (var kv in toLearn)
            // {
            //     kv.Value.GetComponent<Renderer>().enabled = true;
            // }
        };

        Link(objectTag);
        // objectTag.SetActive(false);

        // Update known objects cache
        toLearn[registration.className] = objectTag;
    }

    void OnObjectRemoved(ObjectRegistration registration)
    {
        // TODO: Implement.
    }

    void OnNearIn(ObjectRegistration registration)
    {
        // if (!autoTransition)
        // {
        //     return;
        // }

        // string className = registration.className;

        // if (!awaitingResponse && toLearn.ContainsKey(className))
        // {
        //     if (!InView(toLearn[className]))
        //     {
        //         return;
        //     }

        //     // Grab App Focus
        //     GameManager.Instance.SwitchApp(appID);

        //     // Render event object
        //     // Don't render non-event objects
        //     foreach (var kv in toLearn)
        //     {
        //         kv.Value.GetComponent<Renderer>().enabled = 
        //             kv.Key == className ? true : false;
        //     }

        //     // Set response state
        //     awaitingResponse = true;
        // }
    }

    private bool InView(GameObject g)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(g.transform.position);
        if (vp.x >= 0.0f && vp.x <= 1.0f && 
            vp.y >= 0.0f && vp.y <= 1.0f && 
            vp.z > 0.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
