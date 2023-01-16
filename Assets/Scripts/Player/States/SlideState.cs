using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class SlideState : PlayerState
    {
        public SlideState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _owner.Movement.GoToSlide();
            _owner.AnimControl.ApplyRootMotion = false;
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            if (!_owner.IsGrounded)
            {
                _owner.AnimControl.FadeTo("FallBlend", 0.1f);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else if (_owner.IsGrounded && !_owner.Ground.tag.Equals("Slope"))
            {
                _owner.AnimControl.FadeTo("SlideForwardStop", 0.05f);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
            }
            else if (_owner.UInput.Jump.triggered)
            {
                // Make sure player jumps down slope and not mid-interp
                _owner.SetRotation(_owner.Movement.TargetRotation);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Jump);
            }
            else
            {
                _owner.Movement.Resolve();
            }
        }
    }
}
