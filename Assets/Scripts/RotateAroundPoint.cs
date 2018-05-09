using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateAroundPoint : MonoBehaviour {

    public float speed;
    public Vector3 centerPoint;
    public bool isPlane;

    void Start () {
        if (isPlane) {
            transform.LookAt(centerPoint);
            transform.Rotate(Vector3.right, 90);
        } else {
            transform.LookAt(centerPoint);
        }
    }
    
    void Update () {
        
    }

    void FixedUpdate() {
        transform.RotateAround(centerPoint, Vector3.up, speed);
    }
}
