using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GUIController : MonoBehaviour {
    public bool gamePaused;

    // Start is called before the first frame update
    void Start() {
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
