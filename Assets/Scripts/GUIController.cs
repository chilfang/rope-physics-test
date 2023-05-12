using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GUIController : MonoBehaviour {
    public bool gamePaused;

    // Start is called before the first frame update
    void Start() {
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 300));
        GUILayout.Label("Ian Marler's Swinging Demo");
        GUILayout.Label("1 and 2 to switch between rope physics");
        GUILayout.Label("WASD to move, hold Shift to sprint");
        GUILayout.Label("hold Right Click to grapple");
        GUILayout.Label("click Alt to toggle mouse lock");
        GUILayout.Label("space to jump or boost while swinging");
        GUILayout.EndArea();
    }

    public void ToggleCursor(InputAction.CallbackContext context) {
        if (!gamePaused) {
            if (context.performed) {
                if (Cursor.lockState == CursorLockMode.Locked) {
                    Cursor.lockState = CursorLockMode.None;

                } else {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    public void OnApplicationFocus(bool hasFocus) {
        gamePaused = !hasFocus;
    }
}
