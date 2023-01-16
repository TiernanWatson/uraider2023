using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPositionAnnouncer : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        Debug.Log(player.transform.position.ToString("F6"));

    }
}
