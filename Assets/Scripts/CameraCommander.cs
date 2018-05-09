using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCommander : MonoBehaviour {

    // Each rot axis can move differently
    public Vector3 shakeAmount;
    public Vector3 shakeSpeed;

    public GameObject stareObject; // Set to something to make camera lock onto it

    bool doinALittleShake;

    Vector3 realRotation, shakeRotation;

    public Vector3 originalPos;

    void Start() {
        originalPos = transform.position;
        DoSomeShake(true);
    }

	void Update () {
        if (stareObject) {
            transform.LookAt(stareObject.transform);
            if (doinALittleShake) {
                // Have to set it up like this in order for shake to still work
                realRotation = transform.localEulerAngles;
                transform.localEulerAngles = realRotation + shakeRotation;
            }
        }
	}


    void LateUpdate() {
        if (doinALittleShake) {
            realRotation = transform.localEulerAngles - shakeRotation;

            // Each axis is animated differently, making it feel more organic
            shakeRotation = new Vector3(
                shakeAmount.x * Mathf.Sin(shakeSpeed.x * Time.timeSinceLevelLoad),
                shakeAmount.y * Mathf.Cos(shakeSpeed.y * Time.timeSinceLevelLoad),
                shakeAmount.z * Mathf.Sin(shakeSpeed.z * Time.timeSinceLevelLoad));

            transform.localEulerAngles = realRotation + shakeRotation;
        }
    }

    public void StareAtObject(GameObject obj) {
        stareObject = obj;
    }

    public void DoSomeShake(bool shake) {
        if (doinALittleShake = shake) {
            shakeRotation = Vector3.zero; // Reset shake
        } else {
            transform.localEulerAngles -= shakeRotation; // Reset look
        }
    }
}
