using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCommander : MonoBehaviour {

    public Vector3 originalPos;

    [Header("Shaking")]
    // Each rot axis can move differently
    public Vector3 shakeAmount;
    public Vector3 shakeSpeed;

    bool doinALittleShake;

    Vector3 realRotation, shakeRotation;

    [Header("Staring")]
    public GameObject stareObject; // Set to something to make camera lock onto it

    [Header("Rotating around point")]
    public bool doRotate;
    public Vector3 rotateAroundPoint;
    public float rotateSpeed;

    void Start() {
        originalPos = transform.position;
        DoSomeShake(true);

        if (doRotate) {
            transform.LookAt(rotateAroundPoint);
            if (doinALittleShake) {
                realRotation = transform.localEulerAngles;
                transform.localEulerAngles = realRotation + shakeRotation;
            }
        }
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

        if (doRotate) {
            transform.RotateAround(rotateAroundPoint, Vector3.up, rotateSpeed);
        }
	}


    void LateUpdate() {
        if (doinALittleShake) {
            realRotation = transform.localEulerAngles - shakeRotation;

            // Each axis is animated separately
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
