using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class SwingState : PlayerState
    {
        private const float ChangeHeightRate = 4.0f;

        public GrappleZone Grapple { get; set; } = null;

        private bool _isChangingHeight;

        public SwingState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _isChangingHeight = false;

            _owner.UInput.Interact.performed += (ctx) => _owner.Movement.Swing.IsChangingHeight = _isChangingHeight = !_isChangingHeight;

            _owner.Movement.Motor.UseGroundingForce = false;
            _owner.AnimControl.ApplyRootMotion = true;  // To stop movement mode velocity juttering as it steps position

            float distanceToTether = Vector3.Distance(Grapple.transform.position, _owner.transform.position);

            _owner.Movement.Swing.TetherDistance = Mathf.Clamp(distanceToTether, 3.0f, Grapple.MaxTetherLength);
            _owner.Movement.GoToSwing(Grapple);

            _owner.AnimControl.FadeTo("SwingAirStart", 0.1f);
        }

        public override void OnExit()
        {
            _owner.UInput.Interact.performed -= (ctx) => _owner.Movement.Swing.IsChangingHeight = _isChangingHeight = !_isChangingHeight;
            _owner._grapple.Unhook();
            _owner.SFX.Halt();
            _owner.SFX.PlayGrappleWhind();
        }

        public override void Update()
        {
            if (_owner.UInput.Crouch.triggered)
            {
                _owner.Movement.SetVelocity(Vector3.zero);
                _owner.AnimControl.FadeTo("FallBlend", 0.15f);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else if (_owner.UInput.Jump.triggered && !_owner.AnimControl.IsIn("SwingAirStart") && !_owner.AnimControl.IsIn("JumpToGrapple"))
            {
                float zSpeed = UMath.HorizontalMag(_owner.Velocity);

                float horizontalSpeed = Mathf.Clamp(zSpeed, 1.35f, _owner.settings.maxSwingJumpSpeed);
                float verticalSpeed = Mathf.Max(_owner.Velocity.y, _owner._jumpUpSpeed);

                Vector3 newVelocity = _owner.transform.forward * horizontalSpeed + Vector3.up * verticalSpeed;

                _owner.Movement.SetVelocity(newVelocity);
                bool legsAreBehind = _owner.AnimControl.IsIn("GrappleStart");
                _owner.AnimControl.FadeToNormalized(legsAreBehind ? "RunJumpR" : "GrappleJump", legsAreBehind ? 0.1f : 0.05f);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else
            {
                if (_isChangingHeight)
                {
                    if (_owner.MoveInput.z > 0.5f)
                    {
                        float tryDistance = _owner.Movement.Swing.TetherDistance - ChangeHeightRate * Time.deltaTime;
                        if ( tryDistance > 3.0f )
                        {
                            _owner.AnimControl.CanClimbUp = true;
                            _owner.AnimControl.CanClimbDown = false;

                        }
                        else
                        {
                            _owner.AnimControl.CanClimbUp = false;
                            _owner.AnimControl.CanClimbDown = false;
                        }
                        _owner.Movement.Swing.TetherDistance = Mathf.Clamp(tryDistance, 3.0f, Grapple.MaxTetherLength);
                    }
                    else if (_owner.MoveInput.z < -0.5f)
                    {
                        float tryDistance = _owner.Movement.Swing.TetherDistance + ChangeHeightRate * Time.deltaTime;
                        if (tryDistance < Grapple.MaxTetherLength)
                        {
                            _owner.AnimControl.CanClimbDown = true;
                            _owner.AnimControl.CanClimbUp = false;

                        }
                        else
                        {
                            _owner.AnimControl.CanClimbDown = false;
                            _owner.AnimControl.CanClimbUp = false;
                        }
                        _owner.Movement.Swing.TetherDistance = Mathf.Clamp(tryDistance, 3.0f, Grapple.MaxTetherLength);
                    }
                    else
                    {
                        _owner.AnimControl.CanClimbUp = _owner.AnimControl.CanClimbDown = false;
                    }
                }
                else
                {
                    _owner.AnimControl.CanClimbUp = _owner.AnimControl.CanClimbDown = false;
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
