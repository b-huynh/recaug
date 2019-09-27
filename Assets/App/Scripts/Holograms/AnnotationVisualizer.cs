using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Recaug;

public class AnnotationVisualizer : MonoBehaviour
{
    public ObjectRegistration registration;
    public string text = "";
	public List<Annotation> annotations;

    // Where to place the label once confirmed
    private static Annotation.Orientation ConfirmedAnchor = 
        Annotation.Orientation.RIGHT;

    void Start()
    {
        // registration.addLabelEvent.AddListener(OnAddLabel);
        // registration.removeLabelEvent.AddListener(OnRemoveLabel);
        // registration.confirmLabelEvent.AddListener(OnConfirmLabel);

        // foreach(Annotation ann in annotations)
        // {
        //     // ann.registration = this.registration;
        //     ann.renderEnabled = false;
        // }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var ann in annotations)
        {
            ann.text = text;
        }
    }

    void OnAddLabel(string label)
    {
        foreach(Annotation ann in annotations)
        {
            if (ann.renderEnabled && ann.text == label)
            {
                // Already have this label
                return;
            }
        }

        foreach(Annotation ann in annotations)
        {
            if (!ann.renderEnabled)
            {
                // Add new label
                ann.text = label;
                ann.renderEnabled = true;
                break;
            }
        }
    }

    void OnRemoveLabel(string label)
    {
        foreach(Annotation ann in annotations)
        {
            if (ann.renderEnabled && ann.text == label)
            {
                ann.text = "";
                ann.renderEnabled = false;
            }
        }
    }

    void OnConfirmLabel(string label)
    {
        foreach(Annotation ann in annotations) 
        {
            if (ann.orientation == ConfirmedAnchor) 
            {
                // ann.text = Translator.Translate(registration.className,
                //     Config.Experiment.TargetLanguage);
                ann.renderEnabled = true;
            }
            else
            {
                ann.renderEnabled = false;
            }
        }
    }
}
