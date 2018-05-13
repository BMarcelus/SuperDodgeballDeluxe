using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class IntroAnim : MonoBehaviour {

    Coroutine introCoroutine, flashStartToPlayCoroutine;
    public GameObject startToPlay;
    bool introPlayed, startPressed;

    Canvas currentCanvas;
    Animator currentAnimator;

    void Start() {
        currentCanvas = GameManager.instance.canvas;
        currentAnimator = currentCanvas.GetComponent<Animator>();
        //introCoroutine = StartCoroutine(IntroCoroutine());
    }

    void Update() {
        if (Input.anyKeyDown) {
            if (introPlayed) {
                if (!startPressed) {
                    startPressed = true;
                    StopCoroutine(flashStartToPlayCoroutine);
                    startToPlay.SetActive(false);
                    //GameManager.instance.UI_IntroToMainMenu();
                }
            } else {
                introPlayed = true;
                StopCoroutine(introCoroutine);
                currentAnimator.speed = Mathf.Infinity;
                Camera.main.GetComponent<CameraCommander>().enabled = true;
                GetComponent<AudioSource>().Stop();
                GameManager.instance.musicManager.PlayMenuMusic();
                //GameManager.instance.UI_FlashStartToPlay();
            }
        }
    }

    
}
