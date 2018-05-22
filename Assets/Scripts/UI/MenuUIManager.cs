using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
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
    public GameObject optionsParent;
    public Button optionsBackButton, optionsApplyButton, optionsSettingsTab, optionsControlsTab, optionsPlayerTab;
    public GameObject optionsSettingsPanel, optionsControlsPanel, optionsPlayerPanel;
    // Settings
    public Dropdown resolutionDropdown, graphicsDropdown;
    public Toggle fullscreenToggle;
    public Slider fovSlider, mouseXSlider, mouseYSlider, musicSlider, sfxSlider, crowdSlider;
    public Text fovText, mouseXText, mouseYText;
    // Controls
    public Button lButton, lAltButton, rButton, rAltButton, uButton, uAltButton, dButton, dAltButton;
    public Button jumpButton, jumpAltButton, fire1Button, fire1AltButton, fire2Button, fire2AltButton;
    public Toggle joystickToggle;
    // Player
    public InputField playerNameInputField;
    public ColorPicker playerColorPicker;

    public Color controlModified;

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

    Coroutine controlPressedCoroutine;
    List<Button> controlButtons; // Makes it easier to go through them
    bool controlPressedCancelled;

    public void Start() {
        gm = GameManager.instance;
        nm = CustomNetworkManager.instance;

        controlButtons = new List<Button>();
        controlButtons.AddRange(new Button[] { lButton, lAltButton, rButton, rAltButton, uButton, uAltButton, dButton, dAltButton, jumpButton, jumpAltButton, fire1Button, fire1AltButton, fire2Button, fire2AltButton });

        foreach (Button b in controlButtons) {
            b.onClick.AddListener(delegate { ControlBtnPressed(b);});
        }
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
        optionsParent.SetActive(true);
        creditsPanel.SetActive(true);
        introCoroutine = StartCoroutine(IntroCoroutine());
    }

    void Update() {
        if (!gm.gameInProgress) {
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
                    gm.audioManager.StopIntro();
                    gm.audioManager.PlayMenuMusic();
                    flashStartToPlayCoroutine = StartCoroutine(FlashStartToPlayCoroutine());
                }
            }
        }
    }

    public void MainMenu_QuitPressed() {
        Application.Quit();
    }

    /////////////////////
    //  Options Panel  //
    /////////////////////

    void SetUISettings(GameSettings settings) {
        int resolutionIndex = GetIndexForResolution(settings.resolutionX, settings.resolutionY);
        resolutionDropdown.value = resolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = settings.fullscreen;
        graphicsDropdown.value = settings.gfxPreset;
        graphicsDropdown.RefreshShownValue();

        fovSlider.value = settings.fov;
        mouseXSlider.value = settings.mouseSensitivityX;
        mouseYSlider.value = settings.mouseSensitivityY;
        musicSlider.value = settings.musicVolume;
        sfxSlider.value = settings.sfxVolume;
        crowdSlider.value = settings.crowdVolume;

        playerColorPicker.AssignColor(ColorValues.R, settings.playerColor.r);
        playerColorPicker.AssignColor(ColorValues.G, settings.playerColor.g);
        playerColorPicker.AssignColor(ColorValues.B, settings.playerColor.b);

        playerNameInputField.text = settings.playerName;
    }

    void SetUIKeys(InputKeys keys) {
        lButton.GetComponent<ControlButton>().keyCode = keys.left;
        lAltButton.GetComponent<ControlButton>().keyCode = keys.leftAlt;
        rButton.GetComponent<ControlButton>().keyCode = keys.right;
        rAltButton.GetComponent<ControlButton>().keyCode = keys.rightAlt;
        uButton.GetComponent<ControlButton>().keyCode = keys.up;
        uAltButton.GetComponent<ControlButton>().keyCode = keys.upAlt;
        dButton.GetComponent<ControlButton>().keyCode = keys.down;
        dAltButton.GetComponent<ControlButton>().keyCode = keys.downAlt;
        jumpButton.GetComponent<ControlButton>().keyCode = keys.jump;
        jumpAltButton.GetComponent<ControlButton>().keyCode = keys.jumpAlt;
        fire1Button.GetComponent<ControlButton>().keyCode = keys.fire1;
        fire1AltButton.GetComponent<ControlButton>().keyCode = keys.fire1Alt;
        fire2Button.GetComponent<ControlButton>().keyCode = keys.fire2;
        fire2AltButton.GetComponent<ControlButton>().keyCode = keys.fire2Alt;
        joystickToggle.isOn = keys.joystick;

        foreach (Button b in controlButtons) {
            b.GetComponent<ControlButton>().text.text = b.GetComponent<ControlButton>().keyCode.ToString();
        }
    }

    public void MainMenu_OptionsPressed() {
        animator.SetFloat("IntroToOptionsSpeed", 1);
        animator.Play("UI_IntroToOptions", 0, 0);

        // Get settings from Game Manager and apply to UI elements
        SharpConfig.Configuration config = gm.GetConfiguration();
        GameSettings settings = gm.ConfigurationToGameSettings(config);
        SetUISettings(settings);

        foreach (Button b in controlButtons) {
            b.GetComponent<Image>().color = Color.white;
        }

        fovSlider.minValue = gm.cameraFovClamp.x;
        fovSlider.maxValue = gm.cameraFovClamp.y;
    }

    public void Options_BackPressed() {
        animator.SetFloat("IntroToOptionsSpeed", -1);
        animator.Play("UI_IntroToOptions", 0, 1);

        if (controlPressedCoroutine != null) {
            StopCoroutine(controlPressedCoroutine);
            controlPressedCoroutine = null;
            controlPressedCancelled = false;
        }
    }

    public void Options_SettingsRevertPressed() {
        GameSettings defaults = gm.GetDefaultSettings();
        SetUISettings(defaults);
    }

    int GetIndexForResolution(float x, float y) {
        for (int i = 0; i < resolutionDropdown.options.Count; ++i) {
            Dropdown.OptionData option = resolutionDropdown.options[i];
            if (!option.text.StartsWith("Custom")) {
                string[] resStr = option.text.Split('x');
                int optionX = int.Parse(resStr[0]);
                int optionY = int.Parse(resStr[1]);
                if (x == optionX && y == optionY) {
                    return i;
                }
            }
        }
        return resolutionDropdown.options.Count-1; // Fallback is the last item, "Custom"
    }

    public void Options_SettingsApplyPressed() {
        GameSettings settings = new GameSettings();

        string resStr = resolutionDropdown.options[resolutionDropdown.value].text;
        if (resStr.StartsWith("Custom")) {
            KeyValuePair<int, int> customRes = gm.GetCustomResolution();
            settings.resolutionX = customRes.Key;
            settings.resolutionY = customRes.Value;
        } else {
            settings.resolutionX = int.Parse(resStr.Split('x')[0]);
            settings.resolutionY = int.Parse(resStr.Split('x')[1]);
        }
        settings.fullscreen = fullscreenToggle.isOn;
        settings.gfxPreset = graphicsDropdown.value;
        settings.fov = (int) fovSlider.value;
        settings.mouseSensitivityX = mouseXSlider.value;
        settings.mouseSensitivityY =  mouseYSlider.value;
        settings.musicVolume = musicSlider.value;
        settings.sfxVolume = sfxSlider.value;
        settings.crowdVolume = crowdSlider.value;

        gm.SetOptionSettings(settings);
    }

    public void Options_ControlsRevertPressed() {
        InputKeys defaults = InputManager.GetDefaultKeys();
        SetUIKeys(defaults);

        // Set the text here, rather than doubling the mess above
        foreach (Button b in controlButtons) {
            b.GetComponent<ControlButton>().text.text = b.GetComponent<ControlButton>().keyCode.ToString();
            b.GetComponent<Image>().color = Color.white;
        }
    }

    public void Options_ControlApplyPressed() {
        InputKeys keys = new InputKeys {
            left = lButton.GetComponent<ControlButton>().keyCode,
            leftAlt = lAltButton.GetComponent<ControlButton>().keyCode,
            right = rButton.GetComponent<ControlButton>().keyCode,
            rightAlt = rAltButton.GetComponent<ControlButton>().keyCode,
            up = uButton.GetComponent<ControlButton>().keyCode,
            upAlt = uAltButton.GetComponent<ControlButton>().keyCode,
            down = dButton.GetComponent<ControlButton>().keyCode,
            downAlt = dAltButton.GetComponent<ControlButton>().keyCode,
            jump = jumpButton.GetComponent<ControlButton>().keyCode,
            jumpAlt= jumpAltButton.GetComponent<ControlButton>().keyCode,
            fire1 = fire1Button.GetComponent<ControlButton>().keyCode,
            fire1Alt = fire1AltButton.GetComponent<ControlButton>().keyCode,
            fire2 = fire2Button.GetComponent<ControlButton>().keyCode,
            fire2Alt = fire2AltButton.GetComponent<ControlButton>().keyCode,
            joystick = joystickToggle.isOn
        };

        InputManager.SetKeys(keys);

        gm.SaveControlsToConfig(keys);

        foreach (Button b in controlButtons) {
            b.GetComponent<Image>().color = Color.white;
        }
    }

    public void Options_PlayerApplyPressed() {
        GameSettings settings = new GameSettings() {
            playerName = playerNameInputField.text,
            playerColor = playerColorPicker.CurrentColor
        };
        gm.SetOptionPlayer(settings);
    }

    public void OptionsTab_SettingsPressed() {
        OptionsTabActivate(true, false, false);
        SharpConfig.Configuration config = gm.GetConfiguration();
        GameSettings settings = gm.ConfigurationToGameSettings(config);
        SetUISettings(settings);
    }

    public void OptionsTab_ControlsPressed() {
        OptionsTabActivate(false, true, false);
        foreach (Button b in controlButtons) { // Have to do this manually, unfortunately
            b.GetComponent<ControlButton>().SetUp();
        }
        SharpConfig.Configuration config = gm.GetConfiguration();
        InputKeys keys = gm.ConfigurationToInputKeys(config);
        SetUIKeys(keys);
    }

    public void OptionsTab_PlayerPressed() {
        OptionsTabActivate(false, false, true);
        SharpConfig.Configuration config = gm.GetConfiguration();
        GameSettings settings = gm.ConfigurationToGameSettings(config);
        SetUISettings(settings);
    }

    void OptionsTabActivate(bool settings, bool controls, bool player) {
        optionsSettingsTab.interactable = !settings;
        optionsSettingsPanel.SetActive(settings);

        optionsControlsTab.interactable = !controls;
        optionsControlsPanel.SetActive(controls);

        optionsPlayerTab.interactable = !player;
        optionsPlayerPanel.SetActive(player);
    }

    public void Options_MusicSliderChanged(float value) {
        gm.audioManager.musicVolume = value;
    }

    public void Options_SFXSliderChanged(float value) {
        gm.audioManager.sfxVolume = value;
    }

    public void Options_CrowdSliderChanged(float value) {
        // TODO change volume levels when crowd becomes a thing
    }

    public void Options_FOVSliderChanged(float value) {
        fovText.text = value.ToString("00");
    }

    public void Options_MouseXSliderChanged(float value) {
        mouseXText.text = value.ToString("0.0");
    }

    public void Options_MouseYSliderChanged(float value) {
        mouseYText.text = value.ToString("0.0");

    }

    void ControlBtnPressed(Button button) {
        if (controlPressedCoroutine == null) {
            controlPressedCoroutine = StartCoroutine(ControlPressedCoroutine(button));
        } else {
            controlPressedCancelled = true;
            controlPressedCoroutine = null;
        }
    }

    bool AnotherControlButtonAssignedTo(Button thisButton, KeyCode kcode) {
        foreach (Button otherButton in controlButtons) {
            if (thisButton != otherButton && kcode == otherButton.GetComponent<ControlButton>().keyCode) {
                return true;
            }
        }
        return false;
    }

    IEnumerator ControlPressedCoroutine(Button button) {
        Text keyText = button.GetComponent<ControlButton>().text;
        KeyCode previous = button.GetComponent<ControlButton>().keyCode;

        keyText.text = "...";

        yield return new WaitUntil(delegate { return Input.anyKeyDown || controlPressedCancelled; });

        if (controlPressedCancelled || Input.GetKeyDown(KeyCode.Backspace)) {
            keyText.text = previous.ToString();
        } else {
            foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(kcode)) {
                    if (AnotherControlButtonAssignedTo(button, kcode)) {
                        keyText.text = previous.ToString();
                        break;
                    } else {
                        // Successful key bind
                        button.GetComponent<ControlButton>().keyCode = kcode;
                        button.GetComponent<ControlButton>().text.text = kcode.ToString();
                        button.GetComponent<Image>().color = controlModified;
                        break;
                    }
                }
            }
        }
        controlPressedCoroutine = null;
        controlPressedCancelled = false;
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
        lanMenuPanel.SetActive(false);
        lanBackButton.gameObject.SetActive(false);
        lanConnectingPanel.SetActive(true);

        NetworkClient client = nm.StartHost();
        CustomNetworkManager.instance.connToServer = client.connection;
    }

    public void Lan_JoinPressed() {
        if (lanIPInputField.text.Length == 0) {
            nm.networkAddress = "localhost";
        } else {
            nm.networkAddress = lanIPInputField.text;
        }
        lanMenuPanel.SetActive(false);
        lanBackButton.gameObject.SetActive(false);
        lanConnectingPanel.SetActive(true);

        NetworkClient client = nm.StartClient();

        StartCoroutine(ClientJoinedCoroutine(client));
    }

    public void Lan_CancelPressed() {
        nm.StopHost();
        nm.StopClient();
        
        lanMenuPanel.SetActive(true);
        lanBackButton.gameObject.SetActive(true);
        lanConnectingPanel.SetActive(false);
    }

    IEnumerator ClientJoinedCoroutine(NetworkClient client) {
        // There's no callback for a successful LAN client connection that I found, so this'll have to do
        yield return new WaitUntil(delegate {
            return client.isConnected || !lanConnectingPanel.activeSelf;
        });
        lanConnectingPanel.SetActive(false);
        if (client.isConnected)
            gm.StartGameClient(client);
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
        gm.audioManager.PlayIntroMusic();
        animator.speed = 1;

        yield return new WaitForSeconds(3.1f);
        Camera.main.GetComponent<CameraCommander>().enabled = true;
        yield return new WaitForSeconds(2f);
        gm.audioManager.StopIntro();
        gm.audioManager.PlayMenuMusic();
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
