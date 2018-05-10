using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public MusicManager musicManager;
    public ScreenFader blackFade;
    public IntroAnim intro;
    public GameObject introUI, menuUI;

    bool introPlayed;

    void Awake() {
        // Singleton stuff, needs to carry across multiple scenes
        if(instance != null && instance != this)
            Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(gameObject);

        blackFade.gameObject.SetActive(true);
        intro.gameObject.SetActive(true);
        introUI.SetActive(true);
        menuUI.SetActive(true);
        introPlayed = true;
    }

    public void UI_IntroToMainMenu() {
        intro.gameObject.SetActive(false);
        blackFade.FadeFromTo(Color.clear, new Color(0,0,0,0.25f), 1);
        // Grab the intro parent and main menu parent, slide them both up 1 screen's worth (using Canvas Scaler so that's the reference height)
        float yUp = GameObject.Find("Canvas").GetComponent<CanvasScaler>().referenceResolution.y;
        introUI.GetComponent<UIMover>().Move(introUI.GetComponent<RectTransform>().anchoredPosition,
            new Vector2(introUI.GetComponent<RectTransform>().anchoredPosition.x, introUI.GetComponent<RectTransform>().anchoredPosition.y + yUp),
            1f, false);
        menuUI.GetComponent<UIMover>().Move(menuUI.GetComponent<RectTransform>().anchoredPosition,
            new Vector2(menuUI.GetComponent<RectTransform>().anchoredPosition.x, menuUI.GetComponent<RectTransform>().anchoredPosition.y + yUp),
            1f, false);
    }
}
