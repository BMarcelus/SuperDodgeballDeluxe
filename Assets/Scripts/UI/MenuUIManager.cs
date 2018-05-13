using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class MenuUIManager : MonoBehaviour {

    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public Button lanButton, netButton, optionsButton, quitButton, creditsButton;

    [Header("Lan Menu")]
    public GameObject lanParent;
    public GameObject lanMenuPanel, lanConnectingPanel;
    public Button lanHostButton, lanJoinButton, lanBackButton;
    public InputField lanIPInputField;

    [Header("Net Menu")]
    public GameObject netParent;
    public GameObject netMenuPanel, netConnectingPanel;
    public Button netHostButton, netJoinButton, netRefreshButton, netBackButton;
    public InputField netMatchNameInputField;
    public GameObject netScrollViewContent;
    public Text netMatchListErrorText;
    public GameObject netMatchPanelPrefab;

    [Header("Options Menu")]
    public GameObject optionsPanel;

    [Header("Misc")]
    public GameObject creditsPanel;
    public GameObject introItems, titleUI, startToPlayText;

    MatchInfoSnapshot currentSelectedMatchSnapshot;
    Coroutine introCoroutine, flashStartToPlayCoroutine;
    bool introPlayed, startToPlayPressed;

    GameManager gm;
    CustomNetworkManager nm;
    Canvas canvas;
    Animator animator;

    public void Start() {
        nm = CustomNetworkManager.instance;
    }

    public void PlayIntro() {
        // This is the first thing that runs, before Start, called from Awake in GameManager
        gm = GameManager.instance;
        
        canvas = gm.canvas;
        animator = canvas.GetComponent<Animator>();

        gm.blackFade.gameObject.SetActive(true);
        introItems.SetActive(true);
        titleUI.SetActive(true);
        mainMenuPanel.SetActive(true);
        lanParent.SetActive(true);
        netParent.SetActive(true);
        creditsPanel.SetActive(true);
        introCoroutine = StartCoroutine(IntroCoroutine());
    }

    void Update() {
        // Intro anim stuff
        if (Input.anyKeyDown) {
            if (introPlayed) {
                if (!startToPlayPressed) {
                    startToPlayPressed = true;
                    StopCoroutine(flashStartToPlayCoroutine);
                    startToPlayText.SetActive(false);
                    introItems.SetActive(false);
                    gm.blackFade.FadeFromTo(Color.clear, new Color(0,0,0,0.25f), 1);
                    animator.speed = 1;
                    animator.Play("UI_PressStart");
                }
            } else {
                introPlayed = true;
                StopCoroutine(introCoroutine);
                animator.speed = Mathf.Infinity;
                Camera.main.GetComponent<CameraCommander>().enabled = true;
                gm.musicManager.StopIntro();
                gm.musicManager.PlayMenuMusic();
                flashStartToPlayCoroutine = StartCoroutine(FlashStartToPlayCoroutine());
            }
        }
    }

    public void MainMenu_OptionsPressed() {

    }

    public void MainMenu_QuitPressed() {
        Application.Quit();
    }

    /////////////////////
    // Credits Buttons //
    /////////////////////

    public void MainMenu_CreditsPressed() {
        animator.SetFloat("IntroToCreditsSpeed", 1);
        animator.Play("UI_IntroToCredits", 0, 0);
    }

    public void Credits_BackPressed() {
        animator.SetFloat("IntroToCreditsSpeed", -1);
        animator.Play("UI_IntroToCredits", 0, 1);
    }
    
    ////////////////////////
    // LAN (Local) Events //
    ////////////////////////

    public void MainMenu_LanPressed() {
        animator.SetFloat("IntroToLanSpeed", 1);
        animator.Play("UI_IntroToLan", 0, 0);
    }

    public void Lan_BackPressed() {
        animator.SetFloat("IntroToLanSpeed", -1);
        animator.Play("UI_IntroToLan", 0 , 1);
    }

    public void Lan_HostPressed() {
        if (lanIPInputField.text.Length == 0) {
            nm.networkAddress = "localhost";
        } else {
            nm.networkAddress = lanIPInputField.text;
        }
        nm.StartHost();

        lanMenuPanel.SetActive(false);
        lanBackButton.gameObject.SetActive(false);
        lanConnectingPanel.SetActive(true);
    }

    public void Lan_JoinPressed() {
        if (lanIPInputField.text.Length == 0) {
            nm.networkAddress = "localhost";
        } else {
            nm.networkAddress = lanIPInputField.text;
        }
        NetworkClient client = nm.StartClient();

        lanMenuPanel.SetActive(false);
        lanBackButton.gameObject.SetActive(false);
        lanConnectingPanel.SetActive(true);
        StartCoroutine(HideLanConnectingPanel(client));
    }

    public void Lan_CancelPressed() {
        nm.StopHost();
        nm.StopClient();
        
        lanMenuPanel.SetActive(true);
        lanBackButton.gameObject.SetActive(true);
        lanConnectingPanel.SetActive(false);
    }

    IEnumerator HideLanConnectingPanel(NetworkClient client) {
        // There's no callback for a successful LAN client connection that I found, so this'll have to do
        yield return new WaitUntil(delegate {
            return client.isConnected || !lanConnectingPanel.activeSelf;
        });
        lanConnectingPanel.SetActive(false);
    }

    //////////////////////////////
    // Net (Matchmaking) Events //
    //////////////////////////////

    public void MainMenu_NetPressed() {
        animator.SetFloat("IntroToNetSpeed", 1);
        animator.Play("UI_IntroToNet", 0, 0);

        nm.StartMatchMaker();
        
        nm.matchMaker.ListMatches(0, 32, "", false, 0, 0, OnMatchList);
        netJoinButton.interactable = false;
    }

    public void Net_BackPressed() {
        animator.SetFloat("IntroToNetSpeed", -1);
        animator.Play("UI_IntroToNet", 0 , 1);

        nm.StopMatchMaker();

        foreach (Transform childTransform in netScrollViewContent.transform) {
            Destroy(childTransform.gameObject);
        }
    }

    public void Net_HostPressed() {
        nm.matchMaker.CreateMatch(netMatchNameInputField.text == "" ? nm.matchName : netMatchNameInputField.text, nm.matchSize, true, "", "", "", 0, 0, nm.OnMatchCreate);
        netHostButton.interactable = false;
        netJoinButton.interactable = false;
        netMenuPanel.SetActive(false);
        netBackButton.gameObject.SetActive(false);
        netConnectingPanel.SetActive(true);
    }

    public void Net_JoinPressed() {
        nm.matchMaker.JoinMatch(currentSelectedMatchSnapshot.networkId, "", "", "", 0, 0, nm.OnMatchJoined);
        netHostButton.interactable = false;
        netJoinButton.interactable = false;
        netMenuPanel.SetActive(false);
        netBackButton.gameObject.SetActive(false);
        netConnectingPanel.SetActive(true);
    }

    public void Net_RefreshPressed() {
        nm.matchMaker.ListMatches(0, 32, "", false, 0, 0, OnMatchList);
        netJoinButton.interactable = false;
        currentSelectedMatchSnapshot = null;
        netMatchListErrorText.gameObject.SetActive(false);
        netRefreshButton.interactable = false;
    }

    public void Net_CancelPressed() {
        nm.matchMaker.DropConnection(currentSelectedMatchSnapshot.networkId, 0, 0, null);
        Net_RefreshPressed();
        currentSelectedMatchSnapshot = null;
        netMenuPanel.SetActive(true);
        netBackButton.gameObject.SetActive(true);
        netConnectingPanel.SetActive(false);
    }

    ////////////////////////////////////
    // Match List Delegates/Callbacks //
    ////////////////////////////////////

    void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {
        netRefreshButton.interactable = true;
        if (success) {
            netMatchListErrorText.gameObject.SetActive(matches.Count == 0);
            foreach (Transform childTransform in netScrollViewContent.transform) {
                Destroy(childTransform.gameObject);
            }
            foreach (MatchInfoSnapshot match in matches) {
                GameObject matchListItem = Instantiate(netMatchPanelPrefab, netScrollViewContent.transform);
                matchListItem.GetComponent<Toggle>().onValueChanged.AddListener(delegate {MatchListItemSelected(matchListItem.GetComponent<Toggle>().isOn);});
                matchListItem.GetComponent<Toggle>().group = netScrollViewContent.GetComponent<ToggleGroup>();
                matchListItem.GetComponent<Toggle>().interactable = (match.currentSize < match.maxSize);
                matchListItem.transform.GetChild(0).GetComponent<Text>().text = match.name;
                matchListItem.transform.GetChild(1).GetComponent<Text>().text = string.Format("{0}/{1}", match.currentSize, match.maxSize);
                matchListItem.GetComponent<NetMatchListItem>().matchInfoSnapshot = match;
            }
        } else {
            netMatchListErrorText.gameObject.SetActive(true);
        }
    }

    public void MatchListItemSelected(bool value) {
        // This is inconvenient, can't pass WHICH Toggle was selected)... but Toggle Group is set so only one will be active
        Toggle selectedToggle = null;
        foreach (Toggle toggle in netScrollViewContent.GetComponent<ToggleGroup>().ActiveToggles()) {
            selectedToggle = toggle;
        }

        if (value) {
            currentSelectedMatchSnapshot = selectedToggle.GetComponent<NetMatchListItem>().matchInfoSnapshot;
            netJoinButton.interactable = true;
        } else {
            if (selectedToggle == null || EventSystem.current.currentSelectedGameObject != selectedToggle.gameObject) {
                // Same entry was selected twice (turns it off), so turn off EventSystem UI selection because it looks bad
                netJoinButton.interactable = false;
                currentSelectedMatchSnapshot = null;
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public void HideThingsOnJoined() {
        lanConnectingPanel.SetActive(false);
        netConnectingPanel.SetActive(false);
        gm.blackFade.gameObject.SetActive(false);
        titleUI.SetActive(false);
    }

    IEnumerator IntroCoroutine() {
        animator.Play("UI_Intro");
        animator.speed = 0;

        yield return new WaitForSeconds(0.8f); // Needs a delay so the game doesn't stutter, feel free to adjust
        gm.musicManager.PlayIntroMusic();
        animator.speed = 1;

        yield return new WaitForSeconds(3.1f);
        Camera.main.GetComponent<CameraCommander>().enabled = true;
        yield return new WaitForSeconds(2f);
        gm.musicManager.StopIntro();
        gm.musicManager.PlayMenuMusic();
        yield return new WaitForSeconds(1f);
        introPlayed = true;
        flashStartToPlayCoroutine = StartCoroutine(FlashStartToPlayCoroutine());
    }

    IEnumerator FlashStartToPlayCoroutine() {
        while (!false) {
            startToPlayText.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            startToPlayText.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
