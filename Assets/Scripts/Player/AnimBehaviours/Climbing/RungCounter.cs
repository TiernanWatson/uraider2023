using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRungCountable
{
    int RungCount { get; set; }
}

/// <summary>
/// Counts the rungs in an animation loop so climbing states can correct Lara's position
/// </summary>
public class RungCounter : StateMachineBehaviour
{
    private enum RungType
    {
        Ladder,
        Wallclimb,
        Drainpipe
    }

    private int Rung => _state.RungCount;
    private float NextRungHeight => _player.BaseStates.Wallclimb.NextPositionY;

    [SerializeField] private RungType _rungType = RungType.Wallclimb;
    [SerializeField] private bool _addOnExit = true;
    [SerializeField] private int _addPerRung = 1;
    [SerializeField] private int _rungsPerLoop = 2;

    //private float _timeIncrements;
    private float _lastNormTime;
    private float _timeIncrements;
    private PlayerController _player;
    private IRungCountable _state;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _timeIncrements = _rungsPerLoop == 0 ? 1.0f : 1.0f / _rungsPerLoop;
        _lastNormTime = 0.0f;
        _player = animator.GetComponent<PlayerController>();

        switch (_rungType)
        {
            case RungType.Wallclimb:
                _state = _player.BaseStates.Wallclimb;
                break;
            case RungType.Ladder:
                _state = _player.BaseStates.Ladder;
                break;
            case RungType.Drainpipe:
                _state = _player.BaseStates.Drainpipe;
                break;
            default:
                Debug.LogError("No rung type selected");
                break;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        float normTime = animatorStateInfo.normalizedTime % (1.0f / _rungsPerLoop);
        Debug.Log("Norm Time: " + animatorStateInfo.normalizedTime);

        if (normTime < _lastNormTime)
        {
            IncrementRung();

            /*Vector3 targetPosition = Vector3.up * NextRungHeight;
            MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.up, 0.0f);
            float startTime = animatorStateInfo.normalizedTime;
            float endTime = startTime + _timeIncrements;

            Debug.Log("On: " + Rung + " Going to: " + NextRungHeight + " ST: " + startTime + " ET: " + endTime + " TP: " + targetPosition);

            animator.MatchTarget(targetPosition, Quaternion.identity, AvatarTarget.Root, mask, startTime, endTime);*/
        }

        _lastNormTime = normTime;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_addOnExit)
        {
            IncrementRung();
        }
    }

    private void IncrementRung()
    {
        _state.RungCount += _addPerRung;
    }
}
