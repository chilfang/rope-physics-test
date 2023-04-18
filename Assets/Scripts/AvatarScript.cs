using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AvatarScript : NetworkBehaviour {
    public PlayerController playerController = null;

    private List<int> ceilingBlockers = new List<int>();
    private List<int> floorBlockers = new List<int>();

    private new Rigidbody rigidbody;

    public List<Vector3> ropeGlobalPositons;
    [SerializeField]
    public LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start() {
        rigidbody = GetComponent<Rigidbody>();
        ropeGlobalPositons = new List<Vector3>();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        GameObject.Find("GameNetcodeManager").GetComponent<GameNetcodeManager>().avatars.Add(OwnerClientId, gameObject);
    }

    private void Update() {
        if (lineRenderer.enabled) {
            lineRenderer.positionCount = ropeGlobalPositons.Count + 1;
            for (int i = ropeGlobalPositons.Count; i > 0; i--) {
                lineRenderer.SetPosition(i, ropeGlobalPositons[^i] - transform.position);
            }
            lineRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void OnCollisionEnter(Collision collision) {
        var normalY = collision.GetContact(0).normal.y;

        if (normalY > 0.45) {
            TouchingGround();
            floorBlockers.Add(collision.gameObject.GetInstanceID());
        } else if (playerController.touchingGround && normalY < 0) {
            NotTouchingGround();
            ceilingBlockers.Add(collision.gameObject.GetInstanceID());
        } 
    }

    public void OnCollisionExit(Collision collision) {
        if (ceilingBlockers.Remove(collision.gameObject.GetInstanceID())) {
            if (ceilingBlockers.Count == 0) {
                TouchingGround();
            }
        } else if (floorBlockers.Remove(collision.gameObject.GetInstanceID())) {
            if (floorBlockers.Count == 0) {
                NotTouchingGround();
            }
        }
    }

    public void TouchingGround() {
        playerController.touchingGround = true;
    }
    public void NotTouchingGround() {
        playerController.touchingGround = false;
    }


}
