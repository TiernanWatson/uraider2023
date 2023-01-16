using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RungCountBehaviour : TimedMethodAnimBehaviour
{
    [SerializeField] private int _rungIncrement = 1;

    private PlayerController _player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, animatorStateInfo, layerIndex);

        if (!_player)
        {
            _player = animator.GetComponent<PlayerController>();
        }
    }

    protected override void Execute()
    {
        _player.RungIncrement(_rungIncrement);
    }
}
