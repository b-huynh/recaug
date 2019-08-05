using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardReposition : MonoBehaviour
{
    public float delta = 5.0f;
    public bool debug = false;
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

        if (debug) {
            uiText.text = string.Format("Pos({0:F1}, {1:F1})",
                rectTransform.anchoredPosition.x,
                rectTransform.anchoredPosition.y);
        }
    }
}
