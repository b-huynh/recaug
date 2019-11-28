using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ObjectDiscoveryAnimation : UIAnimation
{
    public float animationLength = 1.0f;

    public Vector3 startScale;
    public Vector3 desiredScale;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void UpdateAnimation()
    {
        float percentElapsed = Time.deltaTime / animationLength;
        float moveX = (desiredScale.x - startScale.x) * percentElapsed;
        float moveY = (desiredScale.y - startScale.y) * percentElapsed;
        float moveZ = (desiredScale.z - startScale.z) * percentElapsed;
        
        Vector3 currScale = transform.localScale;
        currScale.x += moveX;
        currScale.y += moveY;
        currScale.z += moveZ;
        transform.localScale = currScale;

        if (currScale.x >= desiredScale.x)
        {
            StopAnimation();
        }
    }
}
