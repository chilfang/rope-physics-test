using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class IKController : MonoBehaviour {

    protected Animator animator;

    public bool ikActive = false;
    public Transform rightHandObj = null;
    public Transform lookObj = null;

    public bool head = true;
    public bool rightHand = true;
    public bool feet = true;

    public int level;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
    }

    //a callback for calculating IK
    void OnAnimatorIK() {
        if (animator) {
            //if the IK is active, set the position and rotation directly to the goal.
            if (ikActive) {
                // Set the look target position, if one has been assigned
                if (lookObj != null && head) {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(lookObj.position);
                }
                

                // Set the right hand target position and rotation, if one has been assigned
                if (rightHandObj != null && rightHand) {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                    //animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
                }

                if (feet) {
                    //left foot
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);

                    Ray ray = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + (Vector3.up * 1), Vector3.down);
                    if (Physics.Raycast(ray, out var hit, 1f)) {
                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point);

                        Vector3 rotAxis = Vector3.Cross(Vector3.up, hit.normal);
                        float angle = Vector3.Angle(Vector3.up, hit.normal);
                        Quaternion rot = Quaternion.AngleAxis(angle * 1, rotAxis);
                        var feetIKRotation = rot;

                        animator.SetIKRotation(AvatarIKGoal.LeftFoot, feetIKRotation * animator.GetIKRotation(AvatarIKGoal.LeftFoot));
                    }


                    //right foot
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

                    ray = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot) + (Vector3.up * 1), Vector3.down);
                    if (Physics.Raycast(ray, out hit, 1f)) {
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point);

                        Vector3 rotAxis = Vector3.Cross(Vector3.up, hit.normal);
                        float angle = Vector3.Angle(Vector3.up, hit.normal);
                        Quaternion rot = Quaternion.AngleAxis(angle * 1, rotAxis);
                        var feetIKRotation = rot;

                        animator.SetIKRotation(AvatarIKGoal.RightFoot, feetIKRotation * animator.GetIKRotation(AvatarIKGoal.RightFoot));
                    }
                }

            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetLookAtWeight(0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            }
        }
    }
}

