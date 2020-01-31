using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPosition3D : MonoBehaviour
{
    public float delta = 5.0f;

    public bool wasd = false;
    public bool arrows = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (arrows)
        {
            if (Input.GetKey(KeyCode.UpArrow)) {
                UpdateUp();
            }
            if (Input.GetKey(KeyCode.DownArrow)) {
                UpdateDown();
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                UpdateLeft();
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                UpdateRight();
            }
        }

        if (wasd)
        {
            if (Input.GetKey(KeyCode.W)) {
                UpdateUp();
            }
            if (Input.GetKey(KeyCode.S)) {
                UpdateDown();
            }
            if (Input.GetKey(KeyCode.A)) {
                UpdateLeft();
            }
            if (Input.GetKey(KeyCode.D)) {
                UpdateRight();
            }         
        }
    }

    void UpdateUp()
    {
        transform.position += new Vector3(0, delta, 0);
    }
    void UpdateDown()
    {
        transform.position -= new Vector3(0, delta, 0);
    }
    void UpdateLeft()
    {
        transform.position -= new Vector3(delta, 0, 0);
    }
    void UpdateRight()
    {
        transform.position += new Vector3(delta, 0, 0);
    }
}