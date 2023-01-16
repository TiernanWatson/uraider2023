using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Human : MonoBehaviour
{
    public class DeadState : EnemyState
    {
        public DeadState(Human owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _owner.Anim.SetBool("IsCombat", false);
            if (_owner.Ragdoll)
            {
                _owner.Anim.enabled = false;
                _owner._collider.enabled = false;
                _owner.Ragdoll.DoRag();
            }
        }

        public override void Update()
        {
            //_owner.Anim.SetFloat("Speed", 0.0f);
        }
    }
}
