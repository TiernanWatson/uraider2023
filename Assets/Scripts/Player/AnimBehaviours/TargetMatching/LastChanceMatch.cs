using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastChanceMatch : StateMachineBehaviour
{
    private IShimmyable Ledge => _player.BaseStates.Climb.Ledge;

    [SerializeField] private float _startTime;
    [SerializeField] private float _endTime;

    private PlayerController _player;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (_player == null)
        {
            _player = animator.GetComponent<PlayerController>();
        }

        Vector3 testPoint = _player.transform.position;
        float t = Ledge.ClosestParamTo(testPoint, -_player.transform.forward);
        Vector3 point = Ledge.GetPoint(t);

        _targetPosition = _player.Settings.GetLedgePosition(point, Ledge.Forward);
        _targetRotation = Quaternion.LookRotation(Ledge.Forward);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (animator.IsInTransition(0))
        {
            return;
        }

        MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.one, 1.0f);
        animator.MatchTarget(_targetPosition, _targetRotation, AvatarTarget.Root, mask, _startTime, _endTime, true);
    }
}
