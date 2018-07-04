using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationSetter : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		Screen.orientation = ScreenOrientation.Landscape;
	    Application.targetFrameRate = 60;
	}
}
