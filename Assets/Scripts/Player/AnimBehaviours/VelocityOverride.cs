using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityOverride : StateMachineBehaviour
{
    private PlayerController _player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _player = animator.GetComponent<PlayerController>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (!animator.IsInTransition(layerIndex))
        {
            _player.Movement.SetVelocity(animator.velocity);
        }
    }
}
