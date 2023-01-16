using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DPipeOffTarget : StateMachineBehaviour
{
    [SerializeField] private float _startTime;
    [SerializeField] private float _endTime;

    private PlayerController _player;
    private Vector3 _targetPosition;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerController>();
        }

        _targetPosition = _player.BaseStates.Drainpipe.Pipe.transform.position;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (animator.IsInTransition(0))
        {
            return;
        }

        MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.up, 0.0f);
        animator.MatchTarget(_targetPosition, Quaternion.identity, AvatarTarget.Root, mask, _startTime, _endTime, true);
    }
}
