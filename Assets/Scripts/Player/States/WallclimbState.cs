using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class WallclimbState : RungClimbState, IRungCountable
    {
        public WallclimbSurface Surface { get; set; }

        public float RungPositionY => Surface.transform.position.y + Surface.Rungs.GetHeightAt(RungCount);
        public float NextPositionY => Surface.transform.position.y + Surface.Rungs.GetHeightAt(RungCount + 1);

        private bool _goingToLedge;

        public WallclimbState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _owner.Movement.Wall.CanGoUp = IsRungAboveFree;
            _owner.Movement.Wall.CanGoRight = IsSpaceRightFree;
            _owner.Movement.Wall.GetForward = () => Surface.transform.forward;

            _owner.AnimControl.IsWallclimb = true;
            _owner.AnimControl.ApplyRootMotion = true;

            _goingToLedge = false;

            Physics.IgnoreLayerCollision(8, 0);
            Physics.IgnoreLayerCollision(8, 9);
            Physics.IgnoreLayerCollision(8, 11);
        }

        public override void OnExit()
        {
            Physics.IgnoreLayerCollision(8, 0, false);
            Physics.IgnoreLayerCollision(8, 9, false);
            Physics.IgnoreLayerCollision(8, 11, false);
        }

        public override void Update()
        {
            if (_goingToLedge)
            {
                if (_owner.AnimControl.IsIn("Idle") || _owner.AnimControl.IsInTrans("ClimbUp -> Idle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
                }
            }
            else
            {
                if (_owner.AnimControl.IsIn("WallclimbIdle"))
                {
                    // Correct position
                    float targetHeight = Surface.transform.position.y + Surface.Rungs.GetHeightAt(RungCount);
                    Vector3 targetPosition = _owner.transform.position;
                    targetPosition.y = targetHeight + _owner.settings.wallclimbUpOffset;
                    _owner.LerpTo(targetPosition);

                    if (_owner.UInput.MoveInputRaw.z > 0.5f && GetTopRung() == RungCount)
                    {
                        if (Surface.FreeclimbUp) 
                        {
                            _owner.AnimControl.Play("WallclimbToFreeclimb");
                            _owner.BaseStates.Freeclimb.Surface = _owner.BaseStates.Climb.FreeclimbSurface = Surface.FreeclimbUp;
                            _owner.BaseStates.Freeclimb.RungCount = 1;
                            FreeclimbLedge ledge = FreeclimbLedge.Create(Surface.FreeclimbUp, _owner.BaseStates.Freeclimb.RungCount);
                            _owner.BaseStates.Climb.Ledge = ledge;
                            _owner.StateMachine.ChangeState(_owner.BaseStates.Climb);
                        }
                        else if (CanClimbUp())
                        {
                            _owner.AnimControl.Play("WallclimbOnLedge");
                            _goingToLedge = true;
                        }
                    }
                    else if (_owner.UInput.MoveInputRaw.z < -0.5f && RungCount == GetBottomRung())
                    {
                        if (Surface.FreeclimbDown)
                        {
                            _owner.AnimControl.Play("WallclimbToFreeclimbDown");
                            _owner.BaseStates.Freeclimb.Surface = Surface.FreeclimbDown;
                            _owner.BaseStates.Freeclimb.RungCount = Surface.FreeclimbDown.Rungs.GetTopRung();
                            _owner.StateMachine.ChangeState(_owner.BaseStates.Freeclimb);
                        }
                        else if (Surface.CanStepOff)
                        {
                            _owner.AnimControl.Play("WallclimbToIdle");
                            _goingToLedge = true;
                        }
                    }
                }

                _owner.Movement.Resolve();

                base.Update();
            }
        }

        private bool IsRungAboveFree(float amount)
        {
            if (amount > 0.0f)
            {
                // The stop anim does one more increment so account for this
                if (_owner.AnimControl.IsIn("WallclimbIdle"))
                {
                    return RungCount < Surface.Rungs.GetTopRung() - 2;
                }
                else
                {
                    return RungCount < Surface.Rungs.GetTopRung() - 3;
                }
            }
            else
            {
                return RungCount > GetBottomRung();
            }
        }

        private bool IsSpaceRightFree(float amount)
        {
            float offset = Mathf.Sign(amount) * 0.5f;
            return !Surface.IsOutsideSurface(_owner.transform.position + _owner.transform.right * offset);
        }

        private bool CanClimbUp()
        {
            return Surface.CanClimbOff && RungCount == Surface.Rungs.GetTopRung() - 2;
        }

        public override int GetTopRung()
        {
            return Surface.Rungs.GetTopRung() - 2;
        }

        public override int GetBottomRung()
        {
            return Surface.CanStepOff ? 2 : 0;
        }
    }
}
