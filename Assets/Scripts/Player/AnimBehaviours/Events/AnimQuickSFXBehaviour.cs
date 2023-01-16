using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimQuickSFXBehaviour : TimedMethodAnimBehaviour
{
    [SerializeField] private AudioClip _sound;
    [SerializeField] private bool _stopOnExit = false;

    private PlayerSFX _sfx;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, animatorStateInfo, layerIndex);

        if (!_sfx)
        {
            _sfx = animator.GetComponent<PlayerController>().SFX;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (_stopOnExit && _sfx)
        {
            _sfx.Halt();
        }
    }

    protected override void Execute()
    {
        _sfx.PlayOnce(_sound);
    }
}
