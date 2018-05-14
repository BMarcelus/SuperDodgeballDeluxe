using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For passing control schemes around, not really utilized inside of InputManager much
public struct InputKeys {
    public KeyCode left, leftAlt, right, rightAlt, up, upAlt, down, downAlt, jump, jumpAlt, fire1, fire1Alt, fire2, fire2Alt;
    public bool joystick;
}

// Yes we're doing this. Input Manager does not allow for rebindable keys. Put all keys that should be rebound in here.
public static class InputManager {

    public static KeyCode keyLeft, keyLeftAlt, keyRight, keyRightAlt, keyUp, keyUpAlt, keyDown, keyDownAlt;
    public static KeyCode keyJump, keyJumpAlt, keyFire1, keyFire1Alt, keyFire2, keyFire2Alt;

    public static bool useJoystickAxes;

    // Get Left
    public static bool GetLeft() {
        return GetKey(keyLeft) || GetKey(keyLeftAlt) || (useJoystickAxes ? Mathf.Approximately(GetJoyAxisXRaw(), -1f) : false);
    }
    public static bool GetLeftDown() {
        return GetKeyDown(keyLeft) || GetKeyDown(keyLeftAlt);
    }
    public static bool GetLeftUp() {
        return GetKeyUp(keyLeft) || GetKeyUp(keyLeftAlt);
    }

    // Get Right
    public static bool GetRight() {
        return GetKey(keyRight) || GetKey(keyRightAlt) || (useJoystickAxes ? Mathf.Approximately(GetJoyAxisXRaw(), 1f) : false);
    }
    public static bool GetRightDown() {
        return GetKeyDown(keyRight) || GetKeyDown(keyRightAlt);
    }
    public static bool GetRightUp() {
        return GetKeyUp(keyRight) || GetKeyUp(keyRightAlt);
    }

    // Get Up
    public static bool GetUp() {
        return GetKey(keyUp) || GetKey(keyUpAlt) || (useJoystickAxes ? Mathf.Approximately(GetJoyAxisYRaw(), 1f) : false);
    }
    public static bool GetUpDown() {
        return GetKeyDown(keyUp) || GetKeyDown(keyUpAlt);
    }
    public static bool GetUpUp() {
        return GetKeyUp(keyUp) || GetKeyUp(keyUpAlt);
    }

    // AND GET DOWN
    public static bool GetDown() {
        return GetKey(keyDown) || GetKey(keyDownAlt) || (useJoystickAxes ? Mathf.Approximately(GetJoyAxisYRaw(), -1f) : false);
    }
    public static bool GetDownDown() {
        return GetKeyDown(keyDown) || GetKeyDown(keyDownAlt);
    }
    public static bool GetDownUp() {
        return GetKeyUp(keyDown) || GetKeyUp(keyDownAlt);
    }

    // Get Jump
    public static bool GetJump() {
        return GetKey(keyJump) || GetKey(keyJumpAlt);
    }
    public static bool GetJumpDown() {
        return GetKeyDown(keyJump) || GetKeyDown(keyJumpAlt);
    }
    public static bool GetJumpUp() {
        return GetKeyUp(keyJump) || GetKeyUp(keyJumpAlt);
    }

    // Get Fire 1
    public static bool GetFire1() {
        return GetKey(keyFire1) || GetKey(keyFire1Alt);
    }
    public static bool GetFire1Down() {
        return GetKeyDown(keyFire1) || GetKeyDown(keyFire1Alt);
    }
    public static bool GetFire1Up() {
        return GetKeyUp(keyFire1) || GetKeyUp(keyFire1Alt);
    }

    // Get Fire 2
    public static bool GetFire2() {
        return GetKey(keyFire2) || GetKey(keyFire2Alt);
    }
    public static bool GetFire2Down() {
        return GetKeyDown(keyFire2) || GetKeyDown(keyFire2Alt);
    }
    public static bool GetFire2Up() {
        return GetKeyUp(keyFire2) || GetKeyUp(keyFire2Alt);
    }

    // Get horizontal
    public static float GetHorizontal() {
        return (GetLeft() ? -1 : 0) + (GetRight() ? 1 : 0);
    }

    // Get vertical
    public static float GetVertical() {
        return ((GetDown() ? -1 : 0) + (GetUp() ? 1 : 0)) * (useJoystickAxes ? -1 : 1);
    }

    // Joystick axes

    // Unity keeps axis infos locked behind the Input Manager, no way to access and modify it in-game... new input system is taking too long to come out dammit
    public static float GetJoyAxisX() {
        return Input.GetAxis("JoystickX");
    }
    public static float GetJoyAxisXRaw() {
        return Input.GetAxisRaw("JoystickX");
    }
    public static float GetJoyAxisY() {
        return Input.GetAxis("JoystickY");
    }
    public static float GetJoyAxisYRaw() {
        return Input.GetAxisRaw("JoystickY");
    }

    // HELPERS //

    static bool GetKey(KeyCode k) {
        return Input.GetKey(k);
    }

    static bool GetKeyDown(KeyCode k) {
        return Input.GetKeyDown(k);
    }

    static bool GetKeyUp(KeyCode k) {
        return Input.GetKeyUp(k);
    }

    public static InputKeys GetDefaultKeys() {
        InputKeys keys = new InputKeys {
            left = KeyCode.A,
            leftAlt = KeyCode.LeftArrow,
            right = KeyCode.D,
            rightAlt = KeyCode.RightArrow,
            up = KeyCode.W,
            upAlt = KeyCode.UpArrow,
            down = KeyCode.S,
            downAlt = KeyCode.DownArrow,
            jump = KeyCode.Space,
            jumpAlt = KeyCode.None,
            fire1 = KeyCode.Mouse0,
            fire1Alt = KeyCode.LeftControl,
            fire2 = KeyCode.Mouse1,
            fire2Alt = KeyCode.LeftAlt,
            joystick = false
        };
        return keys;
    }

    public static void SetKeys(InputKeys keys) {
        keyLeft = keys.left;
        keyLeftAlt = keys.leftAlt;
        keyRight = keys.right;
        keyRightAlt = keys.rightAlt;
        keyUp = keys.up;
        keyUpAlt = keys.upAlt;
        keyDown = keys.down;
        keyDownAlt = keys.downAlt;
        keyJump = keys.jump;
        keyJumpAlt = keys.jumpAlt;
        keyFire1 = keys.fire1;
        keyFire1Alt = keys.fire1Alt;
        keyFire2 = keys.fire2;
        keyFire2Alt = keys.fire2Alt;
        useJoystickAxes = keys.joystick;
    }
}
