using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class NetworkUIManager : MonoBehaviour {

    public CustomNetworkManager networkManager;

    public GameObject mainMenuPanel;
    public GameObject lanConnectingPanel;
    public GameObject netMenuPanel;
    public GameObject netConnectingPanel;
    public GameObject errorPanel;
    public GameObject creditsPanel;
    public Button lanToNetButton, netToLanButton;
    public InputField mainMenuIPInputField;
    public GameObject netScrollViewContent;
    public Button netJoinButton, netHostButton;
    public InputField netMatchNameInputField;
    public Text netMatchListErrorText;
    public Button netMatchRefreshButton;
    public GameObject netMatchPanelPrefab;

    MatchInfoSnapshot currentSelectedMatchSnapshot;

	void Start () {
        // This is muy importante
		if (networkManager == null) {
            Debug.LogError("Network Manager was not properly set up! Doing GO.Find to locate as backup method...");
            GameObject nm = GameObject.Find("NetworkManager");
            if (nm == null) {
                nm = GameObject.Find("Network Manager");
                if (nm == null) {
                    Debug.Log("Yep, couldn't find it. Fix this.");
                } else {
                    networkManager = nm.GetComponent<CustomNetworkManager>();
                    Debug.Log("Found Network Manager.");
                }
            } else {
                networkManager = nm.GetComponent<CustomNetworkManager>();
                Debug.Log("Found Network Manager.");
            }
        }
	}
	
    public async void ShowErrorOnMenu(string error, float time) {
        if (mainMenuPanel.activeSelf || netMenuPanel.activeSelf) {
            errorPanel.transform.GetChild(0).GetComponent<Text>().text = error;
            errorPanel.SetActive(true);

            await Task.Delay(TimeSpan.FromSeconds(time));

            errorPanel.SetActive(false);
            errorPanel.transform.GetChild(0).GetComponent<Text>().text = "";
        }
    }

    /////////////////////
    // Credits Buttons //
    /////////////////////

    public void CreditsButtonPressed() {
        mainMenuPanel.SetActive(false);
        lanToNetButton.gameObject.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CreditsBackButtonPressed() {
        mainMenuPanel.SetActive(true);
        lanToNetButton.gameObject.SetActive(true);
        creditsPanel.SetActive(false);
    }
    
    ////////////////////////
    // LAN (Local) Events //
    ////////////////////////

    public void LanHostButtonPressed() {
        if (mainMenuIPInputField.text.Length == 0) {
            networkManager.networkAddress = "localhost";
        } else {
            networkManager.networkAddress = mainMenuIPInputField.text;
        }
        networkManager.StartHost();

        mainMenuPanel.SetActive(false);
        lanToNetButton.gameObject.SetActive(false);
    }

    public void LanJoinButtonPressed() {
        if (mainMenuIPInputField.text.Length == 0) {
            networkManager.networkAddress = "localhost";
        } else {
            networkManager.networkAddress = mainMenuIPInputField.text;
        }
        NetworkClient client = networkManager.StartClient();

        mainMenuPanel.SetActive(false);
        lanToNetButton.gameObject.SetActive(false);
        lanConnectingPanel.SetActive(true);
        StartCoroutine(HideLanConnectingPanel(client));
    }

    public void LanCancelPressed() {
        networkManager.StopClient();
        
        lanConnectingPanel.SetActive(false);
        lanToNetButton.gameObject.SetActive(true);
        mainMenuPanel.SetActive(true);
    }

    public void SwitchToLanButtonPressed() {
        networkManager.StopMatchMaker();

        foreach (Transform childTransform in netScrollViewContent.transform) {
            Destroy(childTransform.gameObject);
        }

        lanToNetButton.gameObject.SetActive(true);
        netToLanButton.gameObject.SetActive(false);
        mainMenuPanel.SetActive(true);
        netMenuPanel.SetActive(false);
        netMatchListErrorText.gameObject.SetActive(false);
    }

    public void DisconnectBackToLANMenu() {
        // Something happened, so force back to main menu
        networkManager.StopMatchMaker();

        netMenuPanel.SetActive(false);
        lanConnectingPanel.SetActive(false);
        netConnectingPanel.SetActive(false);
        netToLanButton.gameObject.SetActive(false);
        lanToNetButton.gameObject.SetActive(true);
        mainMenuPanel.SetActive(true);
        netMatchListErrorText.gameObject.SetActive(false);
    }

    IEnumerator HideLanConnectingPanel(NetworkClient client) {
        // There's no callback for a successful LAN client connection that I found, so this'll have to do
        yield return new WaitUntil(delegate {
            return client.isConnected;
        });
        lanConnectingPanel.SetActive(false);
    }

    //////////////////////////////
    // Net (Matchmaking) Events //
    //////////////////////////////

    public void NetHostButtonPressed() {
        networkManager.matchMaker.CreateMatch(netMatchNameInputField.text == "" ? networkManager.matchName : netMatchNameInputField.text, networkManager.matchSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        netHostButton.interactable = false;
    }

    public void NetJoinButtonPressed() {
        networkManager.matchMaker.JoinMatch(currentSelectedMatchSnapshot.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        netMenuPanel.SetActive(false);
        netToLanButton.gameObject.SetActive(false);
        netConnectingPanel.SetActive(true);
    }

    public void NetCancelPressed() {
        networkManager.matchMaker.DropConnection(currentSelectedMatchSnapshot.networkId, 0, 0, null);
        NetRefreshButtonPressed();

        netConnectingPanel.SetActive(false);
        netToLanButton.gameObject.SetActive(true);
        netMenuPanel.SetActive(true);
    }

    public void SwitchToNetButtonPressed() {
        networkManager.StartMatchMaker();
        
        networkManager.matchMaker.ListMatches(0, 32, "", false, 0, 0, OnMatchList);
        netJoinButton.interactable = false;

        lanToNetButton.gameObject.SetActive(false);
        netToLanButton.gameObject.SetActive(true);
        mainMenuPanel.SetActive(false);
        netMenuPanel.SetActive(true);
    }

    public void NetRefreshButtonPressed() {
        networkManager.matchMaker.ListMatches(0, 32, "", false, 0, 0, OnMatchList);
        netJoinButton.interactable = false;
        currentSelectedMatchSnapshot = null;
        netMatchListErrorText.gameObject.SetActive(false);
        netMatchRefreshButton.interactable = false;
    }

    ////////////////////////////////////
    // Match List Delegates/Callbacks //
    ////////////////////////////////////

    void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {
        netMatchRefreshButton.interactable = true;
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
}
