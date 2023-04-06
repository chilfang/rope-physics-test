using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    GUIController guiController;
    GameObject avatar;
    Transform cameraPivot;

    public float horizontalSensitivity;
    public float verticalSensitivity;
    public bool invertVertical;


    private void Start() {
        guiController = GetComponent<GUIController>();
        avatar = transform.parent.gameObject;
        cameraPivot = transform.parent.Find("CameraPivot");
    }

    public void MousePosition(InputAction.CallbackContext context) {
        if (!guiController.gamePaused && (Cursor.lockState == CursorLockMode.Locked)) {
            Vector2 mouseDelta = context.ReadValue<Vector2>();
            
            //horizontal
            avatar.GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(avatar.GetComponent<Rigidbody>().rotation.eulerAngles + new Vector3(
                0, 
                mouseDelta.x * horizontalSensitivity
            )));
            
            
            //vertical
            cameraPivot.localRotation = Quaternion.Euler(cameraPivot.localRotation.eulerAngles + new Vector3(
                mouseDelta.y * verticalSensitivity * (invertVertical ? -1 : 1), 
                0
            ));

            if (cameraPivot.localRotation.eulerAngles.z > 0) {
                cameraPivot.localRotation = Quaternion.Euler(cameraPivot.localRotation.eulerAngles.x, 0, 0);
            }
            


        }
    }
}
