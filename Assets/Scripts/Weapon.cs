using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum WeaponType { Rock, Paper, Scissors };

public class Weapon : NetworkBehaviour {
    public bool isHeld;
    public bool isThrown;
    public WeaponType type;
    public GameObject lastHeld;

    [Command]
    void CmdDestroySelf() {
        NetworkServer.Destroy(gameObject);
    }

    void OnCollisionEnter(Collision c) {
        if (isThrown && c.collider.gameObject.layer != LayerMask.NameToLayer("LocalPlayer") && c.collider.gameObject.layer != LayerMask.NameToLayer("OtherPlayer")) {
            isThrown = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<SphereCollider>().isTrigger = true; // Won't be picked up by players until it becomes a trigger again
        }
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.tag == "Bounds") {
            CmdDestroySelf();
        }
    }

    void OnDestroy() {
        
    }
}
