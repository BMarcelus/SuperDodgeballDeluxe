using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public MusicManager musicManager;
    public Image blackFade;
    public IntroAnim intro;
    public GameObject introUI, menuUI;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        blackFade.gameObject.SetActive(true);
        intro.gameObject.SetActive(true);
        introUI.SetActive(true);
        menuUI.SetActive(true);
    }

    public void UI_IntroToMainMenu() {
        intro.gameObject.SetActive(false);
        // Grab the intro parent and main menu parent, slide them both up 1 screen's worth
        introUI.GetComponent<UIMover>().Move(introUI.GetComponent<RectTransform>().anchoredPosition,
            new Vector2(introUI.GetComponent<RectTransform>().anchoredPosition.x, introUI.GetComponent<RectTransform>().anchoredPosition.y + Screen.height),
            1f, false);
        menuUI.GetComponent<UIMover>().Move(menuUI.GetComponent<RectTransform>().anchoredPosition,
            new Vector2(menuUI.GetComponent<RectTransform>().anchoredPosition.x, menuUI.GetComponent<RectTransform>().anchoredPosition.y + Screen.height),
            1f, false);
    }
}
