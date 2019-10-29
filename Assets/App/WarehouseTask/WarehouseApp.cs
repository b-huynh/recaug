using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseApp : App
{
    public UnityEngine.UI.Text display;
    private float timer = 0.0f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Link(display.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        display.text = timer.ToString();
    }
}
