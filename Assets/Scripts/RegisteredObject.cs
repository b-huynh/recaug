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
	

	void Awake () {
		altLabels = new List<string>();

		altTextLeft.gameObject.SetActive(false);
		altTextRight.gameObject.SetActive(false);
		altTextTop.gameObject.SetActive(false);
		altTextBottom.gameObject.SetActive(false);

		altTextObjs = new List<TextMesh>();
		altTextObjs.Add(altTextLeft);
		altTextObjs.Add(altTextRight);
		altTextObjs.Add(altTextTop);
		altTextObjs.Add(altTextBottom);
		maxAltLabels = altTextObjs.Count;
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

	public void AddAltLabel(string l) {
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
