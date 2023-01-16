using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public enum AutoGrabType
    {
        JumpToLedge,  // Normal locomotion to ledge jump
        LedgeJumpUp,  // Ledge to ledge up
        LedgeDropDown, // Ledge to ledge down
        PoleClimb,         // Jump to pole climb
        PoleSwing,    // Jump to pole swing
        Wallclimb,
        Monkey
    }

    public class AutoGrabLedgeState : AirState
    {
        private const float MinAutoGrabTime = 0.45f;

        private Vector3 _point;
        private Vector3 _forward;
        private Vector3 _aimPoint;  // Point to aim for
        private Vector3 _grabPoint; // Point to grab once twisted
        private Vector3 _hangPoint;  // Final hang position
        private Quaternion _targetRotation;

        private AutoGrabType _grabType;
        private float _forwardSpeed;
        private float _timeToReach;
        private float _rotationRate;
        private bool _sameLedge;

        public AutoGrabLedgeState(PlayerController owner) : base(owner)
        {
            _point = Vector3.zero;
            _forwardSpeed = 0.0f;
            _timeToReach = 0.0f;
        }

        public void ReceiveContext(AutoGrabType type, Vector3 point, Vector3 forward, float forwardSpeed)
        {
            _grabType = type;
            _point = point;
            _forward = forward;
            _forwardSpeed = forwardSpeed;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            // Stop jump trajectory being put off
            _owner.SetIgnoreCollision(true);

            float grabAngle = 0.0f;

            Vector3 grabRightDir = _owner.transform.right;
            Vector3 grabForwardDir = _owner.transform.forward;

            _owner.AnimControl.GrabAngle = 0.0f;

            _targetRotation = _owner.transform.rotation;

            switch (_grabType)
            {
                case AutoGrabType.Wallclimb:
                case AutoGrabType.JumpToLedge:
                    {
                        grabRightDir = _owner.BaseStates.Climb.Ledge.Right;
                        grabForwardDir = _owner.BaseStates.Climb.Ledge.Forward;

                        grabAngle = Vector3.SignedAngle(_owner.transform.forward, _owner.BaseStates.Climb.Ledge.Forward, Vector3.up);
                        _owner.AnimControl.GrabAngle = grabAngle;

                        bool standing = _owner.AnimControl.IsIn("JumpU");
                        bool hasFeetRoom = _owner.BaseStates.Climb.Ledge.HasWall;

                        if (standing)
                        {
                            _aimPoint = _grabPoint = _owner._animData.GetStandGrabLocation(_point, grabForwardDir);
                        }
                        else
                        {
                            _aimPoint = _grabPoint = _owner._animData.GetGrabLocation(_point, grabForwardDir, grabRightDir, grabAngle);
                            if (!hasFeetRoom)
                            {
                                _grabPoint = _owner._animData.GetGrabNoWallLocation(_point, grabForwardDir, grabRightDir, grabAngle);
                            }
                        }

                        break;
                    }
                case AutoGrabType.LedgeJumpUp:
                    {
                        _sameLedge = _owner.BaseStates.Climb.Ledge == _owner.BaseStates.Climb.HopUpLedge;
                        _aimPoint = _grabPoint = _sameLedge ? _owner._animData.GetDropDownLocation(_point, grabForwardDir) : _owner._animData.GetJumpUpLocation(_point, grabForwardDir);
                        break;
                    }
                case AutoGrabType.LedgeDropDown:
                    {
                        bool hasWall = _owner.BaseStates.Climb.Ledge.HasWall;
                        _aimPoint = _grabPoint = hasWall ? _owner._animData.GetDropDownLocation(_point, grabForwardDir) : _owner._animData.GetDropDownNoWallLocation(_point, grabForwardDir);
                        break;
                    }
                case AutoGrabType.PoleSwing:
                    {
                        _aimPoint = _grabPoint = _owner._animData.GetPoleGrabLocation(_point, grabForwardDir);

                        float directionAngle = Vector3.Angle(_owner.transform.forward, _owner.BaseStates.PoleSwing.Pole.transform.right);
                        bool isFacingDownX = directionAngle < 90.0f;

                        _targetRotation = Quaternion.LookRotation((isFacingDownX ? 1.0f : -1.0f) * _owner.BaseStates.PoleSwing.Pole.transform.right);
                        break;
                    }
                default:
                    {
                        _aimPoint = _grabPoint = _owner._animData.GetGrabLocation(_point, grabForwardDir, grabRightDir, grabAngle);
                        break;
                    }
            }

            Vector3 playerToAimPoint = _aimPoint - _owner.transform.position;
            float horizontalDistance = UMath.HorizontalMag(playerToAimPoint);

            Vector3 jumpDirection = playerToAimPoint;
            jumpDirection.y = 0.0f;
            jumpDirection.Normalize();

            Debug.DrawRay(_owner.transform.position, jumpDirection, Color.magenta, 5.0f);
            Debug.DrawRay(_aimPoint, Vector3.up, Color.magenta, 5.0f);

            _timeToReach = horizontalDistance / _forwardSpeed;

            if (_grabType == AutoGrabType.LedgeJumpUp)
            {
                float ySpeed = _owner.settings.ledgeJumpUpMinSpeed;
                _timeToReach = -ySpeed + Mathf.Sqrt(ySpeed * ySpeed - 2.0f * -_owner.settings.gravity * -playerToAimPoint.y);
                _timeToReach /= -_owner.settings.gravity;
                _sameLedge = false;

                // Likely the same ledge we were on
                if (_timeToReach < 0.01f)
                {
                    _timeToReach = -ySpeed - Mathf.Sqrt(ySpeed * ySpeed - 2.0f * -_owner.settings.gravity * -playerToAimPoint.y);
                    _timeToReach /= -_owner.settings.gravity;
                    _sameLedge = true;
                }

                Vector3 jumpVelocity = _owner.transform.forward * _forwardSpeed + Vector3.up * ySpeed;
                _owner.Movement.SetVelocity(jumpVelocity);
            }
            else if (_grabType == AutoGrabType.LedgeDropDown)
            {
                _timeToReach = Mathf.Sqrt(2.0f * -playerToAimPoint.y / -_owner.settings.gravity);
                _owner.Movement.SetVelocity(Vector3.zero);
            }
            else
            {
                if (_owner.AnimControl.IsIn("JumpU"))
                {
                    float ySpeed = UMath.PeakAt(playerToAimPoint.y, -_owner.settings.gravity, out _timeToReach);
                    _forwardSpeed = horizontalDistance / _timeToReach;
                    Vector3 jumpVelocity = _owner.transform.forward * _forwardSpeed + Vector3.up * ySpeed;
                    _owner.Movement.SetVelocity(jumpVelocity);
                }
                else
                {
                    float ySpeed = UMath.JumpToReach(playerToAimPoint.y, _timeToReach, _owner.settings.gravity);

                    Debug.Log("YSpeed: " + ySpeed + " with time: " + _timeToReach);

                    // Always have some upwards jump if ledge is below, or player can clip geometry easily
                    if (ySpeed < 1.0f && _aimPoint.y - _owner.transform.position.y < 0.0f)
                    {
                        Debug.Log("Boosting player jump speed as ledge is below");

                        ySpeed = _owner._jumpUpSpeed;
                        _forwardSpeed = UMath.JumpZToReach(playerToAimPoint.y, horizontalDistance, ySpeed, _owner.settings.gravity, out _timeToReach);
                        Vector3 jumpVelocity = _owner.transform.forward * _forwardSpeed + Vector3.up * ySpeed;
                        _owner.Movement.SetVelocity(jumpVelocity);
                    }
                    else if (_timeToReach < MinAutoGrabTime) // Don't want player just to snap to ledge
                    {
                        Debug.Log("Elongating the time to reach the ledge");

                        Vector3 timeLimited = UMath.JumpInTime(_owner.transform.position, _aimPoint, _owner.settings.gravity, MinAutoGrabTime);
                        _owner.Movement.SetVelocity(timeLimited);
                        _timeToReach = MinAutoGrabTime;
                    }
                    else
                    {
                        Debug.Log("Using default forward speed and adjusting up speed");

                        Vector3 jumpVelocity = jumpDirection * _forwardSpeed + Vector3.up * ySpeed;
                        //Vector3 jumpVelocity = _owner.transform.forward * _forwardSpeed + Vector3.up * ySpeed;
                        _owner.Movement.SetVelocity(jumpVelocity);
                    }
                }
            }

            _owner.AnimControl.IsReaching = true;

            Vector3 targetForward = _targetRotation * Vector3.forward;
            _rotationRate = Vector3.Angle(_owner.transform.forward, targetForward) / _timeToReach;
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.AnimControl.IsReaching = false;
        }

        public override void Update()
        {
            base.Update();

            Quaternion newRotation = Quaternion.RotateTowards(_owner.transform.rotation, _targetRotation, Time.deltaTime * _rotationRate);
            _owner.SetRotation(newRotation);

            _timeToReach -= Time.deltaTime;

            float yNextFrame = _owner.transform.position.y + _owner.Movement.Velocity.y * Time.deltaTime;
            bool willPassNextFrame = _owner.Movement.Velocity.y < 0.0f && yNextFrame < _aimPoint.y;

            // If reached the ledge, grab it
            if (_timeToReach <= 0.0f || willPassNextFrame)
            {
                _owner.Movement.SetVelocity(Vector3.zero);
                _owner.Movement.SetRotation(_targetRotation);
                _owner.Movement.SetPosition(_grabPoint);

                _owner.AnimControl.Play(GetGrabAnimName());
                //_owner.AnimControl.TargetMatchState(animName, _hangPoint, rotation, 0.1f, 0.99f);

                _owner.StateMachine.ChangeState(GetNextState());

                _owner.SFX.PlayClimbExert();
            }
        }

        private string GetGrabAnimName()
        {
            switch (_grabType)
            {
                case AutoGrabType.LedgeJumpUp:
                    return !_sameLedge ? "LedgeJumpGrab" : "FallGrabBraced";
                case AutoGrabType.LedgeDropDown:
                    return _owner.BaseStates.Climb.Ledge.HasWall ? "FallGrabBraced" : "StandGrab";
                case AutoGrabType.PoleClimb:
                    return "PipeGrab";
                case AutoGrabType.PoleSwing:
                    return "PoleSwingGrab";
                    //return _owner.AnimControl.Vertical > 0.25f ? "ReachToPoleSwing" : "PoleSwingGrab";
                case AutoGrabType.Wallclimb:
                    return "Grab";
                default:  // Normal ledge grab
                    {
                        if (_owner.AnimControl.IsIn("JumpU"))
                            return "StandGrab";
                        else if (!_owner.BaseStates.Climb.Ledge.HasWall)
                            return "DeepGrab";
                        else
                            return "GrabBT";
                        
                        /*else if (_owner.BaseStates.Climb.Ledge.HasWall && _reachDirection == 0)
                            return "HangFeetGrab";
                        else if (_owner.BaseStates.Climb.Ledge.HasWall && _reachDirection == -1)
                            return "HangFeetGrab90L";
                        else if (_owner.BaseStates.Climb.Ledge.HasWall && _reachDirection == 1)
                            return "HangFeetGrab90R";
                        else if (_owner.BaseStates.Climb.Ledge.HasWall && _reachDirection == 2)
                            return "HangFeetGrab45R";
                        else if (_owner.BaseStates.Climb.Ledge.HasWall && _reachDirection == -2)
                            return "HangFeetGrab45L";
                        else
                            return "DeepGrab";*/
                    }
            }
        }

        private PlayerState GetNextState()
        {
            switch (_grabType)
            {
                case AutoGrabType.LedgeJumpUp:
                    return _owner.BaseStates.Climb;
                case AutoGrabType.PoleClimb:
                    return _owner.BaseStates.PoleClimb;
                case AutoGrabType.PoleSwing:
                    return _owner.BaseStates.PoleSwing;
                case AutoGrabType.Wallclimb:
                    return _owner.BaseStates.Wallclimb;
                case AutoGrabType.Monkey:
                    return _owner.BaseStates.Monkey;
                default:  // Normal ledge grab
                    return _owner.BaseStates.Climb;
            }
        }
    }
}
