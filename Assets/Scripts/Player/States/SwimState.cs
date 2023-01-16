using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    public class SwimState : PlayerState
    {
        public WaterZone Zone { get; set; }
        public bool IsSubmerged => _isSubmerged;

        private bool _isClimbingOut;
        private bool _isDipping;
        private bool _isSubmerged;
        private float _initialStepOffset;
        private SplineCollider _poolEdgeCollider;

        public SwimState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _isClimbingOut = false;
            _isDipping = false;
            _poolEdgeCollider = null;
            _initialStepOffset = _owner.Movement.Motor.StepOffset;
            _owner.Movement.Motor.StepOffset = 0.01f;

            _owner.AnimControl.IsSwimming = true;

            if (_owner.StateMachine.LastState != _owner.BaseStates.Locomotion)
            {
                _isSubmerged = true;
                _owner.Movement.GoToSwim();
                _owner.Movement.Motor.UseHeadCollision = true;
                _owner.Movement.Motor.CapsuleHeight = 0.5f;
                _owner.Movement.Motor.CharControl.center = Vector3.zero;
                _owner.SFX.PlaySplash();
            }
            else // Coming from wade
            {
                _isSubmerged = false;
                _owner.Movement.GoToRoam();
                _owner.AnimControl.ApplyRootMotion = true;
                _owner.Movement.Motor.UseHeadCollision = false;
            }

            _owner.Movement.Motor.UseGroundingForce = false;

            _owner.UInput.Crouch.performed += DipDown;
        }

        public override void OnExit()
        {
            _owner.Movement.Motor.StepOffset = _initialStepOffset;
            _owner.AnimControl.IsSwimming = false;
            _owner.Movement.Motor.UseHeadCollision = false;
            _owner.UInput.Crouch.performed -= DipDown;
        }

        public override void Update()
        {
            if (_isClimbingOut)
            {
                if (_owner.AnimControl.IsIn("Idle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
                }
            }
            else 
            {
                if (_isSubmerged)  // Underwater
                {
                    if (_owner.AnimControl.IsIn("SwimBlend"))
                    {
                        bool headOutOfWater = !_owner.Movement.Motor.TestHeadCollision(Zone.Collider);
                        if (headOutOfWater)
                        {
                            GoToSurf();
                        }
                    }
                }
                else  // Treading
                {
                    Ray depthRay = new Ray(_owner.GetColliderBottom(), Vector3.down);
                    if (_owner.IsGrounded && _owner.AnimControl.IsTag("Swimming"))
                    {
                        if (Physics.Raycast(depthRay, out RaycastHit hit, _owner.settings.treadYOffset, _owner.settings.groundLayers, QueryTriggerInteraction.Ignore))
                        {
                            _owner.AnimControl.IsWading = true;
                            _owner.AnimControl.FadeTo("SurfToWade", 0.15f);
                            _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
                            return;
                        }
                    }

                    if (_owner.AnimControl.IsTag("Swimming"))
                    {
                        CorrectSurfaceHeight();
                    }

                    if (_poolEdgeCollider && Vector3.Angle(_poolEdgeCollider.transform.forward, _owner.transform.forward) < 30.0f)
                    {
                        float height = _poolEdgeCollider.transform.position.y - (Zone.transform.position.y + Zone.SurfaceHeight);
                        string anim = height < 0.46875f ? "SurfToLedge8th" : "SurfToLedge4th";
                        _isClimbingOut = true;
                        Vector3 targetPos = Vector3.up * _poolEdgeCollider.transform.position.y;
                        Quaternion targetRot = _poolEdgeCollider.transform.rotation;
                        MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.up, 1.0f);
                        _owner.AnimControl.TargetMatchState(anim, targetPos, targetRot, mask, 0.15f, 0.99f);
                        _owner.AnimControl.FadeTo(anim, 0.1f);
                    }

                    if (_isDipping && _owner.AnimControl.IsInTrans("TreadToSwim -> SwimBlend"))
                    {
                        GoToSwim();
                        _isDipping = false;
                    }
                }
            }

            _owner.Movement.Resolve();
        }

        public override void UpdateAnimation(PlayerAnim animControl)
        {
            // Stop blend tree snapping from velocity change
            if (!_owner.AnimControl.IsIn("FallBlend"))
            {
                base.UpdateAnimation(animControl);
            }

            // Don't want swim angle blend tree to affect coming to surface
            if (_isSubmerged)
            {
                float swimAngle = Vector3.SignedAngle(_owner.transform.forward, _owner.Velocity, _owner.transform.right);
                _owner.AnimControl.SwimAngle = swimAngle;
            }
        }

        public override void OnSplineStay(SplineCollider collider)
        {
            base.OnSplineStay(collider);

            if (Vector3.Angle(_owner.transform.forward, collider.transform.forward) < 30.0f)
            {
                _poolEdgeCollider = collider;
            }
        }

        private void DipDown(InputAction.CallbackContext ctx)
        {
            if (!_isSubmerged && !_isDipping)
            {
                _isDipping = true;
                _owner.AnimControl.FadeTo("TreadToSwim", 0.1f);
                _owner.Movement.GoToSwim();
            }
        }

        private void CorrectSurfaceHeight()
        {
            Vector3 newPosition = _owner.transform.position;
            newPosition.y = Zone.GetTopPosition() - _owner.settings.treadYOffset;
            _owner.LerpTo(newPosition);
        }

        private void GoToSurf()
        {
            _isSubmerged = false;
            _owner.Movement.GoToRoam();
            _owner.AnimControl.FadeTo("SurfTread", 0.2f);
            _owner.AnimControl.ApplyRootMotion = true;
            _owner.Movement.Motor.UseGroundingForce = false;
            _owner.Movement.Motor.UseHeadCollision = false;
            _owner.Movement.Motor.CapsuleHeight = 1.75f;
            _owner.Movement.Motor.CharControl.center = Vector3.up * -0.25f;
        }

        private void GoToSwim()
        {
            _isSubmerged = true;
            _owner.Movement.Motor.UseHeadCollision = true;
            _owner.AnimControl.ApplyRootMotion = false;
        }
    }
}
