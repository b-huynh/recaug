using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegisteredObject : MonoBehaviour {
	// public string label;

	// public List<string> altLabels;
	// public TextMesh altTextLeft;
	// public TextMesh altTextRight;
	// public TextMesh altTextTop;
	// public TextMesh altTextBottom;

	public List<Annotation> annotations;
	private Dictionary<Annotation.Orientation, Annotation> assignMap;  

	// private List<TextMesh> altTextObjs;
	// private int maxAltLabels;

	// bool isLabelSet = false;
	public bool confirmed { get; private set; }

	public string label {
		get { return assignMap[Annotation.Orientation.RIGHT].text; }
		private set {}
	}

	void Awake () {
		foreach(Annotation ann in annotations) {
			ann.gameObject.SetActive(false);
		}

		// altLabels = new List<string>();

		// altTextRight.gameObject.SetActive(false);
		// altTextLeft.gameObject.SetActive(false);
		// altTextTop.gameObject.SetActive(false);
		// altTextBottom.gameObject.SetActive(false);

		// altTextObjs = new List<TextMesh>();
		// altTextObjs.Add(altTextRight);
		// altTextObjs.Add(altTextLeft);
		// altTextObjs.Add(altTextTop);
		// altTextObjs.Add(altTextBottom);
		// maxAltLabels = altTextObjs.Count;
	}

    void Start() {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool ContainsLabel(string label) {
		// Tests if l is any of the alt labels encompassed by this object
		// return altLabels.Any(str => str == label);
		return assignMap.Any(kv => kv.Value.text == label);
	}

	// public void SwitchLabel(int id) {
	// 	// Switch to one of the other altLabels
	// 	label = altLabels[id];
	// }

	// public void SwitchLabel(string l) {
	// 	label = l;

	// 	// Remove all other labels except altTextRight now
	// 	altTextRight.GetComponent<TextMesh>().text = Translator.translate(l, Translator.TargetLanguage.Japanese);

	// 	altTextLeft.gameObject.SetActive(false);
	// 	altTextTop.gameObject.SetActive(false);
	// 	altTextBottom.gameObject.SetActive(false);

	// 	isLabelSet = true;
	// }

	// public void SwitchLabel(Annotation.Orientation orientation) {
		
	// }

	public void ConfirmLabel(Annotation.Orientation orientation) {
		// Swap with right annotation
		if (orientation != Annotation.Orientation.RIGHT) {
			string temp = assignMap[orientation].text;
			assignMap[orientation].text = 
				assignMap[Annotation.Orientation.RIGHT].text;
			assignMap[Annotation.Orientation.RIGHT].text = temp;
		}

		// Confirm label
		label = assignMap[Annotation.Orientation.RIGHT].text;
		
		// Translate and hide others
		assignMap[Annotation.Orientation.RIGHT].text =
			Translator.translate(label, Config.UIParams.TargetLanguage);
		assignMap[Annotation.Orientation.LEFT].gameObject.SetActive(false);
		assignMap[Annotation.Orientation.TOP].gameObject.SetActive(false);
		assignMap[Annotation.Orientation.BOTTOM].gameObject.SetActive(false);
		confirmed = true;
	}

	public void AddAltLabel(string label) {
		if (annotations.Count > 0) {
			Annotation ann = annotations[0];
			annotations.RemoveAt(0);
			ann.text = label;
			ann.gameObject.SetActive(true);
			assignMap[ann.orientation] = ann;
		}

		// if (isLabelSet) {
		// 	return;
		// }

		// if (altLabels.Count < maxAltLabels) {
		// 	altLabels.Add(l);
		// 	if (altLabels.Count == 1) {
		// 		SwitchLabel(0);
		// 	}

        //     altTextObjs[altLabels.Count - 1].gameObject.SetActive(true);
        //     altTextObjs[altLabels.Count - 1].text = l;
		// }
	}
}
