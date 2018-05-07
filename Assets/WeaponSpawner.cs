using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponSpawner : NetworkBehaviour {

	public WeaponType spawnType;
    public float respawnTime;

    public float hoverAmount, hoverSpeed;
    public float yRotateSpeed;

    [SyncVar]
    GameObject attachedWeapon;

    float currentRespawnTimer;

    bool hover;

	void Start () {
		currentRespawnTimer = -1; // Auto spawn on server start immediately
	}

    void OnDrawGizmos() {
        switch(spawnType) {
            case WeaponType.Rock:
                Gizmos.color = Color.red;
                break;
            case WeaponType.Paper:
                Gizmos.color = Color.cyan;
                break;
            case WeaponType.Scissors:
                Gizmos.color = Color.green;
                break;
        }
        
        Gizmos.DrawSphere(transform.position, 0.25f);
    }

	[Command]
    void CmdSpawnWeapon(GameObject prefab) {
        GameObject clone = Instantiate(prefab);
        clone.transform.position = transform.position;
        clone.transform.rotation = transform.rotation;
        NetworkServer.Spawn(clone);
        attachedWeapon = clone;
    }

	void Update () {
        // Handle pickup/respawn stuff
		if (attachedWeapon) {
            if (attachedWeapon.GetComponent<Weapon>().isHeld) {
                attachedWeapon = null;
                currentRespawnTimer = respawnTime;
            }
        } else {
            if (currentRespawnTimer > 0) {
                currentRespawnTimer -= Time.deltaTime;
            } else {
                currentRespawnTimer = 0;
                switch (spawnType) {
                    case WeaponType.Rock:
                        CmdSpawnWeapon(Resources.Load<GameObject>("Weapon/Rock"));
                        break;
                    case WeaponType.Paper:
                        CmdSpawnWeapon(Resources.Load<GameObject>("Weapon/Paper"));
                        break;
                    case WeaponType.Scissors:
                        CmdSpawnWeapon(Resources.Load<GameObject>("Weapon/Scissors"));
                        break;
                }
            }
        }

        // Weapon hovering
        if (attachedWeapon) {
            attachedWeapon.transform.position = new Vector3(attachedWeapon.transform.position.x, transform.position.y + Mathf.Sin(Time.timeSinceLevelLoad*hoverSpeed)*hoverAmount, attachedWeapon.transform.position.z);
            attachedWeapon.transform.Rotate(0, yRotateSpeed*Time.deltaTime, 0);
        }
	}
}
