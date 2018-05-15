using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class PlayerController : NetworkBehaviour {

    public Camera camera;

    [SyncVar]
    public GameObject currentWeaponObject;

    public GameObject visor;
    public GameObject serverHandPosition;
    public GameObject clientHandPosition;
    public GameObject cameraPosition;

    public float playerSpeed;
    public float jumpForce;

    float camRotationX;

    Rigidbody rb;

    bool isDying;

    MenuUIManager uiManager;
    GameManager gm;

	void Start () {
		rb = GetComponent<Rigidbody>();
        gm = GameManager.instance;
	}
	
	void Update () {
        if (!isLocalPlayer)
            return;

        //if (!Application.isEditor) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        //}

		if (InputManager.GetFire1Down()) {
            // Raycast to weapon spawn pos, ignoring self. If no hit, throw weapon, else don't (prevents throwing inside a wall and losing it)
            RaycastHit hit;
            Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 1);
            if (!hit.collider && currentWeaponObject) {
                Vector3 pos = camera.transform.position + camera.transform.forward;
                Vector3 force = camera.transform.forward * 1000;
                CmdFireWeapon(gameObject, pos, force);
            }
        }

        if (!isDying) {
            transform.Rotate(0, Input.GetAxis("Mouse X") * gm.mouseSensitivity.x, 0);

            if (camera) {
                // Not as easy as just .Rotate() if we wish to clamp
                camRotationX += Input.GetAxis("Mouse Y") * gm.mouseSensitivity.y;
                camRotationX = Mathf.Clamp(camRotationX, -90, 90);
                camera.transform.localEulerAngles = new Vector3(-camRotationX, camera.transform.localEulerAngles.y, camera.transform.localEulerAngles.z);
            }

            float forceX = InputManager.GetHorizontal() * transform.forward.z + InputManager.GetVertical() * transform.forward.x;
            float forceZ = -InputManager.GetHorizontal() * transform.forward.x + InputManager.GetVertical() * transform.forward.z;
            float forceY = InputManager.GetJumpDown() ? jumpForce : 0;
        
            rb.velocity = new Vector3(forceX * playerSpeed, rb.velocity.y + forceY, forceZ * playerSpeed);
        }
	}

    [Command]
    void CmdDestroyObject(GameObject obj) {
        NetworkServer.Destroy(obj);
    }

    [Command]
    void CmdFireWeapon(GameObject player, Vector3 pos, Vector3 force) {
        RpcFireWeapon(player, pos, force);
    }

    [ClientRpc]
    void RpcFireWeapon(GameObject player, Vector3 pos, Vector3 force) {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (!pc.currentWeaponObject) {
            Debug.LogError("CmdFireWeapon should not have been thrown (currentWeaponObject is null)!");
            return;
        }

        GameObject thrown = pc.currentWeaponObject;
        pc.currentWeaponObject = null;

        thrown.transform.SetParent(null);
        thrown.GetComponent<Rigidbody>().isKinematic = false;
        thrown.transform.position = pos;
        thrown.GetComponent<Rigidbody>().AddForce(force);
        thrown.GetComponent<SphereCollider>().isTrigger = false;
        thrown.GetComponent<Weapon>().isHeld = false;
        thrown.GetComponent<Weapon>().isThrown = true;
        thrown.GetComponent<Weapon>().lastHeld = player;

        if (thrown.GetComponent<Weapon>().type == WeaponType.Rock) {
            thrown.GetComponent<Weapon>().rockRot = new Vector3(Random.Range(-1f,1f), Random.Range(-1f,1f), Random.Range(-1f,1f));
        }
    }

    [Command]
    void CmdPickUpWeapon(GameObject weapon, GameObject player) {
        weapon.GetComponent<Rigidbody>().isKinematic = true;
        weapon.GetComponent<Weapon>().isHeld = true;

        player.GetComponent<PlayerController>().currentWeaponObject = weapon;
        RpcPickUpWeapon(weapon, player);
    }

    [ClientRpc]
    void RpcPickUpWeapon(GameObject weapon, GameObject player) {
        PlayerController pc = player.GetComponent<PlayerController>();

        if (isLocalPlayer) {
            weapon.transform.SetParent(player.GetComponent<PlayerController>().camera.transform);
            weapon.transform.localPosition = pc.clientHandPosition.transform.localPosition;
            weapon.transform.localRotation = Quaternion.identity;
        } else {
            weapon.transform.SetParent(player.transform);
            weapon.transform.localPosition = pc.serverHandPosition.transform.localPosition;
            weapon.transform.localRotation = Quaternion.identity;
        }

        //Gets the PickUpCylinder of the player picking up the item and Plays the Pick-Up Sound stored there
        player.transform.Find("PickupItemCylinder").GetComponent<AudioSource>().Play();
    }

    public override void OnStartLocalPlayer() {
        gm = GameManager.instance;
        uiManager = gm.canvas.GetComponent<MenuUIManager>();
        uiManager.HideThingsOnJoined();

        foreach (PlayerController player in FindObjectsOfType<PlayerController>()) {
            if (player.gameObject != gameObject) {
                player.gameObject.layer = LayerMask.NameToLayer("OtherPlayer");
            }
        }
        gm.musicManager.StopMusic();
        gm.blackFade.ClearColor();
        // Nab and set up the main camera (on client-side there will only ever be one camera, having cameras on player prefabs becomes an issue)
        camera = Camera.main;
        camera.transform.SetParent(transform);
        camera.GetComponent<CameraCommander>().DoSomeShake(false);
        camera.GetComponent<CameraCommander>().DoRotate(false);;
        camera.transform.localPosition = cameraPosition.transform.localPosition;
        camera.transform.localRotation = Quaternion.identity; // Camera rotation wouldn't line up with the player otherwise

        // To get things to look up/down with the camera on client, picked up weapons are children of the camera instead of player
        clientHandPosition.transform.SetParent(camera.transform);

        // Hide player stuff on client
        GetComponent<MeshRenderer>().enabled = false;
        visor.GetComponent<MeshRenderer>().enabled = false;
    }

    void OnTriggerEnter(Collider collider) {
        if (!isLocalPlayer)
            return;

        // Touched weapon that isn't being already held
        Weapon weapon = collider.GetComponent<Weapon>();
        if (collider.tag == "Weapon" && weapon && !weapon.isHeld && !isDying) {
            if (weapon.isThrown) {
                // Got hit -- ignore if from self, kill otherwise
                if (weapon.lastHeld && weapon.lastHeld != gameObject) {
                    Debug.LogError("PLAYER " + name + " WAS KNOCKED OUT BY " + weapon.lastHeld.name + "!");
                    RespawnSelf();
                }
                //RespawnSelf();
            } else {
                if (!currentWeaponObject) {
                    // Performs pickup on server
                    CmdPickUpWeapon(collider.gameObject, gameObject);
                }
            }
        } else if (collider.tag == "Weapon" && !collider.GetComponent<Weapon>()) {
            Debug.LogError("Object tagged as weapon does not have a weapon component attached! FIX THIS OR SUFFER MY UNDYING WRATH!");
        }

        // Out of bounds
        if (collider.tag == "Bounds" && !isDying) {
            // For now, there is no kill state - just bring the player back
            RespawnSelf();
        }
    }

    void OnDestroy() {
        camera.transform.SetParent(null);
        camera.transform.position = camera.GetComponent<CameraCommander>().originalPos;
        camera.transform.rotation = Quaternion.identity;
        camera.GetComponent<CameraCommander>().DoSomeShake(true);
    }

    async void RespawnSelf() {
        
        isDying = true;

        camera.transform.SetParent(null);
        camera.GetComponent<CameraCommander>().StareAtObject(gameObject);
        camera.GetComponent<CameraCommander>().DoSomeShake(true);
        GetComponent<MeshRenderer>().enabled = true;
        visor.GetComponent<MeshRenderer>().enabled = true;
        
        if (currentWeaponObject) {
            currentWeaponObject.transform.SetParent(gameObject.transform);
            currentWeaponObject.transform.localPosition = serverHandPosition.transform.localPosition;
        }

        await Task.Delay(System.TimeSpan.FromSeconds(1));
        
        isDying = false;

        CmdDestroyObject(currentWeaponObject);
        currentWeaponObject = null;

        camera.transform.SetParent(transform);
        camera.GetComponent<CameraCommander>().StareAtObject(null);
        camera.GetComponent<CameraCommander>().DoSomeShake(false);
        camera.transform.localPosition = cameraPosition.transform.localPosition;
        camera.transform.localRotation = Quaternion.identity;
        GetComponent<MeshRenderer>().enabled = false;
        visor.GetComponent<MeshRenderer>().enabled = false;

        Transform respawnSpot = CustomNetworkManager.instance.startPositions[Random.Range(0, CustomNetworkManager.instance.startPositions.Count)];
        transform.position = respawnSpot.position;
        rb.velocity = Vector3.zero;
        transform.rotation = respawnSpot.rotation;
    }
}
