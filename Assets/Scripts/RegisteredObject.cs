using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegisteredObject : MonoBehaviour {
	public string label;
	public List<string> altLabels;

	public TextMesh altTextLeft;
	public TextMesh altTextRight;
	public TextMesh altTextTop;
	public TextMesh altTextBottom;

	private List<TextMesh> altTextObjs;
	private int maxAltLabels;
	
	Translator translator;

	bool isLabelSet = false;

	void Awake () {
		altLabels = new List<string>();

		altTextRight.gameObject.SetActive(false);
		altTextLeft.gameObject.SetActive(false);
		altTextTop.gameObject.SetActive(false);
		altTextBottom.gameObject.SetActive(false);

		altTextObjs = new List<TextMesh>();
		altTextObjs.Add(altTextRight);
		altTextObjs.Add(altTextLeft);
		altTextObjs.Add(altTextTop);
		altTextObjs.Add(altTextBottom);
		maxAltLabels = altTextObjs.Count;

		translator = new Translator();
		translator.Init(Translator.TargetLanguage.Spanish);
	}

    void Start() {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool ContainsLabel(string l) {
		// Tests if l is any of the alt labels encompassed by this object
		return altLabels.Any(str => str == l);
	}

	public void SwitchLabel(int id) {
		// Switch to one of the other altLabels
		label = altLabels[id];
	}

	public void SwitchLabel(string l) {
		label = l;

		// Remove all other labels except altTextRight now
		altTextRight.GetComponent<TextMesh>().text = translator.translate(l, Translator.TargetLanguage.Spanish);

		altTextLeft.gameObject.SetActive(false);
		altTextTop.gameObject.SetActive(false);
		altTextBottom.gameObject.SetActive(false);

		isLabelSet = true;
	}

	public void AddAltLabel(string l) {
		if (isLabelSet) {
			return;
		}

		if (altLabels.Count < maxAltLabels) {
			altLabels.Add(l);
			if (altLabels.Count == 1) {
				SwitchLabel(0);
			}

            altTextObjs[altLabels.Count - 1].gameObject.SetActive(true);
            altTextObjs[altLabels.Count - 1].text = l;
		}
	}
}
