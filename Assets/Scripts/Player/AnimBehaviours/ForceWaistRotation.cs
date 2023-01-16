using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceWaistRotation : StateMachineBehaviour
{
    private PlayerController _player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _player = animator.GetComponent<PlayerController>();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _player.Waist.Stop();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!animatorStateInfo.IsName("JumpR") 
            && !animatorStateInfo.IsName("Backflip")
            && !animatorStateInfo.IsName("JumpL"))
        {
            Quaternion rotation = Quaternion.LookRotation(_player.transform.forward);
            _player.OverrideWaist(rotation);
        }
        else
        {
            _player.Waist.Stop();
        }
    }
}
