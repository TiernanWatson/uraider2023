using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTargetMatch : StateMachineBehaviour
{
    [SerializeField] private float startTime = 0.45f;
    [SerializeField] private float endTime = 0.85f;

    private PlayerController player;

    Quaternion targetRot;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (player == null)
            player = animator.GetComponent<PlayerController>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (animator.IsInTransition(0))
            return;

        Transform cam = Camera.main.transform;

        Quaternion rotater = Quaternion.Euler(0.0f, cam.eulerAngles.y, 0.0f);

        targetRot = Quaternion.LookRotation(rotater * Vector3.forward);

        animator.MatchTarget(Vector3.zero, targetRot, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.zero, 1.0f), startTime, endTime);
    }
}
