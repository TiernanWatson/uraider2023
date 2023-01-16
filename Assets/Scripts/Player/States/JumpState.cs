using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class JumpState : PlayerState
    {
        public JumpState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.AnimControl.IsJumping = true;
            _owner.Movement.Motor.UseGroundingForce = false;
        }

        public override void OnExit()
        {
            _owner.AnimControl.IsJumping = false;
            _owner.AnimControl.IsDiving = false;
        }

        public override void Update()
        {
            if (_owner.UInput.Crouch.triggered)
            {
                _owner.AnimControl.IsDiving = true;
            }

            PlayerAnim.JumpType jumpType = _owner.AnimControl.GetJumpType();

            if (jumpType != PlayerAnim.JumpType.None)
            {
                bool wasSliding = _owner.StateMachine.LastState == _owner.BaseStates.Slide;
                bool wasClimbing = _owner.StateMachine.LastState == _owner.BaseStates.Climb;
                bool wasSwinging = _owner.StateMachine.LastState == _owner.BaseStates.Swing;
                bool inCombat = _owner.EquipedMachine.State == _owner.EquipedStates.Combat;
                bool isDiving = jumpType == PlayerAnim.JumpType.Dive;

                _owner.SFX.PlayJump();

                // Force standing jump to snap to rotation input for better control
                if (jumpType == PlayerAnim.JumpType.Stand)
                {
                    if (!wasSliding && !wasClimbing && !wasSwinging && _owner.UInput.MoveInputRaw.sqrMagnitude > 0.01f)
                    {
                        Vector3 rotatedMove = _owner.GetCameraRotater() * _owner.UInput.MoveInputRaw;
                        Quaternion jumpRotation = Quaternion.LookRotation(rotatedMove);
                        _owner.Movement.SetRotation(jumpRotation);
                    }
                }

                float forwardSpeed = wasSliding ? _owner._jumpForwardSpeed : _owner.CalculateJumpSpeed();
                if (wasSwinging)
                {
                    forwardSpeed = UMath.HorizontalMag(_owner.Velocity);
                }

                Debug.Log("FS: " + forwardSpeed);

                bool isAutoGrabSuccess = false;

                Vector3 rayDirection = _owner.transform.forward;
                float maxDistance = 6.0f;
                float maxHeight = _owner.settings.jumpHeight + _owner.settings.grabSDownOffset;

                // First try look for forward grabs
                if (!inCombat && !isDiving && _owner.TryFindClosestGrabbable(_owner.transform.position, rayDirection, maxDistance, maxHeight, out AutoGrabDetectFlags type, out int index))
                {
                    Debug.Log("Found: " + type);
                    if (type == AutoGrabDetectFlags.Ledge && TryAutoGrabLedge(_owner.GrabDetector.Ledges[index].value, forwardSpeed))
                    {
                        _owner.BaseStates.Climb.Ledge = _owner.GrabDetector.Ledges[index].value;
                        _owner.BaseStates.Climb.FreeclimbSurface = null;
                        _owner.StateMachine.ChangeState(_owner.BaseStates.AutoGrabLedge);
                        isAutoGrabSuccess = true;
                    }
                    else if (type == AutoGrabDetectFlags.Pole && TryAutoGrabPole(_owner.GrabDetector.Poles[index].value, forwardSpeed))
                    {
                        _owner.BaseStates.PoleClimb.ReceivePole(_owner.GrabDetector.Poles[index].value);
                        _owner.BaseStates.PoleSwing.ReceivePole(_owner.GrabDetector.Poles[index].value);
                        _owner.StateMachine.ChangeState(_owner.BaseStates.AutoGrabLedge);
                        isAutoGrabSuccess = true;
                    }
                    else if (type == AutoGrabDetectFlags.Monkey && TryAutoGrabMonkey(_owner.GrabDetector.Monkeys[index].value, _owner._jumpUpSpeed))
                    {
                        _owner.BaseStates.Monkey.Surface = _owner.GrabDetector.Monkeys[index].value;
                        _owner.StateMachine.ChangeState(_owner.BaseStates.AutoGrabLedge);
                        isAutoGrabSuccess = true;
                    }
                    else if (type == AutoGrabDetectFlags.Wallclimb 
                        && TryAutoGrabWallclimb(_owner.GrabDetector.Wallclimbs[index].value, 
                        _owner.GrabDetector.Wallclimbs[index].hit, 
                        forwardSpeed, 
                        _owner._jumpUpSpeed))
                    {
                        _owner.BaseStates.Wallclimb.Surface = _owner.GrabDetector.Wallclimbs[index].value;
                        _owner.StateMachine.ChangeState(_owner.BaseStates.AutoGrabLedge);
                        isAutoGrabSuccess = true;
                    }
                }
                else if (jumpType == PlayerAnim.JumpType.LedgeSide)
                {
                    Debug.Log("Do a side test plz");

                    float sweepIncrement = 1.0f;

                    rayDirection = -_owner.transform.right;
                    maxDistance = 1.0f;
                    maxHeight = 6.0f;

                    // Try find side jumps
                    for (float sweep = sweepIncrement; sweep < 6.0f; sweep += sweepIncrement)
                    {
                        Vector3 startPosition = _owner.transform.position + _owner.transform.forward * sweep;
                        if (_owner.TryFindClosestGrabbable(startPosition, rayDirection, maxDistance, maxHeight, out AutoGrabDetectFlags type2, out int index2))
                        {
                            Debug.Log("Found2: " + type2);
                            if (type2 == AutoGrabDetectFlags.Ledge && TryAutoGrabLedge(_owner.GrabDetector.Ledges[index2].value, forwardSpeed))
                            {
                                Debug.Log("Will auto grab");
                                _owner.BaseStates.Climb.Ledge = _owner.GrabDetector.Ledges[index2].value;
                                _owner.BaseStates.Climb.FreeclimbSurface = null;
                                _owner.StateMachine.ChangeState(_owner.BaseStates.AutoGrabLedge);
                                isAutoGrabSuccess = true;
                            }
                        }
                    }
                }

                if (!isAutoGrabSuccess)
                {
                    if (!wasSwinging)
                    {
                        Vector3 vel = _owner.transform.forward * forwardSpeed + Vector3.up * _owner._jumpUpSpeed;
                        _owner.Movement.SetVelocity(vel);
                    }

                    _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
                }
            }
            else
            {
                _owner.Movement.Resolve();
            }
        }

        private bool TryAutoGrabWallclimb(WallclimbSurface surface, RaycastHit hit, float forwardSpeed, float upSpeed)
        {
            Vector3 handPoint = _owner.transform.position + Vector3.up * (_owner.settings.jumpHeight + _owner.settings.grabDownOffset);
            int maxRung = surface.Rungs.ClosestRungTo(handPoint);
            float height = surface.Rungs.GetHeightAt(maxRung);

            Vector3 hitPoint = hit.point;
            hitPoint = hit.collider.transform.InverseTransformPoint(hitPoint);
            hitPoint.z = 0.0f;  // Remove central deviation
            hitPoint.y = height;
            hitPoint = hit.collider.transform.TransformPoint(hitPoint);

            Vector3 relativePoint = hitPoint - _owner.transform.position;
            float ledgeDistance = UMath.HorizontalMag(relativePoint);

            if (IsWithinReach(forwardSpeed, upSpeed, ledgeDistance, relativePoint.y))
            {
                _owner.BaseStates.Wallclimb.RungCount = maxRung - 2;
                _owner.BaseStates.AutoGrabLedge.ReceiveContext(AutoGrabType.Wallclimb, hitPoint, surface.transform.forward, forwardSpeed);
                return true;
            }

            return false;
        }

        private bool TryAutoGrabPole(PolePoint pole, float forwardSpeed)
        {
            float angle = Vector3.Angle(_owner.transform.forward, pole.transform.forward);
            AutoGrabType grabType = angle < 45.0f || angle > 135.0f ? AutoGrabType.PoleClimb : AutoGrabType.PoleSwing;

            Vector3 grabPoint, grabDirection;

            if (grabType == AutoGrabType.PoleClimb)
            {
                grabPoint = FindPoleClimbPoint(pole);
                if (Vector3.Angle(_owner.transform.forward, pole.transform.forward) < 90.0f)
                    grabDirection = pole.transform.forward;
                else
                    grabDirection = -pole.transform.forward;

                if (_owner.transform.position.y > grabPoint.y)
                {
                    return false;
                }
            }
            else
            {
                forwardSpeed = Mathf.Max(0.5f, forwardSpeed);
                float t = pole.Collider.Point.ClosestParamTo(_owner.transform.position, _owner.transform.forward);
                grabPoint = pole.Collider.Point.GetPoint(t);
                if (Vector3.Angle(_owner.transform.forward, pole.transform.right) < 90.0f)
                    grabDirection = pole.transform.right;
                else
                    grabDirection = -pole.transform.right;
            }

            Vector3 relativePolePoint = grabPoint - _owner.transform.position;

            float poleDistance = UMath.HorizontalMag(relativePolePoint);
            float polePoint = relativePolePoint.y;
            float upSpeed = _owner.StateMachine.LastState == _owner.BaseStates.Swing ? _owner.Velocity.y : _owner._jumpUpSpeed;

            if (IsWithinReach(forwardSpeed, upSpeed, poleDistance, polePoint))
            {
                _owner.BaseStates.AutoGrabLedge.ReceiveContext(grabType, grabPoint, grabDirection, forwardSpeed);
                return true;
            }

            return false;
        }

        private Vector3 FindPoleClimbPoint(PolePoint pole)
        {
            Vector3 point = _owner.transform.position;
            point.y = pole.transform.position.y;

            Vector3 localPoint = pole.transform.InverseTransformPoint(point);
            localPoint.x = 0.0f;  // Remove deviation from pole center

            return pole.transform.TransformPoint(localPoint);
        }

        private bool TryAutoGrabLedge(LedgePoint ledge, float forwardSpeed)
        {
            forwardSpeed = Mathf.Max(0.5f, forwardSpeed);

            float t = ledge.Collider.Point.ClosestParamTo(_owner.transform.position, _owner.transform.forward);

            // Player intersects beyond end, try grab closest end
            if (t < ledge.Collider.Point.GetMinT() || t > ledge.Collider.Point.GetMaxT())
            {
                Debug.Log("Zero me baby");
                t = Mathf.Clamp(ledge.Collider.Point.ClosestParamTo(_owner.transform.position), 0.0f, ledge.Collider.Point.GetMaxT());
                if (float.IsNaN(t))
                {
                    Debug.LogWarning("t parameter for ledge was NaN");
                    t = 0.0f;
                }
            }

            t = ledge.Collider.Point.PaddingClamp(t, 0.5f);

            Vector3 grabPoint = ledge.Collider.Point.GetPoint(t);
            Vector3 relativeGrabPoint = grabPoint - _owner.transform.position;

            Debug.DrawRay(grabPoint, Vector3.up, Color.red, 5.0f);
           
            float ledgeDistance = UMath.HorizontalMag(relativeGrabPoint);
            float ledgeHeight = relativeGrabPoint.y;
            float upSpeed = _owner.StateMachine.LastState == _owner.BaseStates.Swing ? _owner.Velocity.y : _owner._jumpUpSpeed;

            if (ledge.ClimbUp != ClimbUpType.Stand || IsTooLowButWithinReach(forwardSpeed, upSpeed, ledgeDistance, ledgeHeight))
            {
                _owner.BaseStates.AutoGrabLedge.ReceiveContext(AutoGrabType.JumpToLedge, grabPoint, ledge.Forward, forwardSpeed);
                return true;
            }

            return false;
        }

        private bool TryAutoGrabMonkey(MonkeySurface surface, float upSpeed)
        {
            Vector3 grabPoint = surface.ClosetPointTo(_owner.transform.position);
            Vector3 relativePoint = grabPoint - _owner.transform.position;
            float peakHeight = UMath.GetPeakHeight(upSpeed, _owner.settings.gravity);

            if (peakHeight > (relativePoint.y - _owner.settings.monkeyOffset))
            {
                _owner.BaseStates.AutoGrabLedge.ReceiveContext(AutoGrabType.Monkey, grabPoint, _owner.transform.forward, 0.0f);
                return true;
            }

            return false;
        }

        private bool IsTooLowButWithinReach(float forwardSpeed, float upSpeed, float ledgeDistance, float ledgeHeight)
        {
            return !UMath.CanClear(forwardSpeed, upSpeed, ledgeDistance, ledgeHeight, _owner.settings.gravity, out float heightOffset) 
                && heightOffset > -_owner.settings.jumpGrabError;
        }

        private bool IsWithinReach(float forwardSpeed, float upSpeed, float ledgeDistance, float ledgeHeight)
        {
            return UMath.HeightAt(ledgeDistance, forwardSpeed, upSpeed, _owner.settings.gravity) > (ledgeHeight - _owner.settings.jumpGrabError);
        }
    }
}
