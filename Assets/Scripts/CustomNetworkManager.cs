using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Networking source code for reference (implementation does not differ between 2017.3 and 2018.1 significantly so it's still useful):
// https://bitbucket.org/Unity-Technologies/networking/src/303d5875befe232c4833bbb31d9c918bdcd3c62e/Runtime/?at=2017.3

public class CustomNetworkManager : NetworkManager {

    public static CustomNetworkManager instance;

    void Awake() {
        instance = this;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        base.OnServerAddPlayer(conn, playerControllerId);
    }
}
