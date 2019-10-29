using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPositionUI : MonoBehaviour
{
    public float delta = 5.0f;
    private RectTransform rectTransform = null;
    private UnityEngine.UI.Text uiText = null;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        uiText = GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            rectTransform.anchoredPosition += new Vector2(0, delta);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            rectTransform.anchoredPosition -= new Vector2(0, delta);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            rectTransform.anchoredPosition += new Vector2(delta, 0);
        }           
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            rectTransform.anchoredPosition -= new Vector2(delta, 0);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.LogFormat("[{0}] At position: {1}", 
                gameObject.name, rectTransform.position.ToString("F3"));
        }
    }
}
