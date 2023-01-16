using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBlockTargetMatch : StateMachineBehaviour
{
    [SerializeField] private float _zOffset = 0.25f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        var pushBlock = player.BaseStates.BlockPush.PushBlock;

        Vector3 targetPosition = pushBlock.transform.position;
        Vector3 axis = pushBlock.GetBestAxis(animator.transform.position);
        targetPosition += axis * (pushBlock.Collider.size.x * 0.5f + _zOffset);

        Quaternion targetRot = Quaternion.LookRotation(-axis);

        MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.forward, 1.0f);
        animator.MatchTarget(targetPosition, targetRot, AvatarTarget.Root, mask, 0.1f, 0.99f);
    }
}
