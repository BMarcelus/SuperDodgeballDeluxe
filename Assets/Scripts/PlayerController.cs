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
    float clientHandMinZ = 0.7f, clientHandMaxZ = 1.1f;

    public float playerSpeed;
    public float jumpForce;

    public AudioSource runSound;
    public AudioClip[] throwSoundList;
    public AudioSource throwSound;
    public AudioSource jumpSound;

    float camRotationX;

    float curPower = 0;
    float fovAdd = 10;

    Rigidbody rb;

    bool isDying;

    MenuUIManager uiManager;
    GameManager gm;

	void Start () {
		rb = GetComponent<Rigidbody>();
        gm = GameManager.instance;
        throwSound = transform.Find("ThrowSounds").GetComponent<AudioSource>();
    }
	
	void Update () {
        if (!isLocalPlayer)
            return;

        //if (!Application.isEditor) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        //}

        if(InputManager.GetFire1() && currentWeaponObject)
        {
            if (curPower < 1000)
                curPower += 25;
            camera.fieldOfView = Mathf.Lerp(gm.cameraFov, gm.cameraFov + fovAdd, curPower / 1000f);
            if(!throwSound.isPlaying && curPower < 1000)
            {
                CmdPlayerWindUpSound(true);
            }
            else if(curPower >= 1000)
                CmdPlayerWindUpSound(false);
        }
		else if (InputManager.GetFire1Up() && currentWeaponObject) {
            // Raycast to weapon spawn pos, ignoring self. If no hit, throw weapon, else don't (prevents throwing inside a wall and losing it)
            RaycastHit hit;
            Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 1);
            if (!hit.collider) {
                Vector3 pos = camera.transform.position + camera.transform.forward;
                Vector3 force = camera.transform.forward * curPower;
                CmdFireWeapon(gameObject, pos, force);
            }
            else
            {
                //Should just drop it if you're facing a wall
                //Vector3 pos = camera.transform.position;
                //Vector3 force = Vector3.zero;
                //CmdFireWeapon(gameObject, pos, force);
            }
            curPower = 0;
            camera.fieldOfView = gm.cameraFov;
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
            float forceY = 0;
            if(InputManager.GetJumpDown())
            {
                //Check to make sure the player is grouded before allowing them to jump
                RaycastHit[] rhs = Physics.SphereCastAll(
                    new Vector3(transform.position.x, transform.position.y - 0.49f, transform.position.z),
                    0.5f, Vector3.down, 0.02f);

                bool foundGround = false;
                foreach (RaycastHit h in rhs) {
                    if (h.collider.gameObject.layer != LayerMask.NameToLayer("LocalPlayer")) {
                        foundGround = true;
                        break;
                    }
                }
                if (foundGround)
                {
                    forceY = jumpForce;
                    CmdPlayerJumpSound(true);
                }
            }

            if (forceX != 0 || forceZ != 0)
            {
                if (!runSound.isPlaying)
                    CmdPlayerMovementSound(true);
            }
            else
                CmdPlayerMovementSound(false);


            rb.velocity = new Vector3(forceX * playerSpeed, rb.velocity.y + forceY, forceZ * playerSpeed);
        }
	}


    //This is the sound controllers for running, jumping, and wind-up across network
    #region

    [Command]
    void CmdPlayerMovementSound(bool val)
    {
        RpcPlayerMovementSounds(gameObject, val);
    }

    [ClientRpc]
    void RpcPlayerMovementSounds(GameObject player, bool val)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (val)
            pc.runSound.Play();
        else
            pc.runSound.Stop();
    }

    [Command]
    void CmdPlayerJumpSound(bool val)
    {
        RpcPlayerJumpSound(gameObject, val);
    }

    [ClientRpc]
    void RpcPlayerJumpSound(GameObject player, bool val)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (val)
            pc.jumpSound.Play();
        else
            pc.jumpSound.Stop();
    }

    [Command]
    void CmdPlayerWindUpSound(bool val)
    {
        RpcPlayerWindUpSound(gameObject, val);
    }

    [ClientRpc]
    void RpcPlayerWindUpSound(GameObject player, bool val)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (val)
        {
            pc.throwSound.clip = pc.throwSoundList[0];
            pc.throwSound.Play();
        }
        else
            pc.throwSound.Stop();
    }

    #endregion


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
        AudioSource g = player.transform.Find("ThrowSounds").GetComponent<AudioSource>();

        //First sounds is reserved for wind up sound
        g.clip = throwSoundList[Random.Range(1, throwSoundList.Length)];
        g.pitch = Random.Range(0.9f, 1.1f);
        g.Play();

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
            Vector3 clientPos = pc.clientHandPosition.transform.localPosition;
            clientPos = new Vector3(clientPos.x, clientPos.y, Mathf.Lerp(clientHandMinZ, clientHandMaxZ, (gm.cameraFovClamp.y - gm.cameraFov)/(gm.cameraFovClamp.x - gm.cameraFov)));
            weapon.transform.localPosition = clientPos;
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
        //uiManager.HideThingsOnJoined();

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
        //camera.GetComponent<CameraCommander>().DoSomeShake(false);
        //camera.GetComponent<CameraCommander>().DoRotate(false);
        camera.GetComponent<CameraCommander>().enabled = false;
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
        if (camera) {
            camera.transform.SetParent(null);
            camera.transform.position = camera.GetComponent<CameraCommander>().originalPos;
            camera.transform.rotation = Quaternion.identity;
            camera.GetComponent<CameraCommander>().DoSomeShake(true);
        }
        
    }

    async void RespawnSelf() {
        
        isDying = true;

        camera.transform.SetParent(null);
        //camera.GetComponent<CameraCommander>().StareAtObject(gameObject);
        //camera.GetComponent<CameraCommander>().DoSomeShake(true);
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
        //camera.GetComponent<CameraCommander>().StareAtObject(null);
        //camera.GetComponent<CameraCommander>().DoSomeShake(false);
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
