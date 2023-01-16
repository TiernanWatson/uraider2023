using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class AirState : PlayerState
    {
        private const float DeathHeight = 8.0f;
        private const float DamageHeight = 5.0f;

        private WaterZone _requestedWater;
        private bool _divingIn;
        private bool _goingToSwing;
        private float _initialHeight;
        private float _initialStepOffset;

        public AirState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.Movement.GoToAir();

            _requestedWater = null;
            _divingIn = false;
            _goingToSwing = false;
            _initialHeight = _owner.transform.position.y;
            _initialStepOffset = _owner.Movement.Motor.StepOffset;
            _owner.Movement.Motor.StepOffset = 0.01f;
        }

        public override void OnExit()
        {
            _owner.Movement.Motor.StepOffset = _initialStepOffset;
        }

        public override void Update()
        {
            if (_owner.IsGrounded && !_divingIn)
            {
                float displacement = _initialHeight - _owner.transform.position.y;

                if (displacement > DamageHeight)
                {
                    float relativeMove = displacement -_owner.settings.damageHeight;
                    float damageAmount = relativeMove / (DeathHeight - DamageHeight);
                    _owner.Stats.ChangeHealth(-damageAmount * 100.0f);
                }

                if (_owner.Stats.Health > 0)
                {
                    // Calculated here because run target velocity isnt calculated until after update in next state
                    Vector3 targetVelocity = _owner.GetCameraRotater() * _owner.UInput.MoveInputRaw;
                    _owner.AnimControl.LandMultiJump(_owner.Velocity, targetVelocity);

                    bool inCombat = _owner.EquipedMachine.State == _owner.EquipedStates.Combat;
                    var targetState = inCombat ? (PlayerState)_owner.BaseStates.Strafe : _owner.BaseStates.Locomotion;

                    _owner.StateMachine.ChangeState(targetState);
                }
                else
                {
                    _owner.SFX.PlayFlop();
                    //string anim = _owner.AnimControl.IsIn("Dive") ? "DiveDeath" : "LandDeath";
                    //_owner.AnimControl.Play(anim);
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Dead);
                }
            }
            else if (_goingToSwing && _owner._grapple.IsHooked)
            {
                // Want to keep air physics until hook attached, and also check in case feet hit the ground above
                _owner.StateMachine.ChangeState(_owner.BaseStates.Swing);
            }
            else if (!_goingToSwing && _owner.UInput.Jump.triggered && _owner.Triggers.Grapple && IsGrappleInRange(_owner.Triggers.Grapple))
            {
                _owner.AnimControl.Play("JumpToGrapple");
                _owner.BaseStates.Swing.Grapple = _owner.Triggers.Grapple;
                _owner._grapple.HookTo(_owner.Triggers.Grapple.transform.position);
                _goingToSwing = true;
            }
            else if (_requestedWater && GetWaterDepth(_requestedWater) > _owner.settings.maxWadeDepth)
            {
                _owner.BaseStates.Swim.Zone = _requestedWater;
                _owner.StateMachine.ChangeState(_owner.BaseStates.Swim);
            }
            else
            {
                _owner.Movement.Resolve();
            }
        }

        private float GetWaterDepth(WaterZone volume)
        {
            Ray depthRay = new Ray(_owner.transform.position, Vector3.down);
            if (Physics.Raycast(depthRay, out RaycastHit hit, Mathf.Infinity, _owner.settings.groundLayers, QueryTriggerInteraction.Ignore))
            {
                float depth = volume.transform.position.y + volume.SurfaceHeight - hit.point.y;
                return depth;
            }

            return Mathf.Infinity;
        }

        public override void UpdateAnimation(PlayerAnim animControl)
        {
            // Stop fall blend tree snapping
            if (!_requestedWater)
            {
                base.UpdateAnimation(animControl);
            }
        }

        public override void OnWaterEnter(WaterZone volume)
        {
            base.OnWaterEnter(volume);

            _owner.SFX.PlaySplash();
        }

        public override void OnWaterStay(WaterZone volume)
        {
            base.OnWaterStay(volume);

            if (!_owner.AnimControl.IsIn("Dive"))
            {
                if (_owner.PredictYNextFrame() < volume.GetTopPosition() - 1.7f)
                {
                    _requestedWater = volume;
                }
            }
            else
            {
                if (_owner.PredictYNextFrame() < volume.GetTopPosition() - 1.7f)
                {
                    _divingIn = true;
                    _owner.Movement.OverrideMode = true;
                    Vector3 vel = _owner.Movement.Velocity;
                    vel.y += _owner.settings.diveEnterDrag * Time.deltaTime;
                    _owner.Movement.SetVelocity(vel);

                    if (vel.y >= -1.0f)
                    {
                        _requestedWater = volume;
                        _owner.Movement.OverrideMode = false;
                    }
                }
            }
        }
    }
}
