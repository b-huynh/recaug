using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPosition3D : MonoBehaviour
{
    public float delta = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            transform.position += new Vector3(0, delta, 0);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            transform.position -= new Vector3(0, delta, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            transform.position += new Vector3(delta, 0, 0);
        }           
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            transform.position -= new Vector3(delta, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.LogFormat("[{0}] At position: {1}", 
                gameObject.name, transform.position.ToString("F3"));
        }
    }
}