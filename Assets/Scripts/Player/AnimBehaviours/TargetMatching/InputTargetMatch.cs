using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTargetMatch : StateMachineBehaviour
{
    [SerializeField] private bool canReadjust = true;
    [SerializeField] private float startTime = 0.45f;
    [SerializeField] private float endTime = 0.85f;

    private bool fired;
    private PlayerController player;
    private Quaternion targetRot;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        fired = false;

        if (player == null)
        {
            player = animator.GetComponent<PlayerController>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        /*if (animator.IsInTransition(0))
        {
            return;
        }*/

        float normalizedTime = Mathf.Repeat(animatorStateInfo.normalizedTime, 1.0f);

        if (!fired && normalizedTime > startTime && player.UInput.MoveInputRaw.sqrMagnitude > 0.01f)
        {
            if (!canReadjust)
            {
                fired = true;
            }

            /*Quaternion rotater = Quaternion.Euler(0.0f, player.Camera.eulerAngles.y, 0.0f);
            Vector3 targetDirection = rotater * player.UInput.MoveInputRaw;
            targetRot = Quaternion.LookRotation(targetDirection);*/
            targetRot = player.Movement.TargetRotation;

            animator.InterruptMatchTarget(false);
            animator.MatchTarget(Vector3.zero, targetRot, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.zero, 1.0f), normalizedTime + 0.01f, endTime, false);

            //animator.MatchTarget(Vector3.zero, targetRot, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.zero, 1.0f), startTime, endTime, false);
        }
    }
}
