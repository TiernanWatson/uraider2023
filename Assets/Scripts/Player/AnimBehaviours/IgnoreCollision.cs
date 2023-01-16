using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : StateMachineBehaviour
{
    [SerializeField] private bool _forceGrounded = false;

    private bool _wasUsingGf;
    private PlayerController _player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _player = animator.GetComponent<PlayerController>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        // Ignoring while in transition can cause character to clip walls
        if (!animator.IsInTransition(0))
        {
            if (_forceGrounded)
            {
                _player.Movement.Motor.OverrideGrounding(true);
                _wasUsingGf = _player.Movement.Motor.UseGroundingForce;
                _player.Movement.Motor.UseGroundingForce = false;
            }

            Physics.IgnoreLayerCollision(8, 0);
            Physics.IgnoreLayerCollision(8, 9);
            Physics.IgnoreLayerCollision(8, 11);
        }
        else
        {
            Physics.IgnoreLayerCollision(8, 0, false);
            Physics.IgnoreLayerCollision(8, 9, false);
            Physics.IgnoreLayerCollision(8, 11, false);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (_forceGrounded)
        {
            _player.Movement.Motor.OverrideGrounding(false);
            _player.Movement.Motor.UseGroundingForce = true;
        }

        Physics.IgnoreLayerCollision(8, 0, false);
        Physics.IgnoreLayerCollision(8, 9, false);
        Physics.IgnoreLayerCollision(8, 11, false);
    }
}
