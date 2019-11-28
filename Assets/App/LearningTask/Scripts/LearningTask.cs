   using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;
using Recaug.Client;

public class LearningTask : App
{
    // Object Discovery Prefab
    public GameObject discoveryPrefab;

    // Learning Activity Prefab
    public GameObject objectTagPrefab;

    // Learning State
    private Dictionary<string, GameObject> toLearn;
    private List<string> learned;

    // UI State
    private bool awaitingResponse = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        toLearn = new Dictionary<string, GameObject>();
        learned = new List<string>();

        // Context Events
        RecaugClient.Instance.objectNearbyEvent.AddListener(OnObjectNearby);
        ObjectRegistry.Instance.objectRegisteredEvent.AddListener(OnObjectRegistered);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            var registration =
                new ObjectRegistration("test", 1.0f, new Vector3(0, 1, -7));
            OnObjectRegistered(registration);
        }
    }

    void OnObjectRegistered(ObjectRegistration registration)
    {
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
        Link(objectTag);

        // Create the Quiz
        var quiz = objectTag.GetComponent<MultipleChoiceQuiz>();
        int correctResponse = Random.Range(0, 4);
        var junk = new List<string> { "surfboard", "motorcycle", "airplane", "zebra" };
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

            // Release App Focus
            GameManager.Instance.SwitchApp(1);

            // Continue rendering learnables in case of manual switch
            foreach (var kv in toLearn)
            {
                kv.Value.GetComponent<Renderer>().enabled = true;
            }
        };

        objectTag.SetActive(false);

        // Update known objects cache
        toLearn[registration.className] = objectTag;
    }

    void OnObjectRemoved(ObjectRegistration registration)
    {
        // TODO: Implement.
    }

    void OnObjectNearby(ContextEventData eventData)
    {
        if (!awaitingResponse && toLearn.ContainsKey(eventData.objectType))
        {
            if (!InView(toLearn[eventData.objectType]))
            {
                return;
            }

            // Grab App Focus
            GameManager.Instance.SwitchApp(2);

            // Render event object
            // Don't render non-event objects
            foreach (var kv in toLearn)
            {
                kv.Value.GetComponent<Renderer>().enabled = 
                    kv.Key == eventData.objectType ? true : false;
            }

            // Set response state
            awaitingResponse = true;
        }
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
