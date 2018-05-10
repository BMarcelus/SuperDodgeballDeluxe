using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour {

    public GameObject fadeObject;
    Coroutine currentFade;

    // Don't really need this if we put a fader in each scene

    //void Awake() {
    //    if (fadeObject == null) {
    //        fadeObject = new GameObject("Fade", typeof(Image));
    //        fadeObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
    //        fadeObject.transform.SetSiblingIndex(fadeObject.transform.GetSiblingIndex() - 1);
    //        fadeObject.GetComponent<RectTransform>().anchorMin = Vector2.zero;
    //        fadeObject.GetComponent<RectTransform>().anchorMax = Vector2.one;
    //        fadeObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
    //        fadeObject.GetComponent<Image>().raycastTarget = false;
    //    }
    //    fadeObject.GetComponent<Image>().color = Vector4.zero;
    //}

    public void ClearColor() {
        fadeObject.GetComponent<Image>().color = Vector4.zero;
        if (currentFade != null) {
            StopCoroutine(currentFade);
        }
    }

    public void FadeFrom(Color color, float seconds, bool useFixedDeltaTime = false) {
        if (currentFade != null) {
            StopCoroutine(currentFade);
        }
        currentFade = StartCoroutine(Fade(color, Vector4.zero, seconds, useFixedDeltaTime));
    }

    public void FadeTo(Color color, float seconds, bool useFixedDeltaTime = false) {
        if (currentFade != null) {
            StopCoroutine(currentFade);
        }
        currentFade = StartCoroutine(Fade(Vector4.zero, color, seconds, useFixedDeltaTime));
    }

    public void FadeFromTo(Color from, Color to, float seconds, bool useFixedDeltaTime = false) {
        if (currentFade != null) {
            StopCoroutine(currentFade);
        }
        currentFade = StartCoroutine(Fade(from, to, seconds, useFixedDeltaTime));
    }

    private IEnumerator Fade(Color colorFrom, Color colorTo, float seconds, bool useFixedDeltaTime) {
        float startTime = Time.time;
        while (Time.time < startTime + seconds) {
            float ratio = (Time.time - startTime) / seconds;
            fadeObject.GetComponent<Image>().color = Color.Lerp(colorFrom, colorTo, ratio);

            if (useFixedDeltaTime) {
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            } else {
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }

        fadeObject.GetComponent<Image>().color = colorTo;
    }
}
