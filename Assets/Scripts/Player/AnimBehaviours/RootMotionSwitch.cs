using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionSwitch : StateMachineBehaviour
{
    [SerializeField] private bool enterRotationState = true;
    [SerializeField] private bool exitRotationState = false;
    [SerializeField] private bool setRotationOnExit = true;

    private PlayerMovement _player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (_player == null)
            _player = animator.GetComponent<PlayerMovement>();

        _player.RootMotionRotate = enterRotationState;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        _player.RootMotionRotate = enterRotationState;
        _player.Rotation = _player.transform.rotation;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _player.RootMotionRotate = exitRotationState;

        if (setRotationOnExit)
        {
            _player.SetRotation(_player.transform.rotation);
        }
    }
}
