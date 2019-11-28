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
        string prompt = string.Format("\n   [EVEREST] Employee View\n   Work timer: {0:0.00}\n   Fetch:\n   -> #A-125 (Pillow)", timer);
        // display.text = timer.ToString();
        display.text = prompt;
    }
}
