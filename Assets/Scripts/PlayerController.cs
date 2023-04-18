using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour {
    private bool ForwardCheck;
    private bool LeftCheck;
    private bool RightCheck;
    private bool BackwardCheck;
    private bool JumpCheck;
    private bool ShiftCheck;
    private bool CtrlCheck;
    public int GrappleSetting = 1;


    GUIController guiController;
    GameNetcodeManager gameNetcodeManager;
    [SerializeField] GameObject avatar;
    LineRenderer lineRenderer;


    public float force; //0.4
    public float airForce; //0.15
    public float sprintSpeedAddition; //0.2


    private Vector3 direction;
    public bool touchingGround;


    private RaycastHit anchorInfo;
    public List<GameObject> rope = new List<GameObject>();
    public List<Vector3> ropeGlobalPositions;


    // Start is called before the first frame update
    void Start() {
        guiController = GetComponent<GUIController>();
        lineRenderer = avatar.GetComponent<AvatarScript>().lineRenderer;
        ropeGlobalPositions = avatar.GetComponent<AvatarScript>().ropeGlobalPositons;
        gameNetcodeManager = GameObject.Find("GameNetcodeManager").GetComponent<GameNetcodeManager>();
    }
    

    [ServerRpc]
    public void UpdateRopeServerRpc(bool enabled, Vector3[] ropePositions, ServerRpcParams serverParams = default) {
        if (enabled) {
            var list = new List<Vector3>();
            list.AddRange(ropePositions);

            gameNetcodeManager.avatars[serverParams.Receive.SenderClientId].GetComponent<AvatarScript>().ropeGlobalPositons = list;
        }
    }
    [ClientRpc]
    public void UpdateRopesClientRpc(bool enabled, ulong[] keys, Vector3[][] ropePositionsGroup) {
        if (enabled) {
            Debug.Log("start");
            Debug.Log(keys.Length);
            Debug.Log(ropePositionsGroup.Length);
            for (int i = 0; i < keys.Length; i++) {
                if (keys[i] == OwnerClientId) { continue; }
                Debug.Log(OwnerClientId);
                Debug.Log(keys[i]);
                Debug.Log(ropePositionsGroup[i]);
                AvatarScript avatar = gameNetcodeManager.avatars[keys[i]].GetComponent<AvatarScript>();

                avatar.ropeGlobalPositons.Clear();
                avatar.ropeGlobalPositons.AddRange(ropePositionsGroup[i]);
                avatar.lineRenderer.enabled = enabled;
                Debug.Log("end" + i);
            }
            Debug.Log("true end");
        }
    }

    // Update is called once per frame
    void Update() {
        Vector3 forwardValue = transform.forward;

        //if (MovementInputCheck = ForwardCheck || LeftCheck || RightCheck || BackwardCheck) {
        direction = Vector3.zero;

        if (ForwardCheck) { direction += forwardValue; }
        if (LeftCheck) { direction += Quaternion.Euler(0, -90, 0) * forwardValue; }
        if (RightCheck) { direction += Quaternion.Euler(0, 90, 0) * forwardValue; }
        if (BackwardCheck) { direction -= forwardValue; }

        if (lineRenderer.enabled) {
            if (GrappleSetting == 2) {
                //rope collision checking
                Physics.Raycast(new Ray(transform.position, rope[^1].transform.position - transform.position), out var hit);


                if (hit.collider.gameObject != null) {
                    if (rope.Count > 1 && hit.collider.gameObject == rope[^1]) {
                        Physics.Raycast(new Ray(transform.position, rope[^2].transform.position - transform.position), out hit);
                        if (hit.collider.gameObject == rope[^2]) {
                            Destroy(rope[^1]);
                            rope.Remove(rope[^1]);
                            ropeGlobalPositions.RemoveAt(ropeGlobalPositions.Count - 1);
                            AttachAvatarToRope();

                        }
                    } else if (hit.collider.gameObject != rope[^1]) {
                        CreateAnchorNode(hit);
                    }
                }
            }
        }

        if (IsOwner) {
            if (IsServer) {
                Dictionary<ulong, GameObject> avatars = gameNetcodeManager.avatars;
                List<Vector3[]> ropePositionsGroup = new List<Vector3[]>();
                List<ulong> keys = new List<ulong>();

                foreach (var key in avatars.Keys) {
                    keys.Add(key);
                    ropePositionsGroup.Add(avatars[key].GetComponent<AvatarScript>().ropeGlobalPositons.ToArray());
                    Debug.Log(avatars[key].GetComponent<AvatarScript>().ropeGlobalPositons.Count);
                }
                Debug.Log(ropePositionsGroup.Count);
                Debug.Log(ropePositionsGroup.ToArray().Length);
                UpdateRopesClientRpc(lineRenderer.enabled, keys.ToArray(), ropePositionsGroup.ToArray());

            } else {
                UpdateRopeServerRpc(lineRenderer.enabled, avatar.GetComponent<AvatarScript>().ropeGlobalPositons.ToArray());
            }
        }

        
    }

    private void FixedUpdate() {
        if (!guiController.gamePaused) {
            if (touchingGround) { // ON GROUND
                //stats
                avatar.GetComponent<Rigidbody>().drag = 1;
                avatar.GetComponent<Rigidbody>().angularDrag = 1;

                //wasd movement
                avatar.GetComponent<Rigidbody>().AddForce(direction.normalized * force, ForceMode.Impulse);

                //jump
                if (JumpCheck) {
                    avatar.GetComponent<Rigidbody>().AddForce(Vector3.up.normalized * 7.5F, ForceMode.Impulse);
                    touchingGround = false;
                    if (lineRenderer.enabled) {
                        JumpCheck = false;
                    }
                }
            } else { // IN AIR
                //stats
                avatar.GetComponent<Rigidbody>().drag = 0.1f;
                avatar.GetComponent<Rigidbody>().angularDrag = 0.1f;

                //wasd movement
                avatar.GetComponent<Rigidbody>().AddForce(direction.normalized * airForce, ForceMode.Impulse);


                //jump | boost
                if (lineRenderer.enabled && JumpCheck) {
                    avatar.GetComponent<Rigidbody>().AddForce((rope[0].transform.position - transform.position).normalized * 7.5F, ForceMode.Impulse);
                    ClearRope();
                }
            }

            if (lineRenderer.enabled) {
                /*
                //rope visuals
                lineRenderer.positionCount = rope.Count + 1;
                for (int i = rope.Count; i > 0; i--) {
                    lineRenderer.SetPosition(i, rope[^i].transform.position - transform.position);
                }
                lineRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                */

                //length adjustment
                ConfigurableJoint avatarJoint = transform.parent.gameObject.GetComponent<ConfigurableJoint>();
                float ropeChangeRate = 0.05f;
                if (ShiftCheck && avatarJoint.linearLimit.limit > 0.01f) {
                    avatarJoint.linearLimit = new SoftJointLimit() { limit = avatarJoint.linearLimit.limit - ropeChangeRate };
                } else if (CtrlCheck) {
                    avatarJoint.linearLimit = new SoftJointLimit() { limit = avatarJoint.linearLimit.limit + ropeChangeRate };
                }
            }

            
        }
    }
    



    

    private void CreateRope() {
        //create anchor
        var anchor = CreateAnchor();
        rope.Add(anchor.transform.Find("AnchorBall").gameObject);
        avatar.GetComponent<AvatarScript>().ropeGlobalPositons.Add(anchor.transform.position);

        //attach avatar to rope
        ConfigurableJoint avatarJoint = transform.parent.gameObject.AddComponent<ConfigurableJoint>();
        avatarJoint.xMotion = ConfigurableJointMotion.Limited;
        avatarJoint.yMotion = ConfigurableJointMotion.Limited;
        avatarJoint.zMotion = ConfigurableJointMotion.Limited;
        avatarJoint.autoConfigureConnectedAnchor = false;
        avatarJoint.connectedAnchor = Vector3.zero;
        avatarJoint.linearLimitSpring = new SoftJointLimitSpring() { spring = 20, damper = 5 };

        AttachAvatarToRope();
        lineRenderer.enabled = true;
    }

    private GameObject CreateAnchor() {
        var anchor = new GameObject("Anchor");

        //make anchor visuals
        anchor.AddComponent<MeshFilter>().mesh = Resources.Load<Mesh>("Meshes/Anchor");
        anchor.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/White");
        anchor.transform.position = anchorInfo.point;
        anchor.transform.rotation = Quaternion.LookRotation(anchorInfo.point - transform.position);
        anchor.transform.localScale = new Vector3(10, 10, 10);


        switch (GrappleSetting) {
            case 1:
                //make anchor joint
                ConfigurableJoint anchorJoint = anchor.AddComponent<ConfigurableJoint>();
                anchorJoint.xMotion = ConfigurableJointMotion.Limited;
                anchorJoint.yMotion = ConfigurableJointMotion.Limited;
                anchorJoint.zMotion = ConfigurableJointMotion.Limited;
                anchorJoint.angularXMotion = ConfigurableJointMotion.Locked;
                anchorJoint.angularYMotion = ConfigurableJointMotion.Locked;
                anchorJoint.angularZMotion = ConfigurableJointMotion.Locked;
                anchorJoint.connectedBody = anchorInfo.rigidbody;
                break;
            case 2:
                //make anchor rigidbody
                Rigidbody anchorRigidbody = anchor.AddComponent<Rigidbody>();
                anchorRigidbody.constraints = RigidbodyConstraints.FreezePosition;
                anchorRigidbody.isKinematic = true;
                break;
        }
        


        //make anchor ball
        var anchorBall = new GameObject("AnchorBall"); 

        //visuals
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anchorBall.AddComponent<MeshFilter>().mesh = Instantiate(obj.GetComponent<MeshFilter>().mesh);
        Destroy(obj);

        anchorBall.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/White");

        //positioning
        anchorBall.transform.parent = anchor.transform;
        anchorBall.transform.localPosition = new Vector3(0, 0, -0.0235f);
        anchorBall.transform.localScale = new Vector3(0.01067802f, 0.01067802f, 0.01067802f);

        //other
        anchorBall.layer = LayerMask.NameToLayer("Rope");
        anchorBall.AddComponent<SphereCollider>();
        
        switch (GrappleSetting) {
            case 1:
                //make anchor joint
                ConfigurableJoint anchorBallJoint = anchorBall.AddComponent<ConfigurableJoint>();
                anchorBallJoint.xMotion = ConfigurableJointMotion.Limited;
                anchorBallJoint.yMotion = ConfigurableJointMotion.Limited;
                anchorBallJoint.zMotion = ConfigurableJointMotion.Limited;
                anchorBallJoint.anchor = Vector3.zero;
                anchorBallJoint.connectedBody = anchor.GetComponent<Rigidbody>();
                break;
            case 2:
                //make anchorball rigidbody
                Rigidbody anchorBallRigidbody = anchorBall.AddComponent<Rigidbody>();
                anchorBallRigidbody.constraints = RigidbodyConstraints.FreezePosition;
                anchorBallRigidbody.isKinematic = true;
                break;
        }

        //return
        return anchor;
    }

    private void CreateAnchorNode(RaycastHit hit) {
        //make anchor node
        var anchorNode = new GameObject("AnchorNode" + rope.Count);

        //visuals
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anchorNode.AddComponent<MeshFilter>().mesh = Instantiate(obj.GetComponent<MeshFilter>().mesh);
        Destroy(obj);

        anchorNode.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/White");

        //positioning
        anchorNode.transform.parent = rope[^1].transform;
        anchorNode.transform.position = hit.point;
        anchorNode.transform.localScale = new Vector3(2, 2, 2);

        //rigidbody
        Rigidbody rigidBody = anchorNode.AddComponent<Rigidbody>();
        rigidBody.constraints = RigidbodyConstraints.FreezePosition;
        rigidBody.isKinematic = true;

        //other
        anchorNode.layer = LayerMask.NameToLayer("Rope");
        anchorNode.AddComponent<SphereCollider>();

        rope.Add(anchorNode);
        avatar.GetComponent<AvatarScript>().ropeGlobalPositons.Add(anchorNode.transform.position);

        AttachAvatarToRope();

    }

    private void AttachAvatarToRope() {
        ConfigurableJoint avatarJoint = transform.parent.gameObject.GetComponent<ConfigurableJoint>();
        avatarJoint.connectedBody = rope[^1].GetComponent<Rigidbody>();
        avatarJoint.linearLimit = new SoftJointLimit() { limit = (rope[^1].transform.position - (transform.position + new Vector3(0, 1.5f, 0))).magnitude };

    }

    private void ClearRope() {
        lineRenderer.enabled = false;


        Destroy(rope[0].transform.parent.gameObject);
        Destroy(transform.parent.gameObject.GetComponent<ConfigurableJoint>());

        rope.Clear();
        avatar.GetComponent<AvatarScript>().ropeGlobalPositons.Clear();

    }



    public void Forward(InputAction.CallbackContext context) {
        ForwardCheck = context.performed;
    }
    public void Left(InputAction.CallbackContext context) {
        LeftCheck = context.performed;
    }
    public void Right(InputAction.CallbackContext context) {
        RightCheck = context.performed;
    }
    public void Backward(InputAction.CallbackContext context) {
        BackwardCheck = context.performed;
    }

    public void Jump(InputAction.CallbackContext context) {
        JumpCheck = context.performed;
    }
    public void Shift(InputAction.CallbackContext context) {
        ShiftCheck = context.performed;

        if (context.performed) {
            force += sprintSpeedAddition;
        }

        if (context.canceled) {
            force -= sprintSpeedAddition;
        }
    }

    public void Ctrl(InputAction.CallbackContext context) {
        CtrlCheck = context.performed;

    }

    public void RMB(InputAction.CallbackContext context) {
        if (context.started) {
            Vector3 aim = Cursor.lockState == CursorLockMode.Locked ? new Vector3(Screen.width / 2, Screen.height / 2) : Input.mousePosition;
            if (Physics.Raycast(transform.parent.GetComponentInChildren<Camera>().ScreenPointToRay(aim), out anchorInfo) && anchorInfo.collider.gameObject != transform.parent.gameObject) {
                if (Physics.Raycast(new Ray(transform.position, anchorInfo.point - transform.position), out anchorInfo)) {
                    CreateRope();
                }
            }
        } else if (context.canceled && lineRenderer.enabled) {
            ClearRope();
        }
    }

    public void One(InputAction.CallbackContext context) {
        GrappleSetting = 1;
    }
    public void Two(InputAction.CallbackContext context) {
        GrappleSetting = 2;
    }
}
