using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour {
    GUIController guiController;
    [SerializeField]
    GameObject avatar;
    [SerializeField]
    Transform cameraPivot;

    public float horizontalSensitivity;
    public float verticalSensitivity;
    public bool invertVertical;

    //public NetworkVariable<Quaternion> rotation;

    private void Start() {
        guiController = GetComponent<GUIController>();
    }
    
    [ServerRpc]
    public void SetRotationServerRpc(Quaternion rotation) {
        avatar.GetComponent<Rigidbody>().MoveRotation(rotation);
    }
    

    public void MousePosition(InputAction.CallbackContext context) {
        if (!guiController.gamePaused && (Cursor.lockState == CursorLockMode.Locked)) {
            Vector2 mouseDelta = context.ReadValue<Vector2>();

            //horizontal
            /*
            try {
                rotation.Value = Quaternion.Euler(avatar.GetComponent<Rigidbody>().rotation.eulerAngles + new Vector3(
                    0,
                    mouseDelta.x * horizontalSensitivity
                ));
                avatar.GetComponent<Rigidbody>().MoveRotation(rotation.Value);
                SetRotationServerRpc(mouseDelta);
            } catch (System.Exception e) {
                Debug.Log(e);
            }
            */
            var rotation = Quaternion.Euler(avatar.GetComponent<Rigidbody>().rotation.eulerAngles + new Vector3(
                0,
                mouseDelta.x * horizontalSensitivity
            ));
            avatar.GetComponent<Rigidbody>().MoveRotation(rotation);

            /*
            if (!IsServer) {
                SetRotationServerRpc(rotation);
            }
            */


            //vertical
            cameraPivot.localRotation = Quaternion.Euler(cameraPivot.localRotation.eulerAngles + new Vector3(
                mouseDelta.y * verticalSensitivity * (invertVertical ? -1 : 1), 
                0
            ));

                //limiter
            if (cameraPivot.localRotation.eulerAngles.z > 0) {
                cameraPivot.localRotation = Quaternion.Euler(cameraPivot.localRotation.eulerAngles.x, 0, 0);
            }
            


        }
    }
}
