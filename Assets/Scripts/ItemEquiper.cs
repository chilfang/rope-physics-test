using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEquiper : MonoBehaviour {
    [SerializeField]
    public GameObject grappleShooter;

    Animator animator;

    Transform rightHand;

    public string itemsEquiped = "";

    void Start() {
        animator = gameObject.GetComponentInChildren<Animator>();
        rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

        grappleShooter = Instantiate(grappleShooter);
        grappleShooter.transform.parent = (new GameObject("GrappleShooterHolder")).transform;

        grappleShooter.transform.localRotation = Quaternion.Euler(84.5f, 53, 20.8f); //95
        grappleShooter.transform.localPosition = new Vector3(0.008f, -0.0014f, -0.017f);
        grappleShooter.transform.localScale = new Vector3(21.21f, 21.21f, 31.84f);


        grappleShooter = grappleShooter.transform.parent.gameObject;
        grappleShooter.transform.parent = rightHand.transform;
        grappleShooter.transform.position = rightHand.transform.position;

        itemsEquiped += "GrappleShooter";
    }
    private void Update() {
        /*
        grappleShooter.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal).transform);
        grappleShooter.transform.Rotate(Vector3.up, 135f);
        */
        grappleShooter.transform.rotation = rightHand.rotation;
    }
}
