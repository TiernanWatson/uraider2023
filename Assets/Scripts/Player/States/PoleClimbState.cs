using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class PoleClimbState : PlayerState
    {
        private PolePoint _pole;

        public PoleClimbState(PlayerController owner) : base(owner)
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

            _owner.Movement.GoToRoam();
            _owner.Movement.RootMotionRotate = true;
            _owner.Movement.Motor.UseGroundingForce = false;
            _owner.SetIgnoreCollision(false);
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.Movement.RootMotionRotate = false;
        }

        public override void Update()
        {
            if (_owner.AnimControl.IsIn("PoleClimb90L"))
            {
                _owner.StateMachine.ChangeState(_owner.BaseStates.PoleSwing);
            }
            else if (_owner.UInput.Crouch.triggered)
            {
                _owner.Movement.SetVelocity(Vector3.zero);
                _owner.AnimControl.FadeTo("FallBlend", 0.2f);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else
            {
                _owner.Movement.Resolve();

                float dirAngle = Vector3.Angle(_pole.transform.forward, _owner.Movement.TargetVelocity);
                bool isForward = dirAngle < 90.0f;
                var splinePoint = isForward ? _pole.Collider.Point.Next : _pole.Collider.Point;

                Vector3 playerRelative = splinePoint.transform.InverseTransformPoint(_owner.transform.position);

                if (!_owner.AnimControl.IsIn("GrabPipe") && !_owner.AnimControl.IsIn("PoleSwing90R"))
                {
                    if (!_owner.AnimControl.IsIn("PipeReverse"))
                    {
                        playerRelative.x = 0.0f;
                    }

                    // Stop player moving off pole
                    bool beyondExtents = isForward ? playerRelative.z > 0.0f : playerRelative.z < 0.0f;
                    if (beyondExtents)
                    {
                        playerRelative.z = 0.0f;
                    }

                    Vector3 newPosition = splinePoint.transform.TransformPoint(playerRelative);

                    // Correct height if need be
                    newPosition.y = _pole.transform.position.y - _owner.settings.poleHandDownOffset;
                    _owner.LerpTo(newPosition);
                }

                if (_owner.Movement.TargetVelocity.sqrMagnitude > 0.01f)
                {
                    // Stop moving animation if at end
                    bool canMove = isForward ? playerRelative.z > -0.5f : playerRelative.z < 0.5f;
                    if (canMove)
                    {
                        _owner.Movement.SetVelocity(Vector3.zero);
                    }
                }
            }
        }
    }
}
