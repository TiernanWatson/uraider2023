using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterColliderAdjuster : StateMachineBehaviour
{
    [SerializeField] private bool _resetOnExit = false;
    [SerializeField] private bool _adjustPosition = false;
    [SerializeField] private bool _adjustHeight = false;
    [SerializeField] private AnimationCurve _heightCurve;
    [SerializeField] private AnimationCurve _positionCurve;

    private float _heightOld;
    private float _positionOld;

    private PlayerController _player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (!_player)
        {
            _player = animator.GetComponent<PlayerController>();
        }

        if (_resetOnExit)
        {
            _heightOld = _player.CharControl.height;
            _positionOld = _player.CharControl.center.y;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        float normalizedTime = animatorStateInfo.normalizedTime;
        
        if (_adjustHeight)
        {
            _player.CharControl.height = _heightCurve.Evaluate(normalizedTime);
        }

        if (_adjustPosition)
        {
            Vector3 position = _player.CharControl.center;
            position.y = _positionCurve.Evaluate(normalizedTime);
            _player.CharControl.center = position;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (_resetOnExit)
        {
            _player.CharControl.height = _heightOld;
            Vector3 position = _player.CharControl.center;
            position.y = _positionOld;
            _player.CharControl.center = position;
        }
    }
}
