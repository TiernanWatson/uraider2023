using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimedMethodAnimBehaviour : StateMachineBehaviour
{
    [SerializeField] private bool _eventCanLoop = true;
    [SerializeField] private float _timeToPlay = 0.0f;

    private bool _executed = false;
    private float _lastNormTime = 0.0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _lastNormTime = 0.0f;
        _executed = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        float normTime = animatorStateInfo.normalizedTime % 1.0f;

        if (_eventCanLoop && normTime < _lastNormTime)
        {
            _executed = false;
        }

        if (_timeToPlay < normTime && !_executed)
        {
            Execute();
            _executed = true;
        }

        _lastNormTime = normTime;
    }

    protected abstract void Execute();
}
