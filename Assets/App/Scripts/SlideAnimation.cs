using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SlideAnimation : UIAnimation
{
    // Public Animation State
    public float animateSpeed = 1.0f;
    public Vector3 animateStart;
    public Vector3 animateEnd;
    private float animateDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected override void UpdateAnimation()
    {
        animateDistance = Vector3.Distance(animateStart, animateEnd);
        float distCovered = (Time.time - animateStartTime) * animateSpeed;
        float distFraction = distCovered / animateDistance;
        transform.position = Vector3.Lerp(animateStart, animateEnd, distFraction);
        if (distFraction >= 1.0f)
        {
            transform.position = animateEnd;
            StopAnimation();
        }
    }
}
