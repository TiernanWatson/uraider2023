using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class LocomotionState : GroundState
    {
        public Vector3 WallclimbStartTarget { get; private set; }

        private const float MaxStepUpHeight = 2.0f;
        private const float HalfStepUpHeight = 1.5f;
        private const float QtrStepUpHeight = 1.0f;
        private const float StepUpRunDistance = 1.0f;
        private const float StepUpIdleDistance = 0.35f;

        private bool _stopTriggered;
        private bool _isClimbingOver;
        private bool _isRunningOver;
        private bool _startedMovingToObj;
        private bool _isGoingToDPipe;
        private bool _isGoingToTread;
        private Drainpipe _requestedDrainpipe;
        private Ladder _requestedLadder;
        private PushBlock _requestedPushBlock;
        private WallclimbSurface _requestedWallclimb;

        public LocomotionState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _stopTriggered = false;
            _isClimbingOver = false;
            _isRunningOver = false;
            _startedMovingToObj = false;
            _isGoingToDPipe = false;
            _isGoingToTread = false;

            _owner.Movement.Roam.TurnRate = _owner.settings.locomotionTurnRate;
        }

        public override void OnExit()
        {
            base.OnExit(); 

            _requestedDrainpipe = null;
            _requestedLadder = null;
            _requestedPushBlock = null;
            _requestedWallclimb = null;

            // In case jump is called while running over
            _owner.Movement.Motor.UseGroundingForce = true;
            _owner.Movement.Motor.OverrideGrounding(false);

            _owner.AnimControl.IsWading = false;
        }

        public override void Update()
        {
            if (_isGoingToTread)
            {
                _owner.StateMachine.ChangeState(_owner.BaseStates.Swim);
                return;
            }
            else if (_goingToGrab)
            {
                base.Update();
                return;
            }
            else if (_isGoingToDPipe)
            {
                if (!_owner.AnimControl.IsInTrans()) // Stop blend tree snapping
                {
                    _owner.BaseStates.Drainpipe.Pipe = _owner.Triggers.Drainpipe;
                    _owner.BaseStates.Drainpipe.RungCount = 1;
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Drainpipe);
                }
                return;
            }

            bool isMoving = _owner.Movement.Velocity.sqrMagnitude > 0.01f;

            bool hitStepThisFrame = 
                SweepHitSteps(out RaycastHit forwardStepHit, 
                out RaycastHit downStepHit, 
                out float stepHeight) 
                && isMoving;

            // Disconnected so player can rotate while running over
            if (_isRunningOver && !_owner.AnimControl.IsTargetMatching)
            {
                _isRunningOver = false;
                _owner.Movement.Motor.UseGroundingForce = true;
                _owner.Movement.Motor.OverrideGrounding(false);
            }

            if (_isClimbingOver)
            {
                if (_owner.AnimControl.IsInTrans("WindowVault -> FallBlend"))
                {
                    _owner.Movement.Motor.OverrideGrounding(false);
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
                }
                // Target matching will be false when finished anim
                else if (!_owner.AnimControl.IsTargetMatching)
                {
                    _isClimbingOver = false;
                    _owner.Movement.Motor.UseGroundingForce = true;
                    _owner.Movement.Motor.OverrideGrounding(false);
                }

                // Stop edging forward when finished
                _owner.Movement.Resolve();
            }
            else
            {
                // Check for all possible climbable surfaces to transition to first
                if (_owner.Triggers.Drainpipe && CanUseDrainpipe(_owner.Triggers.Drainpipe))
                {
                    Vector3 targetPosition = _owner.settings.GetDPipeStartPos(_owner.Triggers.Drainpipe);
                    Quaternion targetRot = _owner.Triggers.Drainpipe.transform.rotation;
                    MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.one, 1.0f);
                    _owner.AnimControl.TargetMatchState("StanceToDPipe", targetPosition, targetRot, mask, 0.1f, 0.99f);
                    _owner.AnimControl.FadeTo("StanceToDPipe", 0.2f);
                    _startedMovingToObj = true;
                    _isGoingToDPipe = true;
                }
                else if (_owner.Triggers.Ladder && CanUseLadder(_owner.Triggers.Ladder))
                {
                    // Going to straight to ladder state would set speed to 0, making ugly transition
                    if (!_startedMovingToObj)
                    {
                        _owner.BaseStates.Ladder.Ladder = _owner.Triggers.Ladder;
                        Vector3 targetPosition = _owner.settings.GetLadderPos(_owner.Triggers.Ladder.transform.position + Vector3.up * 0.3125f, _owner.Triggers.Ladder.transform.forward);
                        Quaternion targetRot = _owner.Triggers.Ladder.transform.rotation;
                        MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.one, 1.0f);
                        _owner.AnimControl.TargetMatchState("IdleToLadder", targetPosition, targetRot, mask, 0.1f, 0.99f);
                        _owner.AnimControl.FadeTo("IdleToLadder", 0.2f, 0.1f);
                        _owner.Movement.Motor.UseGroundingForce = false;
                        _startedMovingToObj = true;
                    }
                    else if (!_owner.AnimControl.IsInTrans())
                    {
                        _owner.StateMachine.ChangeState(_owner.BaseStates.Ladder);
                    }
                }
                else if (_owner.Triggers.Wallclimb && CanUseWallclimb(_owner.Triggers.Wallclimb))
                {
                    if (!_startedMovingToObj)
                    {
                        _owner.BaseStates.Wallclimb.Surface = _owner.Triggers.Wallclimb;
                        _owner.AnimControl.FadeTo("IdleToWallclimb", 0.2f);
                        _owner.Movement.Motor.UseGroundingForce = false;
                        _startedMovingToObj = true;
                    }
                    else if (!_owner.AnimControl.IsInTrans()) // Finished auto move
                    {
                        _owner.BaseStates.Wallclimb.RungCount = 2;
                        _owner.StateMachine.ChangeState(_owner.BaseStates.Wallclimb);
                    }
                }
                else if (_requestedPushBlock)
                {
                    _owner.AnimControl.FadeTo("BlockPushIdle", 0.2f);
                    _owner.BaseStates.BlockPush.PushBlock = _requestedPushBlock;
                    _owner.StateMachine.ChangeState(_owner.BaseStates.BlockPush);
                }
                else if (_owner.UInput.Jump.triggered)
                {
                    bool vaulted = _isClimbingOver = TryVault();

                    if (!vaulted)
                    {
                        if (!_isRunningOver && !_isClimbingOver && hitStepThisFrame)
                        {
                            AttemptStepUp(forwardStepHit, downStepHit, stepHeight, false);
                        }

                        if (!_isClimbingOver)
                        {
                            _owner.StateMachine.ChangeState(_owner.BaseStates.Jump);
                        }
                    }
                }
                else if (_owner.UInput.Crouch.triggered && _owner.AnimControl.IsTag("Moving"))
                {
                    Vector3 rayStart = _owner.transform.position + _owner.transform.forward * 0.5f;
                    Vector3 rayDirection = -_owner.transform.forward;

                    if (LedgePoint.FindLedge(rayStart, rayDirection, 1.0f, _owner.settings.ledgeLayers, out LedgePoint ledge))
                    {
                        if (Vector3.Angle(ledge.transform.forward, rayDirection) < 80.0f)
                        {
                            _owner.SetIgnoreCollision(true);
                            _owner.Movement.Motor.UseGroundingForce = false;
                            _goingToGrab = true;
                            _owner.BaseStates.Climb.ReceiveLedge(ledge);
                            _owner.AnimControl.Play("StanceToLedge");
                        }
                    }

                    if (!_goingToGrab)
                    {
                        _owner.AnimControl.IdleToCrouch();
                        _owner.StateMachine.ChangeState(_owner.BaseStates.Crouch);
                    }
                }
                else if (_owner.EquipedMachine.State == _owner.EquipedStates.Combat)
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Strafe);
                }
                else
                {
                    if (!_isClimbingOver && !_isRunningOver && hitStepThisFrame)
                    {
                        //TryRunOver(forwardStepHit, downStepHit, stepHeight);
                    }
                    
                    base.Update();
                }
            }
        }

        public override void UpdateAnimation(PlayerAnim animControl)
        {
            base.UpdateAnimation(animControl);

            // Allows player to have more fine control of movement with dynamic offset
            bool goingToRun = _owner.AnimControl.IsIn("IdleToRun") || _owner.AnimControl.IsIn("Idle -> IdleToRun");
            if (goingToRun && animControl.NormTime > 0.4f)
            {
                if (!_stopTriggered && animControl.TargetSpeed < 1.0f)
                {
                    float offset = Mathf.Lerp(0.5f, 0.0f, animControl.NormTime);
                    animControl.FadeTo("RunToIdleM", 0.15f, offset);
                    _stopTriggered = true;
                }
            }
            else if (goingToRun && animControl.NormTime < 0.4f)
            {
                if (!_stopTriggered && animControl.TargetSpeed < 1.0f)
                {
                    animControl.FadeTo("Idle", 0.1f);
                    _stopTriggered = true;
                }
            }
            else
            {
                _stopTriggered = false;
            }
        }

        public override void OnWaterStay(WaterZone volume)
        {
            base.OnWaterStay(volume);

            Ray depthRay = new Ray(_owner.transform.position, Vector3.down);
            if (Physics.Raycast(depthRay, out RaycastHit hit, _owner.settings.treadYOffset, _owner.settings.groundLayers, QueryTriggerInteraction.Ignore))
            {
                float depth = volume.transform.position.y + volume.SurfaceHeight - hit.point.y;
                _owner.AnimControl.IsWading = depth > _owner.settings.minWadeDepth;

                if (depth > _owner.settings.maxWadeDepth)
                {
                    float targetHeight = volume.transform.position.y + volume.SurfaceHeight - _owner.settings.treadYOffset;
                    _owner.BaseStates.Swim.Zone = volume;
                    MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.up, 0.0f);
                    _owner.AnimControl.TargetMatchState("WadeToSurf", Vector3.up * targetHeight, Quaternion.identity, mask, 0.1f, 0.99f);
                    _owner.AnimControl.FadeTo("WadeToSurf", 0.15f);
                    _isGoingToTread = true;
                }
            }
        }

        private bool TryVault()
        {
            bool vaulted = false;
            Vault vault = _owner.Vaults.GetClosest();

            if (!_isRunningOver && vault)
            {
                float angleForward = Vector3.SignedAngle(_owner.transform.forward, vault.transform.forward, Vector3.up);
                float absAngle = Mathf.Abs(angleForward);

                if (absAngle < 30.0f || absAngle > 150.0f)
                {
                    float closestT = vault.Collider.Point.ClosestParamTo(_owner.transform.position, _owner.transform.forward);
                    Vector3 closestPoint = vault.Collider.Point.GetPoint(closestT);
                    Debug.DrawRay(closestPoint, Vector3.up, Color.red, 2.0f);

                    Quaternion targetRot = vault.transform.rotation;
                    if (absAngle > 150.0f)
                    {
                        targetRot = Quaternion.Euler(0.0f, 180.0f, 0.0f) * targetRot;
                    }

                    MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.right, 1.0f);
                    _owner.AnimControl.TargetMatchState("WindowVault", closestPoint, targetRot, mask, 0.1f, 0.25f);
                    _owner.AnimControl.FadeTo("WindowVault", 0.1f);
                    _owner.Movement.Motor.UseGroundingForce = false;

                    if (absAngle < 30.0f && !vault.GroundForward)
                    {
                        _owner.Movement.Motor.OverrideGrounding(true, false);
                    }
                    else
                    {
                        _owner.Movement.Motor.OverrideGrounding(true);
                    }

                    vaulted = true;
                }
            }

            return vaulted;
        }

        public void ReceivePushBlock(PushBlock block)
        {
            _requestedPushBlock = block;
        }

        public override bool CanUseButton(WallButton button)
        {
            return Vector3.Angle(button.transform.forward, _owner.transform.forward) < 45.0f
                && _owner.AnimControl.IsTag("Moving") || _owner.AnimControl.IsTag("Idle");
        }

        public override void TriggerButton(WallButton button)
        {
            base.TriggerButton(button);

            Vector3 targetPosition = button.transform.position - button.transform.forward * 0.45f;
            targetPosition.y = _owner.transform.position.y;
            targetPosition -= button.transform.right * 0.15f;
            Quaternion targetRotation = button.transform.rotation;

            _owner.AnimControl.FadeTo("PushButton", 0.1f);
            _owner.AnimControl.TargetMatchState("PushButton", targetPosition, targetRotation, 0.01f, 0.25f);
            _owner.AnimControl.HandInteract += () => button.Push();
        }

        public override bool CanPushBlock(PushBlock block)
        {
            bool okAnim = _owner.AnimControl.IsTag("Moving") || _owner.AnimControl.IsTag("Idle");

            Vector3 axis = -block.GetBestAxis(_owner.transform.position);

            return okAnim && Vector3.Angle(axis, _owner.transform.forward) < 45.0f;
        }

        public override bool CanUseDoor(Door door)
        {
            if (!_owner.AnimControl.IsIn("TryDoor") && !_owner.AnimControl.IsIn("UseCrowbar"))
            {
                Vector3 localPosition = door.transform.InverseTransformPoint(_owner.transform.position);
                bool inFront = localPosition.z < 0.0f;
                if (inFront)
                {
                    float angle = Vector3.Angle(_owner.transform.forward, door.transform.forward);
                    return angle < 45.0f;
                }
                else
                {
                    float angle = Vector3.Angle(-_owner.transform.forward, door.transform.forward);
                    return angle < 45.0f;
                }
            }

            return false;
        }

        public override bool CanUseLock(Lock locked)
        {
            float angle = Vector3.Angle(_owner.transform.forward, locked.transform.forward);
            return angle < 45.0f;
        }

        public override bool CanPickup(InventoryItem item)
        {
            return _owner.AnimControl.IsTag("Moving");
        }

        public override void TriggerPickup(InventoryItem item)
        {
            base.TriggerPickup(item);

            _owner.AnimControl.PickUp += () => item.Pickup(_owner);
            _owner.AnimControl.FadeTo("PickUp", 0.1f);
        }

        public override void TriggerDoor(Door door)
        {
            base.TriggerDoor(door);

            Vector3 localPosition = door.transform.InverseTransformPoint(_owner.transform.position);
            bool inFront = localPosition.z < 0.0f;

            string anim;
            float startTime = 0.02f;
            float endTime = 0.2f;
            Vector3 targetPos;
            Quaternion targetRot = inFront ? door.transform.rotation : Quaternion.LookRotation(-door.transform.forward);

            if (door.IsLocked)
            {
                startTime = 0.05f;

                Vector3 forward = inFront ? door.transform.forward : -door.transform.forward;

                if (door.Key && _owner.Inventory.Contains(door.Key, out var details))
                {
                    anim = "UseKey";
                    targetPos = _owner.settings.GetUseKey(door.transform.position, forward, door.transform.right);
                    door.Unlock();
                }
                else if (door.CanUseCrowbar && _owner.Inventory.Contains("Crowbar", out var detail))
                {
                    anim = "UseCrowbar";
                    targetPos = _owner.settings.GetCrowbar(door.transform.position, forward, door.transform.right);
                    door.Unlock();
                }
                else
                {
                    anim = inFront ? "TryDoorR" : "TryDoorL";
                    targetPos = _owner.settings.GetTryDoor(door.transform.position, forward, door.transform.right);
                }
            }
            else
            {
                if (inFront)
                {
                    if (door.IsPush)
                    {
                        anim = "PushDoorR";
                        targetPos = _owner.settings.GetPushDoor(door.transform.position, door.transform.forward, door.transform.right);
                        _owner.AnimControl.HandInteract += () => door.OpenPush();
                    }
                    else
                    {
                        anim = "PullDoorR";
                        targetPos = _owner.settings.GetPullDoor(door.transform.position, door.transform.forward, door.transform.right);
                        _owner.AnimControl.HandInteract += () => door.OpenPull();
                    }
                }
                else
                {
                    if (door.IsPush)
                    {
                        anim = "PullDoorL";
                        targetPos = _owner.settings.GetPullDoor(door.transform.position, -door.transform.forward, door.transform.right);
                        _owner.AnimControl.HandInteract += () => door.OpenPush();
                    }
                    else
                    {
                        anim = "PushDoorL";
                        targetPos = _owner.settings.GetPushDoor(door.transform.position, -door.transform.forward, door.transform.right);
                        _owner.AnimControl.HandInteract += () => door.OpenPull();
                    }
                }

                endTime = 0.1f;
            }

            _owner.AnimControl.FadeTo(anim, 0.1f);
            _owner.AnimControl.TargetMatchState(anim, targetPos, targetRot, startTime, endTime);
        }

        public bool CanUseLadder(Ladder ladder)
        {
            float angle = Vector3.Angle(_owner.transform.forward, ladder.transform.forward);
            Vector3 input = _owner.GetCameraRotater() * _owner.MoveInput;
            float inputAngle = Vector3.Angle(input, ladder.transform.forward);

            return _owner.UInput.MoveInputRaw.sqrMagnitude > 0.01f &&
                angle < 30.0f && inputAngle < 30.0f;
        }
        public bool CanUseWallclimb(WallclimbSurface surface)
        {
            float angle = Vector3.Angle(_owner.transform.forward, surface.transform.forward);
            Vector3 input = _owner.GetCameraRotater() * _owner.MoveInput;
            float inputAngle = Vector3.Angle(input, surface.transform.forward);

            return _owner.UInput.MoveInputRaw.sqrMagnitude > 0.01f &&
                angle < 30.0f && inputAngle < 30.0f;
        }


        public bool CanUseDrainpipe(Drainpipe pipe)
        {
            if (_owner.Movement.TargetVelocity.sqrMagnitude < 0.01f)
            {
                return false;
            }

            float angle = Vector3.Angle(_owner.transform.forward, _owner.Triggers.Drainpipe.transform.forward);
            float targetAngle = Vector3.Angle(_owner.Movement.TargetVelocity, _owner.Triggers.Drainpipe.transform.forward);
            float heightDiff = _owner.Triggers.Drainpipe.transform.position.y - _owner.transform.position.y;

            return _owner.Velocity.sqrMagnitude > 0.01f && Mathf.Abs(heightDiff) < 0.5f && angle < 30.0f && targetAngle < 30.0f;
        }

        public override void TriggerLadder(Ladder ladder)
        {
            float angle = Vector3.Angle(_owner.transform.forward, ladder.transform.forward);
            Vector3 input = _owner.GetCameraRotater() * _owner.MoveInput;
            float inputAngle = Vector3.Angle(input, ladder.transform.forward);

            if (_owner.UInput.MoveInputRaw.sqrMagnitude > 0.01f && 
                angle < 30.0f && inputAngle < 30.0f)
            {
                _owner.BaseStates.Ladder.Ladder = ladder;
                _requestedLadder = ladder;
            }
        }

        public override void TriggerWallclimb(WallclimbSurface surface)
        {
            _owner.BaseStates.Wallclimb.Surface = surface;
            _requestedWallclimb = surface;
        }

        private bool SweepHitSteps(out RaycastHit hit, out RaycastHit downHit, out float height)
        {
            if (_owner.SweepCapsule(_owner.transform.position, _owner.Forward * StepUpRunDistance, out hit)) 
            {
                float angle = Vector3.Angle(_owner.transform.forward, -hit.normal);
                if (angle < 20.0f)
                {
                    Vector3 rayStart = hit.point;
                    rayStart.y = _owner.transform.position.y + MaxStepUpHeight;
                    rayStart += _owner.transform.forward * _owner.Movement.Motor.CharControl.radius;

                    Ray ray = new Ray(rayStart, -_owner.transform.up);

                    if (Physics.Raycast(ray, out downHit, MaxStepUpHeight, _owner.settings.groundLayers, QueryTriggerInteraction.Ignore) && IsValidStep(downHit, out height))
                    {
                        Vector3 edgePos = hit.point;
                        edgePos.y = downHit.point.y;

                        return !_owner.CheckCapsule(edgePos);
                    }
                }
            }

            hit = new RaycastHit();
            downHit = new RaycastHit();
            height = 0.0f;

            return false;
        }

        private bool TryRunOver(RaycastHit hit, RaycastHit downHit, float height)
        {
            if (_owner.AnimControl.IsIn("Idle") || hit.collider.CompareTag("PushBlock"))
            {
                return false;
            }

            if (hit.distance > StepUpIdleDistance - 0.24f)
            {
                return false;
            }

            Vector3 targetPosition = hit.point;
            targetPosition.y = downHit.point.y;
            targetPosition.y += _owner.Movement.Motor.CharControl.skinWidth;

            if (height < QtrStepUpHeight)
            {
                if (_owner.Movement.Velocity.magnitude < 2.0f)
                {
                    _owner.AnimControl.StepUpQtrWalk(targetPosition);
                }
                else
                {
                    _owner.AnimControl.StepUpQtr(targetPosition);
                }
                _owner.Movement.Motor.UseGroundingForce = false;
                _owner.Movement.Motor.OverrideGrounding(true);
                _isRunningOver = true;

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AttemptStepUp(RaycastHit hit, RaycastHit downHit, float height, bool ignorePushBlock = true)
        {
            if (ignorePushBlock && hit.collider.gameObject.layer == 11)
            {
                return false;
            }

            if (hit.distance > StepUpIdleDistance - 0.24f)
            {
                return false;
            }

            Vector3 targetPosition = hit.point;
            targetPosition.y = downHit.point.y;

            if (height < HalfStepUpHeight)
            {
                _owner.AnimControl.StepUpHalfIdle(targetPosition);
            }
            else if (height < MaxStepUpHeight)
            {
                _owner.AnimControl.StepUpMaxIdle(targetPosition);
            }
            else
            {
                return false;
            }

            _owner.Movement.Motor.UseGroundingForce = false;
            _isClimbingOver = true;

            return true;
        }

        private bool IsValidStep(RaycastHit downHit, out float height)
        {
            height = downHit.point.y - _owner.transform.position.y;
            float angle = Vector3.Angle(downHit.normal, _owner.transform.up);

            bool okHeight = height > _owner.Movement.Motor.StepOffset;
            bool okAngle = angle < _owner.Movement.Motor.CharControl.slopeLimit;

            return okAngle && okHeight;
        }
    }
}
