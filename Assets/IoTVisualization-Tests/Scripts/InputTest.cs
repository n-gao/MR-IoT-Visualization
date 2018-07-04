using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class InputTest : MonoBehaviour, IInputClickHandler
{
    private TouchScreenKeyboard keyboard = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
    void Update()
    {
        if (!TouchScreenKeyboard.visible)
            keyboard = null;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (keyboard == null)
        {
            print("Hi");
            keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false);
        }
    }
}
