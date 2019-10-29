   using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;
using Recaug.Client;

public class LearningTask : App
{
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
        var quiz = objectTag.GetComponent<Quiz>();
        int correctResponse = Random.Range(0, 4);
        var junk = new List<string> { "surfboard", "motorcycle", "airplane", "stop sign" };
        for(int i = 0; i < 4; ++i)
        {
            if (i == correctResponse)
            {
                quiz.SetOption(i, Translator.Translate(
                    registration.className, Translator.TargetLanguage.Japanese));
            }
            else
            {
                quiz.SetOption(i, Translator.Translate(
                    junk[i], Translator.TargetLanguage.Japanese));
            }
        }

        // Set response callback
        quiz.responseEvent.AddListener(r => {
            Debug.Log("Response Received: " + r.ToString());
            if (r == correctResponse)
            {
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
            }
        });

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
}
