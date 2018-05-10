using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMover : MonoBehaviour {
    Vector2 from, to;
    float lerpTime, lerpCurrent;
    bool smooth;

    void Start() {
        if (GetComponent<RectTransform>() == null) {
            Debug.LogWarning("No RectTransform on this object! UIMover WILL NOT work!");
        }
    }

    public void Move(Vector2 moveFrom, Vector2 moveTo, float time, bool makeItSmooth) {
        from = moveFrom;
        to = moveTo;
        lerpTime = time;
        lerpCurrent = 0f;
        smooth = makeItSmooth;
    }

    public void Restart() {
        lerpCurrent = 0f;
    }

    void Update() {
        if (lerpCurrent / lerpTime < 1f) {
            Vector2 move;
            if (smooth) {
                move = new Vector2(
                Mathf.Lerp(from.x, to.x, lerpCurrent / lerpTime),
                Mathf.Lerp(from.y, to.y, lerpCurrent / lerpTime));
            } else {
                move = new Vector2(
                Mathf.SmoothStep(from.x, to.x, lerpCurrent / lerpTime),
                Mathf.SmoothStep(from.y, to.y, lerpCurrent / lerpTime));
            }
            
            lerpCurrent += Time.deltaTime;
            gameObject.GetComponent<RectTransform>().anchoredPosition = move;
        }
        if (lerpCurrent / lerpTime > 1f) {
            // Put GO exactly in place when the end is reached
            lerpCurrent = lerpTime;
            gameObject.transform.localPosition = to;
        }
    }
}
