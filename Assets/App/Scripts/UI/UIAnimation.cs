using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIAnimation : MonoBehaviour
{
    public event Action OnAnimationStart = delegate {};
    public event Action OnAnimationStop = delegate {};

    bool isAnimating = false;

    protected float animateStartTime;
    protected float animateEndTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (isAnimating)
        {
            UpdateAnimation();
        }
    }

    protected virtual void UpdateAnimation() {}

    public void StartAnimation()
    {
        OnAnimationStart();
        isAnimating = true;
        animateStartTime = Time.time;
    }

    protected void StopAnimation()
    {
        isAnimating = false;
        OnAnimationStop();
        animateEndTime = Time.time;
    }
}
