using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallclimbStartMatch : StateMachineBehaviour
{
    private enum ClimbType
    {
        Wallclimb,
        Ladder,
        Drainpipe
    }

    private Vector3 SurfaceForward => _rungs.transform.forward;

    [SerializeField] private ClimbType _rungType = ClimbType.Wallclimb;
    [SerializeField] private float _startTime;
    [SerializeField] private float _endTime;
    [SerializeField] private float _ladderXOffset = 0.075f;

    private bool _started;
    private PlayerController _player;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private RungMaker _rungs;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _started = false;

        if (_player == null)
        {
            _player = animator.GetComponent<PlayerController>();
        }

        switch (_rungType)
        {
            case ClimbType.Wallclimb:
                _rungs = _player.BaseStates.Wallclimb.Surface.Rungs;
                CalculateWallclimb();
                break;
            case ClimbType.Ladder:
                _rungs = _player.BaseStates.Ladder.Ladder.Rungs;
                CalculateLadder();
                break;
            case ClimbType.Drainpipe:
                _rungs = _player.BaseStates.Drainpipe.Pipe.Rungs;
                CalculateDrainpipe();
                break;
            default:
                Debug.LogError("Could not select climb type");
                break;
        }

        _targetRotation = Quaternion.LookRotation(SurfaceForward);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (animator.IsInTransition(0))
        {
            return;
        }

        if (!_started)
        {
            MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.one, 1.0f);
            animator.MatchTarget(_targetPosition, _targetRotation, AvatarTarget.Root, mask, _startTime, _endTime, false);
            _started = true;
        }
    }

    private void CalculateDrainpipe()
    {
        var pipe = _player.BaseStates.Drainpipe.Pipe;
        Vector3 closestPoint2 = pipe.transform.position;

        _targetPosition = closestPoint2
            - pipe.transform.forward * 0.38f
            + Vector3.up * _rungs.GetHeightAt(1)
            + Vector3.up * -0.625f;
    }

    private void CalculateLadder()
    {
        var ladder = _player.BaseStates.Ladder.Ladder;
        Vector3 closestPoint2 = ladder.transform.position;

        _targetPosition = closestPoint2
            - ladder.transform.forward * 0.375f
            + Vector3.up * _rungs.GetHeightAt(1)
            + Vector3.up * 0.0f;

        Vector3 ladderRight = Vector3.Cross(Vector3.up, ladder.transform.forward);

        _targetPosition += ladderRight * _ladderXOffset;
    }

    private void CalculateWallclimb()
    {
        Vector3 closestPoint = _rungs.ClosestPointTo(_player.transform.position);

        _targetPosition = closestPoint
            - _rungs.transform.forward * 0.5f
            + Vector3.up * _rungs.GetHeightAt(2)
            + Vector3.up * _player.Settings.wallclimbUpOffset;
    }
}
