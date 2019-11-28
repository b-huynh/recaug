using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAnimation : MonoBehaviour
{
    public UIAnimation uIAnimation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            uIAnimation.StartAnimation();
        }   
    }
}
