using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponSpawner : NetworkBehaviour {

	public WeaponType spawnType;
    public float respawnTime;

    public float hoverAmount, hoverSpeed;

    [SyncVar]
    GameObject attachedWeapon;

    float currentRespawnTimer;

    bool hover;

	void Start () {
		currentRespawnTimer = -1; // Auto spawn on server start immediately
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
                        CmdSpawnWeapon(Resources.Load<GameObject>("TestDodgeball"));
                        break;
                    case WeaponType.Paper:
                        break;
                    case WeaponType.Scissors:
                        break;
                }
            }
        }

        // Weapon hovering
        if (attachedWeapon) {
            attachedWeapon.transform.position = new Vector3(attachedWeapon.transform.position.x, transform.position.y + Mathf.Sin(Time.timeSinceLevelLoad*hoverSpeed)*hoverAmount, attachedWeapon.transform.position.z);
        }
	}
}
