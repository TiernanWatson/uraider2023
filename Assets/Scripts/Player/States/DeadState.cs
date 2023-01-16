using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class DeadState : PlayerState
    {
        public DeadState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.AnimControl.Enabled = false;
            _owner.CharControl.enabled = false;
            _owner.Ragdoll.DoRag();
            _owner.Movement.SetVelocity(Vector3.zero);
            
        }

        public override void Update()
        {
            
        }
    }
}
