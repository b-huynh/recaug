using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Label {
	public string className;
	public int xmin;
	public int xmax;
	public int ymin;
	public int ymax;
	public int xcen;
	public int ycen;
}

[System.Serializable]
public class Predictions {
	public List<Label> labels;
}
