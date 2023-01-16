using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    public abstract class GroundState : PlayerState
    {
        protected bool _goingToGrab;

        public GroundState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.Movement.GoToRoam();
            _goingToGrab = false;
        }

        public override void OnExit()
        {
        }

        public override void Update()
        {
            if (_goingToGrab)
            {
                if (_owner.AnimControl.IsIn("Grab") || _owner.AnimControl.IsIn("HangIdle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Climb);
                }
            }
            else if (!_owner.IsGrounded)
            {
                Vector3 rayStart = _owner.transform.position;
                Vector3 rayDirection = -_owner.transform.forward;

                Debug.DrawRay(rayStart, rayDirection, Color.green, 5.0f);

                if (LedgePoint.FindLedge(rayStart, rayDirection, 1.0f, _owner.settings.ledgeLayers, out LedgePoint ledge))
                {
                    if (Vector3.Angle(ledge.transform.forward, rayDirection) < 80.0f)
                    {
                        _owner.Movement.Motor.UseGroundingForce = false;
                        _goingToGrab = true;
                        _owner.BaseStates.Climb.ReceiveLedge(ledge);
                    }
                }

                if (_goingToGrab)
                {
                    _owner.SetIgnoreCollision(true);
                    _owner.AnimControl.Play("LastChanceGrab");
                }
                else
                {
                    _owner.AnimControl.FadeTo("FallBlend", 0.25f);
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
                }
            }
            else
            {
                if (_owner.Ground.tag.Equals("Slope"))
                {
                    _owner.AnimControl.FadeTo("SlideForward", 0.1f);
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Slide);
                }

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

        public override bool CanPickup(InventoryItem item)
        {
            return true;
        }
    }
}
