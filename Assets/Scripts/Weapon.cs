﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum WeaponType { Rock, Paper, Scissors, None };

public class Weapon : NetworkBehaviour {
    public bool isHeld;
    public bool isThrown;
    public WeaponType type;
    public GameObject lastHeld;
    public float rockRotSpeed, scissorRotSpeed;
    public Vector3 rockRot; // Set to random by player on throw
    public AudioSource impactSound;

    Coroutine selfDestroyCoroutine;

    void Start()
    {
        impactSound = GetComponent<AudioSource>();
    }

    [Command]
    void CmdDestroySelf() {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    void CmdSetPosition() {
        RpcSetPosition(transform.position); // server's pos
    }

    [ClientRpc]
    void RpcSetPosition(Vector3 pos) {
        transform.position = pos;
    }

    void Update() {
        // Rotation during throwing
        if (isThrown) {
            switch (type) {
                case WeaponType.Rock:
                    transform.Rotate(rockRot * rockRotSpeed * Time.deltaTime);
                    break;
                case WeaponType.Paper:
                    transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity);
                    break;
                case WeaponType.Scissors:
                    transform.Rotate(scissorRotSpeed * Time.deltaTime, 0, 0);
                    break;
            }
        }
    }

    void OnCollisionEnter(Collision c) {
        //If it collides with something that is not a player or weapon
        if (isThrown &&
                c.collider.gameObject.layer != LayerMask.NameToLayer("LocalPlayer") &&
                c.collider.gameObject.layer != LayerMask.NameToLayer("OtherPlayer") &&
                c.collider.gameObject.layer != LayerMask.NameToLayer("Weapon")) {
            lastHeld = null;
            isThrown = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<SphereCollider>().isTrigger = true; // Won't be picked up by players until it becomes a trigger again
            CmdSetPosition(); // Make sure every player has the exact server position when it stops, rotation doesn't really matter
            impactSound.pitch = Random.Range(0.9f, 1.1f);
            impactSound.Play();
            selfDestroyCoroutine = StartCoroutine(DestroyAfterSeconds(15));
        }
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.tag == "Bounds") {
            CmdDestroySelf();
        }
        if ((c.gameObject.tag == "WeaponPickUp" || c.gameObject.tag == "Player") && selfDestroyCoroutine != null) {
            StopCoroutine(selfDestroyCoroutine);
            selfDestroyCoroutine = null;
        }
    }

    IEnumerator DestroyAfterSeconds(float seconds) {
        yield return new WaitForSeconds(seconds);
        CmdDestroySelf();
        selfDestroyCoroutine = null;
    }
}
