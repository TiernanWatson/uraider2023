using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHeightAdjuster : StateMachineBehaviour
{
    [SerializeField] private AnimationCurve _heightCurve;

    private PlayerController _player;
    private PlayerCamTracker _camTracker;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (!_player)
        {
            _player = animator.GetComponent<PlayerController>();
            _camTracker = _player.CameraControl.Follow.GetComponent<PlayerCamTracker>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        float normalizedTime = animatorStateInfo.normalizedTime;
        _camTracker.IsOverriding = true;
        _camTracker.ForcedHeight = _heightCurve.Evaluate(normalizedTime);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _camTracker.IsOverriding = false;
    }
}
