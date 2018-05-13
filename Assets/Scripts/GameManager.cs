using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharpConfig;

public class GameManager : MonoBehaviour {

    public static GameManager instance;
    public Canvas canvas; // Ref here so no UI elements have to GameObject.Find it every timew

    public MusicManager musicManager;
    public ScreenFader blackFade;

    const string SETTINGS_FILENAME = "settings.cfg";

    Configuration config;

    void Awake() {
        // Singleton stuff, needs to carry across multiple scenes
        if (instance != null && instance != this)
            Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(gameObject);

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        // Read config file -- if none exists, make one
        try {
            config = Configuration.LoadFromFile(SETTINGS_FILENAME);
            Screen.SetResolution(config["Settings"]["ResolutionX"].IntValue, config["Settings"]["ResolutionY"].IntValue, config["Settings"]["Fullscreen"].BoolValue);
            SetControlsFromConfig(config);
        } catch (FileNotFoundException) {
            Debug.Log("No config file '" + SETTINGS_FILENAME + "' found. Making one with defaults!");
            config = MakeDefaultConfig(true);
            config.SaveToFile(SETTINGS_FILENAME);
        }

        ApplyQualitySettings(config["Settings"]["GFXPreset"].IntValue);
        
        canvas.GetComponent<MenuUIManager>().PlayIntro();
    }

    ///////////////////////
    //  CONFIG/SETTINGS  //
    ///////////////////////

    public void SaveControls(Configuration cfg) {
        cfg["Controls"]["Left"].IntValueArray = new int[] { (int) InputManager.keyLeft, (int) InputManager.keyLeftAlt };
        cfg["Controls"]["Right"].IntValueArray = new int[] { (int) InputManager.keyRight, (int) InputManager.keyRightAlt };
        cfg["Controls"]["Up"].IntValueArray = new int[] { (int) InputManager.keyUp, (int) InputManager.keyUpAlt };
        cfg["Controls"]["Down"].IntValueArray = new int[] { (int) InputManager.keyDown, (int) InputManager.keyDownAlt };
        cfg["Controls"]["Jump"].IntValueArray = new int[] { (int) InputManager.keyJump, (int) InputManager.keyJumpAlt };
        cfg["Controls"]["Fire1"].IntValueArray = new int[] { (int) InputManager.keyFire1, (int) InputManager.keyFire1Alt };
        cfg["Controls"]["Fire2"].IntValueArray = new int[] { (int) InputManager.keyFire2, (int) InputManager.keyFire2Alt };
        cfg["Controls"]["UseJoystick"].BoolValue = InputManager.useJoystickAxes;
    }

    public void SetControlsFromConfig(Configuration cfg) {
        InputManager.keyLeft = (KeyCode) cfg["Controls"]["Left"].IntValueArray[0];
        InputManager.keyLeftAlt = (KeyCode) cfg["Controls"]["Left"].IntValueArray[1];
        InputManager.keyRight = (KeyCode) cfg["Controls"]["Right"].IntValueArray[0];
        InputManager.keyRightAlt = (KeyCode) cfg["Controls"]["Right"].IntValueArray[1];
        InputManager.keyUp = (KeyCode) cfg["Controls"]["Up"].IntValueArray[0];
        InputManager.keyUpAlt = (KeyCode) cfg["Controls"]["Up"].IntValueArray[1];
        InputManager.keyDown = (KeyCode) cfg["Controls"]["Down"].IntValueArray[0];
        InputManager.keyDownAlt = (KeyCode) cfg["Controls"]["Down"].IntValueArray[1];
        InputManager.keyJump = (KeyCode) cfg["Controls"]["Jump"].IntValueArray[0];
        InputManager.keyJumpAlt = (KeyCode) cfg["Controls"]["Jump"].IntValueArray[1];
        InputManager.keyFire1 = (KeyCode) cfg["Controls"]["Fire1"].IntValueArray[0];
        InputManager.keyFire1Alt = (KeyCode) cfg["Controls"]["Fire1"].IntValueArray[1];
        InputManager.keyFire2 = (KeyCode) cfg["Controls"]["Fire2"].IntValueArray[0];
        InputManager.keyFire2Alt = (KeyCode) cfg["Controls"]["Fire2"].IntValueArray[1];
        InputManager.useJoystickAxes = cfg["Controls"]["UseJoystick"].BoolValue;
    }

    Configuration MakeDefaultConfig(bool resetControls) {
        
        Configuration cfg = new Configuration();
        cfg["Settings"]["ResolutionX"].IntValue = Screen.currentResolution.width;
        cfg["Settings"]["ResolutionY"].IntValue = Screen.currentResolution.height;
        cfg["Settings"]["Fullscreen"].BoolValue = true;
        cfg["Settings"]["FOV"].IntValue = 60;
        cfg["Settings"]["MouseSensitivityX"].FloatValue = 1f;
        cfg["Settings"]["MouseSensitivityY"].FloatValue = 1f;
        cfg["Settings"]["MusicVolume"].FloatValue = 0.5f;
        cfg["Settings"]["SFXVolume"].FloatValue = 0.7f;
        cfg["Settings"]["CrowdVolume"].FloatValue = 0.6f;
        cfg["Settings"]["GFXPreset"].IntValue = QualitySettings.names.Length/2; // Unity has no "recommended settings" functionality, so just go for the median option
        cfg["Settings"]["PlayerColor"].FloatValueArray = new float[] { 1f, 1f, 1f };
        
        if (resetControls) InputManager.SetDefaultKeys();

        
        cfg["Controls"].Comment = "Controls value corresponds to Unity's KeyCode enum value. {Main Key, Alt Key}";
        SaveControls(cfg);

        return cfg;
    }

    void ApplyQualitySettings(int level) {
        QualitySettings.SetQualityLevel(level, true);
    }
}
