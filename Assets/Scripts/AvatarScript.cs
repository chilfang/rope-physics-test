using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AvatarScript : NetworkBehaviour {
    [SerializeField]
    public PlayerController playerController = null;
    public IKController ikController;

    private List<int> ceilingBlockers = new List<int>();
    private List<int> floorBlockers = new List<int>();

    private new Rigidbody rigidbody;

    public List<Vector3> ropeGlobalPositions;
    [SerializeField]
    public LineRenderer lineRenderer;
    [SerializeField]
    GameObject modelPivot;
    ItemEquiper itemEquiper;

    protected Animator animator;

    Vector3 upAirDirection;

    // Start is called before the first frame update
    void Start() {
        animator = gameObject.GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        ropeGlobalPositions = new List<Vector3>();
        ikController = GetComponent<IKController>();
        itemEquiper = GetComponent<ItemEquiper>();
        Application.onBeforeRender += UpdateLineRenderer;
        upAirDirection = Vector3.up;
    }

    public override void OnNetworkSpawn() {
        GameObject.Find("GameNetcodeManager").GetComponent<GameNetcodeManager>().avatars.Add(OwnerClientId, gameObject);
        Debug.Log("spawn: " + OwnerClientId);

        base.OnNetworkSpawn();
    }

    public override void OnDestroy() {
        Application.onBeforeRender -= UpdateLineRenderer;

        if (OwnerClientId == 0) { Debug.Log("despawn: 0\nGame Closing"); return; }
        GameObject.Find("GameNetcodeManager").GetComponent<GameNetcodeManager>().avatars.Remove(OwnerClientId);
        Debug.Log("despawn: " + OwnerClientId);

        base.OnDestroy();
    }

    private void Update() {
        if (playerController.grappleShootObject == null && itemEquiper.itemsEquiped.Contains("GrappleShooter")) {
                playerController.grappleShootObject = itemEquiper.grappleShooter.transform.GetChild(0).GetChild(0).gameObject;
        }

        if (!playerController.touchingGround) {
            var direction = rigidbody.velocity;

            if (Mathf.Abs(direction.x) < 1.5 && Mathf.Abs(direction.z) < 1.5) {
                direction.y = 0;
            }

            if (lineRenderer.enabled) {
                upAirDirection = ropeGlobalPositions[0] - modelPivot.transform.position;
            }
            
            if (direction.magnitude == 0) {
                direction = transform.forward;

                if (direction.magnitude == 0) {
                    direction = transform.root.forward;

                    if (direction.magnitude == 0) {
                        direction = Vector3.forward;
                    }
                }
            }
            
            modelPivot.transform.rotation = Quaternion.LookRotation(direction, upAirDirection); //, ropeGlobalPositions[0] - model.transform.position
                
            
        } else if (modelPivot.transform.rotation != transform.rotation) {
            modelPivot.transform.rotation = transform.rotation;
            upAirDirection = Vector3.up;
        } else {
            upAirDirection = Vector3.up;
        }
    }

    private void FixedUpdate() {
        animator.SetFloat("Speed", (float) System.Math.Round(rigidbody.velocity.magnitude, 2));
        animator.SetBool("InAir", !playerController.touchingGround);
    }

    //rope visuals
    private void UpdateLineRenderer() {
        if (lineRenderer.enabled) {
            lineRenderer.positionCount = ropeGlobalPositions.Count + 1;
            lineRenderer.SetPosition(0, playerController.grappleShootObject.transform.position);
            for (int i = ropeGlobalPositions.Count; i > 0; i--) {
                lineRenderer.SetPosition(i, ropeGlobalPositions[^i]);
            }
            lineRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void OnCollisionEnter(Collision collision) {
        var normalY = collision.GetContact(0).normal.y;

        if (OwnerClientId == 1) {
            Debug.Log("test");
        }

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
        //ikController.rightHand = true;
        //animator.SetBool("InAir", false);
        //animator.Play("Movement");
    }
    public void NotTouchingGround() {
        playerController.touchingGround = false;
        //ikController.rightHand = false;
        //animator.SetBool("InAir", true);
        //animator.Play("Armature|TPose");
    }


}
