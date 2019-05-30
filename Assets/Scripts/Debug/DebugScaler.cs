using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScaler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            GetComponent<CanvasScaler>().scaleFactor += 0.02f;
            Debug.Log(GetComponent<CanvasScaler>().scaleFactor);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            GetComponent<CanvasScaler>().scaleFactor -= 0.02f;
            Debug.Log(GetComponent<CanvasScaler>().scaleFactor);
        }
    }
}
