using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    public class ClimbState : PlayerState
    {
        //public LedgePoint Ledge
        public IShimmyable Ledge
        {
            get { return _ledge; }
            set
            {
                _ledge = value;
                _owner.Movement.Climb.Ledge = value;
            }
        }

        public IShimmyable HopUpLedge
        {
            get { return _hopUpLedge; }
        }

        public FreeclimbSurface FreeclimbSurface { get; set; }

        private bool _isCornering;
        private bool _didCornerAnim;
        private bool _isClimbingUp;
        private bool _handstandRequested;
        private bool _climbUpRequested;
        private bool _goingToHopUp;
        private IShimmyable _ledge;
        private IShimmyable _nextLedge;
        private LedgePoint _hopUpLedge;

        public ClimbState(PlayerController owner) : base(owner)
        {
        }

        public void ReceiveLedge(IShimmyable ledge)
        {
            Ledge = ledge;
        }

        public override void OnEnter()
        {
            if (_goingToHopUp)
            {
                _ledge = _hopUpLedge; // For our return
            }

            _isCornering = false;
            _didCornerAnim = false;
            _isClimbingUp = false;
            _handstandRequested = false;
            _climbUpRequested = false;
            _goingToHopUp = false;

            // TODO: FIX HANDSTAND

            _owner.UInput.ClimbUp.performed += Handstand;
            _owner.UInput.ClimbUp.canceled += ClimbUp;
            //_owner.UInput.Jump.performed += ClimbUp;

            _owner.Movement.GoToClimb(_ledge);

            // Stop character jerking out of walls
            _owner.SetIgnoreCollision(true);
        }

        public override void OnExit()
        {
            //_owner.UInput.ClimbUp.performed -= Handstand;
            //_owner.UInput.ClimbUp.canceled -= ClimbUp;
            //_owner.UInput.Jump.performed -= ClimbUp;

            // If jumping to another ledge, don't want to jerk the character out of the wall
            if (!_goingToHopUp)
            {
                _owner.SetIgnoreCollision(false);
            }
        }

        public override void Update()
        {
            _owner.Movement.Resolve();

            if (_goingToHopUp)
            {
                if (_owner.AnimControl.IsIn("LedgeJump"))
                {
                    float t = _hopUpLedge.ClosestParamTo(_owner.transform.position, _owner.transform.forward);
                    Vector3 point = _hopUpLedge.GetPoint(t);
                    _owner.BaseStates.AutoGrabLedge.ReceiveContext(AutoGrabType.LedgeJumpUp, point, _owner.transform.forward, 0.1f);
                    _owner.StateMachine.ChangeState(_owner.BaseStates.AutoGrabLedge);
                    _owner.SFX.PlayJump();
                }
            }
            else if (_isClimbingUp)
            {
                if (_owner.AnimControl.IsIn("Idle") || _owner.AnimControl.IsInTrans("ClimbUp -> Idle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
                }
                else if (_owner.AnimControl.IsIn("CrouchIdle") || _owner.AnimControl.IsInTrans("ClimbUp -> Idle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Crouch);
                }
            }
            else if (_isCornering)
            {
                if (!_didCornerAnim)
                {
                    if (InCornerAnim())
                    {
                        _didCornerAnim = true;
                    }
                }
                else if (InIdleAnim())
                {
                    _isCornering = false;
                    _didCornerAnim = false;
                    _owner.SetRotation(Quaternion.LookRotation(_nextLedge.Forward));
                   Ledge = _nextLedge;
                }
            }
            else if (_climbUpRequested)
            {
                if (_owner.AnimControl.IsIn("SideLook") || _owner.AnimControl.IsIn("SideLookR"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Jump);
                }
                else if (CanClimbUp())
                {
                    _owner.AnimControl.IsCrouching = Ledge.ClimbUp == ClimbUpType.Crouch;

                    if (_owner.AnimControl.IsIn("HangIdle"))
                    {
                        _owner.AnimControl.Play("ClimbUp");
                    }
                    else
                    {
                        _owner.AnimControl.FadeTo("ClimbUp", 0.15f);
                    }

                    _isClimbingUp = true;
                }
                else if (CanJumpUp())
                {
                    Vector3 startPoint = GetClosestPoint() - _owner.transform.forward * _owner.settings.ledgeJumpUpMaxDepth;
                    float rayDistance = _owner.settings.ledgeJumpUpMaxDepth * 2.0f + Mathf.Epsilon;
                    LedgePoint ledge = LedgePoint.FindLedgeInRange(startPoint, _owner.transform.forward, rayDistance, _owner.settings.ledgeJumpUpMaxDistance, 30.0f, _owner.settings.ledgeLayers, 0.25f, _ledge as LedgePoint);
                    _hopUpLedge = ledge ? ledge : _ledge as LedgePoint;
                    _owner.AnimControl.Play("HangToJumpU");
                    _goingToHopUp = true;
                }
                else if (CanJumpBack())
                {
                    _owner.AnimControl.Play("HangBackJump");
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Jump);
                }
                else if (!_owner.AnimControl.IsIn("Grab"))
                {
                    // Want to keep climb up request for after grab animation
                    _climbUpRequested = false;
                }
            }
            else if (_handstandRequested)
            {
                if (_owner.AnimControl.IsIn("HangIdle") && Ledge.ClimbUp == ClimbUpType.Stand)
                {
                    _owner.AnimControl.Play("Handstand");
                    _isClimbingUp = true;
                }
                else if (!_owner.AnimControl.IsIn("Grab"))
                {
                    _handstandRequested = false;
                }

            }
            else if (_owner.UInput.Crouch.triggered)
            {
                Vector3 startPoint = GetClosestPoint() - _owner.transform.forward * _owner.settings.ledgeJumpUpMaxDepth
                    + Vector3.down * 3.0f;
                float rayDistance = _owner.settings.ledgeJumpUpMaxDepth * 2.0f + Mathf.Epsilon;
                LedgePoint ledge = LedgePoint.FindLedgeInRange(startPoint, _owner.transform.forward, rayDistance, 2.5f, 30.0f, _owner.settings.ledgeLayers, 0.25f, _ledge as LedgePoint);
                if (ledge)
                {
                    string animName = _ledge.HasWall ? "FallGrabPose" : "JumpU";
                    float t = ledge.ClosestParamTo(_owner.transform.position, _owner.transform.forward);
                    Vector3 point = ledge.GetPoint(t);
                    _ledge = ledge; // For our return
                    _owner.AnimControl.FadeTo(animName, 0.15f);
                    _owner.BaseStates.AutoGrabLedge.ReceiveContext(AutoGrabType.LedgeDropDown, point, ledge.Forward, 0.0f);
                    _owner.StateMachine.ChangeState(_owner.BaseStates.AutoGrabLedge);
                }
                else
                {
                    // Stop character getting stuck in wall
                    Vector3 noIntersection = _owner.transform.position - _owner.transform.forward * _owner.CharControl.radius;
                    _owner.SetPosition(noIntersection);
                    _owner.AnimControl.FadeTo("FallBlend", 0.1f);
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
                }
            }
            else
            {
                if (InIdleAnim() || InMoveAnim())
                {
                    // Stop any error from building up
                    Vector3 correctPosition = _owner.settings.GetLedgePosition(GetClosestPoint(), Ledge.Forward);
                    _owner.LerpTo(correctPosition);

                    // Check for cornering
                    if (_owner.Movement.Velocity.sqrMagnitude > 0.01f)
                    {
                        float dotMoveLedgeRight = Vector3.Dot(_owner.Movement.Velocity.normalized, Ledge.Right);
                        bool isGoingRight = dotMoveLedgeRight > 0.0f;
                        bool isGoingLeft = dotMoveLedgeRight < 0.0f;

                        if (isGoingRight)
                        {
                            CheckForCorners(goingRight: true);
                        }
                        else if (isGoingLeft)
                        {
                            CheckForCorners(goingRight: false);
                        }
                    }

                    // Check if we should go to freeclimb
                    TryGoToFreeclimb();
                }
            }
        }

        private bool CanJumpBack()
        {
            return _owner.AnimControl.IsIn("FeetHangBack") || _owner.AnimControl.IsIn("ToBackLook");
        }

        private bool CanJumpUp()
        {
            return _owner.AnimControl.IsIn("HangToBraced") && _ledge.HasWall && Ledge.ClimbUp == ClimbUpType.Blocked;
        }

        private bool CanClimbUp()
        {
            return (_owner.AnimControl.IsIn("HangIdle") || _owner.AnimControl.IsIn("HangFeetIdle") || _owner.AnimControl.IsIn("HangToBraced"))
                                && Ledge.ClimbUp != ClimbUpType.Blocked;
        }

        public override void UpdateAnimation(PlayerAnim animControl)
        {
            base.UpdateAnimation(animControl);

            animControl.HasLegRoom = _ledge.HasWall;
        }

        public override void OnDeath()
        {
            base.OnDeath();

            _owner.StateMachine.ChangeState(_owner.BaseStates.Dead);
        }

        private Vector3 GetClosestPoint()
        {
            float t = Ledge.ClosestParamTo(_owner.transform.position, _owner.transform.forward);
            return Ledge.GetPoint(t);
        }

        private bool InMoveAnim()
        {
            return _owner.AnimControl.IsTag("Moving");
        }

        private bool InIdleAnim()
        {
            return _owner.AnimControl.IsIn("HangIdle") || _owner.AnimControl.IsIn("HangFeetIdle");
        }

        private bool InTransToCorner()
        {
            return _owner.AnimControl.IsInTrans("HangIdle -> CornerOutRight")
                || _owner.AnimControl.IsInTrans("HangIdle -> CornerOutLeft")
                || _owner.AnimControl.IsInTrans("HangIdle -> CornerInRight")
                || _owner.AnimControl.IsInTrans("HangIdle -> CornerInLeft");
        }

        private bool InCornerAnim()
        {
            return _owner.AnimControl.IsTag("Cornering");
        }

        private void CheckForCorners(bool goingRight)
        {
            _nextLedge = goingRight ? Ledge.Next : Ledge.Previous;

            float t = Ledge.ClosestParamTo(_owner.transform.position, _owner.transform.forward);
            float distance = goingRight ? Ledge.GetMaxT() - t : t;

            bool isEnd = goingRight ? Ledge.Next == null || Ledge.Next.IsEnd : Ledge.IsStart;

            Debug.Log("IsEnd: " + isEnd + " Distance: " + distance);

            _owner.AnimControl.IsLedgeEnd = false;

            if (isEnd)
            {
                if (distance < _owner.settings.ledgeEndPadding)
                {
                    _owner.AnimControl.IsLedgeEnd = true;
                    _owner.Movement.SetVelocity(Vector3.zero);
                }
            }
            else if (CanCorner(goingRight, Ledge, _nextLedge, out bool onOutside))
            {
                if (onOutside)
                {
                    if (distance < _owner.settings.ledgeCornerPadding)
                    {
                        string anim = goingRight ? "CornerOutRight" : "CornerOutLeft";
                        _owner.AnimControl.FadeTo(anim, 0.1f);
                        _isCornering = true;
                    }
                }
                else
                {
                    if (distance < _owner.settings.ledgeCornerInPadding)
                    {
                        string anim = goingRight ? "CornerInRight" : "CornerInLeft";
                        _owner.AnimControl.FadeTo(anim, 0.1f);
                        _isCornering = true;
                    }
                }
            }
            else if (Ledge.IsBeyondEnd(GetClosestPoint()))
            { 
                Ledge = _nextLedge;
            }
        }

        private bool CanCorner(bool goingRight, IShimmyable current, IShimmyable next, out bool onOutside)
        {
            if (next == null)
            {
                onOutside = true;
                return false;
            }

            float angle = Vector3.SignedAngle(current.Forward, next.Forward, Vector3.up);

            onOutside = angle < 0.0f;
            if (!goingRight)
            {
                onOutside = !onOutside;
            }

            return Mathf.Abs(angle) > 60.0f;
        }

        private void TryGoToFreeclimb()
        {
            if (InIdleAnim() && FreeclimbSurface)
            {
                if (Mathf.Abs(_owner.UInput.MoveInputRaw.z) > 0.25f)
                {
                    if (FreeclimbSurface.WallclimbDown
                        && _owner.UInput.MoveInputRaw.z < 0.0f
                        && _owner.BaseStates.Freeclimb.RungCount == 1)
                    {
                        int targetRung = FreeclimbSurface.WallclimbDown.Rungs.GetTopRung() - 2;
                        Vector3 rungPosition = FreeclimbSurface.WallclimbDown.transform.position + Vector3.up * FreeclimbSurface.WallclimbDown.Rungs.GetHeightAt(targetRung);
                        rungPosition -= FreeclimbSurface.WallclimbDown.transform.forward * _owner.settings.wallclimbBackOffset;

                        Vector3 sideways = Vector3.Project(_owner.transform.position, FreeclimbSurface.WallclimbDown.transform.right);
                        Vector3 upwards = Vector3.ProjectOnPlane(rungPosition, FreeclimbSurface.WallclimbDown.transform.right);
                        Vector3 targetPosition = sideways + upwards;

                        _owner.AnimControl.Play("HangToWallclimbGrab");
                        _owner.AnimControl.TargetMatchState("HangToWallclimbGrab", targetPosition, FreeclimbSurface.WallclimbDown.transform.rotation, 0.1f, 0.99f);
                        _owner.BaseStates.Wallclimb.Surface = FreeclimbSurface.WallclimbDown;
                        _owner.BaseStates.Wallclimb.RungCount = targetRung;
                        _owner.StateMachine.ChangeState(_owner.BaseStates.Wallclimb);
                    }
                    else
                    {
                        _owner.AnimControl.Play("HangToFreeclimb");
                        _owner.StateMachine.ChangeState(_owner.BaseStates.Freeclimb);
                    }
                }
            }
        }

        private void ClimbUp(InputAction.CallbackContext ctx)
        {
            // Work around for weird Unity unsubscribe bug
            if (_owner.StateMachine.State == this)
            {
                _climbUpRequested = true;
            }
        }

        private void Handstand(InputAction.CallbackContext ctx)
        {
            // Work around for weird Unity unsubscribe bug
            if (_owner.StateMachine.State == this)
            {
                _handstandRequested = true;

            }
        }
    }
}
