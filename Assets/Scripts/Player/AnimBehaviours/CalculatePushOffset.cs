using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculatePushOffset : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();

        if (player.StateMachine.State == player.BaseStates.BlockPush)
        {
            var pushBlock = player.BaseStates.BlockPush.PushBlock;
            var rightHand = player.RightHand;

            Vector3 handToBlock = pushBlock.transform.position - rightHand.position;
            player.BaseStates.BlockPush.BlockOffset = handToBlock;
        }
    }
}
