using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClickHandler : MonoBehaviour
{
    static public float LongPressWindow = 0.35f;

    // public KeyCode key = KeyCode.Space;
    // public event Action OnClick = delegate {};
    // public event Action OnLongPress = delegate {};

    public List<GameObject> ClickHandlers = new List<GameObject>();
    public List<GameObject> LongPressHandlers = new List<GameObject>();

    // Used for determining click state
    private float holdTime = 0.0f;
    private bool longPressTriggered = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.J))
        // {
        //     Debug.Log("Attempting to print joysticks:");
        //     foreach(var ss in Input.GetJoystickNames())
        //     {
        //         Debug.LogFormat("Joystick Name: {0}", ss);
        //     }
        // }

        // if (Input.GetButton("joystick button 0"))
        // {
        //     Debug.Log("Joystick Button 0 Pressed");
        // }

        // if (Input.GetKeyDown(key))
        if (Input.GetButtonDown("Submit") ||
            Input.GetMouseButtonDown(0))
        {
            holdTime = 0.0f;
        }

        // if (Input.GetKey(key))
        if (Input.GetButton("Submit") ||
            Input.GetMouseButton(0))
        {
            holdTime += Time.deltaTime;
        }

        // if (Input.GetKeyUp(key))
        if (Input.GetButtonUp("Submit") ||
            Input.GetMouseButtonUp(0))
        {
            if (holdTime < LongPressWindow)
            {
                // OnClick();
                foreach(var h in ClickHandlers)
                {
                    Debug.LogFormat("Handler: {0}", Utils.GetFullName(h));
                }

                ClickHandlers.ForEach(r => r.BroadcastMessage(
                    "OnClick", SendMessageOptions.DontRequireReceiver));
            }

            // Reset click state
            holdTime = 0.0f;
            longPressTriggered = false;
        }

        if (!longPressTriggered)
        {
            if (holdTime > LongPressWindow)
            {
                // OnLongPress();
                LongPressHandlers.ForEach(r => r.BroadcastMessage(
                    "OnLongPress", SendMessageOptions.DontRequireReceiver));
                longPressTriggered = true;
            }
        }
    }    
}
