using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class StrafeState : PlayerState
    {
        public StrafeState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.Movement.GoToStrafe();
        }

        public override void OnExit()
        { 
        }

        public override void Update()
        {
            if (_owner.UInput.Jump.triggered)
            {
                if (_owner.AnimControl.IsIn("MoveBT") || _owner.AnimControl.IsIn("Idle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.MultiJump);
                }
            }
            else if (!_owner.IsGrounded)
            {
                _owner.AnimControl.Fall();
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else if (_owner.Ground.tag.Equals("Slope"))
            {
                _owner.AnimControl.FadeTo("SlideForward", 0.1f);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Slide);
            }
            else if (_owner.EquipedMachine.State != _owner.EquipedStates.Combat)
            {
                _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
            }
            else
            {
                _owner.Movement.Resolve();
            }
        }

        public override void OnDeath()
        {
            if (_owner.AnimControl.IsIn("Idle"))
            {
                _owner.AnimControl.Play("IdleDeath");
            }
            else
            {
                _owner.AnimControl.FadeTo("RunDeath", 0.1f);
            }

            _owner.StateMachine.ChangeState(_owner.BaseStates.Dead);
        }
    }
}
