using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Recaug;

public class MenuElement : MonoBehaviour
{
    // The app that will hold ownership of this element. May influence the
    // appearance of the menu.
    private App parentApp;

    private bool isOpen = false;

    public ObjectRegistration dummyObject = null;

    public ObjectRegistration attachedObject = null;

    // Start is called before the first frame update
    void Start()
    {
        var closeButton = transform.Find("MenuPlane/CloseButton");
        closeButton.GetComponent<UIFocusable>().OnSelect += delegate {
            CloseMenu();
        };
        
        if (isOpen)
        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }

        if (dummyObject)
        {
            AttachObject(dummyObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (attachedObject)
        {
            var lr = GetComponent<LineRenderer>();
            // lr.enabled = true;

            // Determine menu bounds
            var collider = transform.Find("MenuPlane").GetComponent<Collider>();
            float width = collider.bounds.extents.x;
            float height = collider.bounds.extents.y;

            Vector3 lineStart = collider.gameObject.transform.position;
            Vector3 rightExtent = lineStart - new Vector3(width, 0, 0);
            Vector3 leftExtent = lineStart + new Vector3(width, 0, 0);
            
            Vector3 bottomExtent = lineStart - new Vector3(0, height, 0);

            // Determine left or right orientation
            // Vector3 lrdot = attachedObject.transform.InverseTransformPoint(transform.position);
            // if (lrdot.x < 0)
            // {
            //     // left side
            //     lineStart.x += width;
            // }
            // else
            // {
            //     // right side
            //     lineStart.x -= width;
            // }
            Vector3 rightInverse =
                attachedObject.transform.InverseTransformPoint(rightExtent);
            Vector3 leftInverse =
                attachedObject.transform.InverseTransformPoint(leftExtent);
            Vector3 bottomInverse =
                attachedObject.transform.InverseTransformPoint(bottomExtent);
            
            if (bottomInverse.y > 1.5)
            {
                lineStart = bottomExtent;
            }
            else if (rightInverse.x < 0)
            {
                // on left side of object
                lineStart = rightExtent;
            }
            else if (leftInverse.x > 0)
            {
                // on right side of object
                lineStart = leftExtent;
            }
            
            lr.SetPosition(0, lineStart);
            lr.SetPosition(1, attachedObject.position);
        }
        else
        {
            GetComponent<LineRenderer>().enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (isOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    void OnLink(App app)
    {
        parentApp = app;
    }

    void OnUnlink()
    {
        parentApp = null;
    }

    public void OpenMenu()
    {
        // Debug.Log("Opened Menu");
        GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.enabled = true);
        GetComponentsInChildren<Collider>().ToList().ForEach(r => r.enabled = true);

        // Don't render focus indicator unless selecting something. This is a bug.
        transform.Find("MenuPlane/FocusIndicator").GetComponent<Renderer>().enabled = false;
        GetComponent<LineRenderer>().enabled = true;

        var animation = GetComponentInChildren<ExpandAnimation>();
        animation.animationLength = 0.25f;
        animation.startScale = new Vector3(0.1f, 0.1f, 0.1f);
        // animation.desiredScale = new Vector3(1.0f, 1.0f, 1.0f);
        animation.StartAnimation();
        isOpen = true;
    }

    public void CloseMenu()
    {
        // Debug.Log("Closed menu");
        GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.enabled = false);
        GetComponentsInChildren<Collider>().ToList().ForEach(r => r.enabled = false);
        GetComponent<LineRenderer>().enabled = false;
        transform.Find("MenuPlane").localScale = new Vector3(0.1f, 0.1f, 0.1f);
        isOpen = false;
    }

    public void AttachObject(ObjectRegistration obj)
    {
        attachedObject = obj;
    }
}
