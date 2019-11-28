using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FadeInAnimation : UIAnimation
{
    public float animationLength = 1.0f; // Seconds

    private float startAlpha = 0.0f;
    private float desiredAlpha = 1.0f;
    private float alpha
    {
        get
        {
            return GetComponent<Renderer>().material.color.a;
        }
        set
        {
            Color curr = GetComponent<Renderer>().material.color;
            curr.a = value;
            GetComponent<Renderer>().material.color = curr;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        alpha = startAlpha;
    }

    protected override void UpdateAnimation()
    {
        float percentElapsed = Time.deltaTime / animationLength;
        float moved = (desiredAlpha - startAlpha) * percentElapsed;
        alpha += moved;
        if (alpha >= desiredAlpha)
        {
            alpha = desiredAlpha;
            StopAnimation();
        }
    }   
}
