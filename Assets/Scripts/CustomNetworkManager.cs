using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

// Networking source code for reference (implementation does not differ between 2017.3 and 2018.1 significantly so it's still useful):
// https://bitbucket.org/Unity-Technologies/networking/src/303d5875befe232c4833bbb31d9c918bdcd3c62e/Runtime/?at=2017.3

// Reference order of operations:
// after StartHost gets called, the following events occur:
// -> OnStartHost
// -> StartServer
//   -> OnStartServer
//   -> If onlineScene exists, switch to it
// -> ConnectLocalClient
// -> OnStartClient

public class CustomNetworkManager : NetworkManager {

    public static CustomNetworkManager instance;

    public NetworkConnection connToServer;

    void Awake() {
        // Singleton stuff, needs to carry across multiple scenes
        if(instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
    }

    public override void OnStartHost() {
        base.OnStartHost();
        // Before rest of host code runs, set up the onlineScene and some flags
        GameManager.instance.PickRandomLevelAndPlay();
    }

    public override void OnClientSceneChanged(NetworkConnection conn) {
        //base.OnClientSceneChanged(conn); // Keep this commented
    }

    public override void OnClientConnect(NetworkConnection conn) {
        //base.OnClientConnect(conn); // Keep this commented
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo) {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        connToServer = client.connection;
        GameManager.instance.PickRandomLevelAndPlay();
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo) {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        GameManager.instance.StartGameClient(client);
    }

}
