using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class PoleSwingState : PlayerState
    {
        public PolePoint Pole => _pole;

        private PolePoint _pole;

        public PoleSwingState(PlayerController owner) : base(owner)
        {
            _pole = null;
        }

        public void ReceivePole(PolePoint pole)
        {
            _pole = pole;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _owner.SetIgnoreCollision(false);

            _owner.Movement.GoToRoam();
            _owner.Movement.RootMotionRotate = true;
            _owner.Movement.Motor.UseGroundingForce = false;
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            if (_owner.AnimControl.IsIn("PoleSwing90R"))
            {
                _owner.StateMachine.ChangeState(_owner.BaseStates.PoleClimb);
            }
            else if (_owner.UInput.Crouch.triggered)
            {
                _owner.Movement.RootMotionRotate = false;
                _owner.Movement.SetVelocity(Vector3.zero);
                _owner.AnimControl.FadeTo("FallBlend", 0.2f);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else if (_owner.UInput.Jump.triggered)
            {
                _owner.StateMachine.ChangeState(_owner.BaseStates.Jump);
            }
            else
            {
                if (_owner.AnimControl.IsTag("Moving"))
                {
                    Vector3 playerPosition = _owner.transform.position;
                    playerPosition.y = _pole.transform.position.y - 2.059f /*- 0.095f*/;
                    _owner.LerpTo(playerPosition);

                    float directionAngle = Vector3.Angle(_owner.transform.forward, _pole.transform.right);

                    if (directionAngle < 90.0f)
                    {
                        _owner.SetRotation(Quaternion.LookRotation(_pole.transform.right));
                    }
                    else
                    {
                        _owner.SetRotation(Quaternion.LookRotation(-_pole.transform.right));
                    }
                }

                _owner.Movement.Resolve();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }
    }
}
