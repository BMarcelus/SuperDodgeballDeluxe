using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ControlButton : MonoBehaviour {
    public Text text;
    public KeyCode keyCode;

    void Start() {
        SetUp();
    }

    public void SetUp() {
        text = transform.GetChild(0).GetComponent<Text>();
        if (text == null) {
            Debug.LogError("CONTROL BUTTON '" + gameObject.name + "' HAS NO TEXT COMPONENT!");
            return;
        }
        try {
            keyCode = (KeyCode) Enum.Parse(typeof(KeyCode), text.text); 
        } catch (Exception) {
            Debug.LogError("TEXT ON CONTROL BUTTON '" + gameObject.name + "' DOES NOT CORRESPOND TO A KEYCODE!");
        }
    }
}
