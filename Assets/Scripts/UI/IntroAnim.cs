using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class IntroAnim : MonoBehaviour {

    public GameManager gameManager;
    Coroutine introCoroutine, flashStartToPlayCoroutine;
    public GameObject startToPlay;
    bool introPlayed, startPressed;

    void Awake() {
        introCoroutine = StartCoroutine(IntroCoroutine());
    }

    void Update() {
        if (Input.anyKeyDown) {
            if (introPlayed) {
                if (!startPressed) {
                    startPressed = true;
                    StopCoroutine(flashStartToPlayCoroutine);
                    startToPlay.SetActive(false);
                    gameManager.UI_IntroToMainMenu();
                }
            } else {
                introPlayed = true;
                StopCoroutine(introCoroutine);
                GetComponent<Animator>().speed = Mathf.Infinity;
                Camera.main.GetComponent<CameraCommander>().enabled = true;
                GetComponent<AudioSource>().Stop();
                gameManager.musicManager.PlayMenuMusic();
                flashStartToPlayCoroutine = StartCoroutine(FlashStartToPlayCoroutine());
            }
        }
    }

    IEnumerator IntroCoroutine() {
        GetComponent<Animator>().Play("Intro");
        GetComponent<Animator>().speed = 0;

        yield return new WaitForSeconds(0.8f); // Needs a delay so the game doesn't stutter, feel free to adjust
        GetComponent<AudioSource>().Play();
        GetComponent<Animator>().speed = 1;

        yield return new WaitForSeconds(3.1f);
        Camera.main.GetComponent<CameraCommander>().enabled = true;
        yield return new WaitForSeconds(2f);
        gameManager.musicManager.PlayMenuMusic();
        yield return new WaitForSeconds(1f);
        introPlayed = true;
        flashStartToPlayCoroutine = StartCoroutine(FlashStartToPlayCoroutine());
    }

    IEnumerator FlashStartToPlayCoroutine() {
        while (!false) {
            startToPlay.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            startToPlay.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
