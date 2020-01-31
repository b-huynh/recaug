using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIFocusable : MonoBehaviour, IFocusable
{
    public UIFocusableGroup group;

	public float selectDuration = 0.0f; // Focus-to-select duration, 0.0f to disable.
    private float selectTimer = 0.0f;

    public event Action<GameObject> OnSelect = delegate {};
    public List<GameObject> receivers = new List<GameObject>();
    
    // Start is called before the first frame update
    void Awake()
    {
        group = GetComponentInParent<UIFocusableGroup>();
    }

    void Start()
    {
        // GameManager.Instance.GetComponent<ClickHandler>().OnClick += OnClick;
        // GameManager.Instance.GetComponent<ClickHandler>().ClickHandlers.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (group.current == this)
        {
            // if (Input.GetKeyDown(KeyCode.Space))
            // if (Input.GetButtonDown("Submit"))
            // {
            //     RaiseSelect();
            // }

            if (GameManager.Instance.Clicked)
            {
                RaiseSelect();
            }

            if (selectDuration > 0.0f)
            {
                selectTimer += Time.deltaTime;
                if (selectTimer >= selectDuration)
                {
                    selectTimer = 0.0f;
                    RaiseSelect();
                }
            }
        }
    }

    void OnClick()
    {
        if (group.current == this)
        {
            RaiseSelect();
        }
    }

    void OnEnable()
    {
        selectTimer = 0.0f;
    }

    void OnDisable()
    {
        selectTimer = 0.0f;
    }

    void RaiseSelect()
    {
        OnSelect(gameObject);
        receivers.ForEach(r => r.BroadcastMessage("OnFocusSelect", gameObject));
    }

    public void OnFocusEnter()
	{
        group.current = this;
        
        group.focusIndicator.SetActive(true);
        group.focusIndicator.GetComponent<Renderer>().enabled = true;
        group.focusIndicator.transform.position = transform.position;
	}

	public void OnFocusExit()
	{
        group.current = null;
        selectTimer = 0.0f;
        group.focusIndicator.SetActive(false);
        group.focusIndicator.GetComponent<Renderer>().enabled = false;
	}
}


// public class UIFocusable : MonoBehaviour, IFocusable
// {
//     public GameObject focusIndicator; // An object placed over this one to indicate focus.
// 	public float selectDuration = 0.0f; // Focus-to-select duration, 0.0f to disable.
//     private float selectTimer = 0.0f;
//     public static UIFocusable current;
//     public event Action<GameObject> OnSelect = delegate {};
//     public List<GameObject> receivers = new List<GameObject>();
    
//     // Start is called before the first frame update
//     void Awake()
//     {
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.U))
//         {
//             Debug.LogFormat("Current {0}", current);
//         }

//         if (current == this)
//         {
//             if (Input.GetKeyDown(KeyCode.Space))
//             {
//                 RaiseSelect();
//             }

//             if (selectDuration > 0.0f)
//             {
//                 selectTimer += Time.deltaTime;
//                 if (selectTimer >= selectDuration)
//                 {
//                     selectTimer = 0.0f;
//                     RaiseSelect();
//                 }
//             }
//         }
//     }

//     void RaiseSelect()
//     {
//         Debug.Log("Raise Select");
//         OnSelect(gameObject);
//         receivers.ForEach(r => r.BroadcastMessage("OnFocusSelect", gameObject));
//     }

//     public void OnFocusEnter()
// 	{
//         Debug.Log("Focus Entered");
//         current = this;
//         focusIndicator.GetComponent<Renderer>().enabled = true;
//         focusIndicator.transform.position = transform.position;
// 	}

// 	public void OnFocusExit()
// 	{
//         Debug.Log("Focus Exited");
//         current = null;
//         selectTimer = 0.0f;
//         focusIndicator.GetComponent<Renderer>().enabled = false;
// 	}

//     // public void OnDisable()
//     // {
//     //     current = null;
//     //     selectTimer = 0.0f;
//     //     focusIndicator.GetComponent<Renderer>().enabled = false;
//     // }
// }
