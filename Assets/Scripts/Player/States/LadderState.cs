using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class LadderState : RungClimbState, IRungCountable
    {
        public Ladder Ladder { get; set; }

        private bool _goingToLocomotion;

        public LadderState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (_owner.StateMachine.LastState == _owner.BaseStates.Locomotion)
            {
                RungCount = 1;
            }

            _owner.Movement.Wall.CanGoUp = CanGoUp;
            _owner.Movement.Wall.CanGoRight = (amount) => false;
            _owner.Movement.Wall.GetForward = () => Ladder.transform.forward;

            _owner.AnimControl.ApplyRootMotion = true;
            _owner.AnimControl.CanClimbUp = Ladder.CanClimbOff;
            _owner.AnimControl.CanClimbDown = Ladder.CanStepOff;

            _goingToLocomotion = false;
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            if (ShouldTransitionToLocomotion())
            {
                _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
                _owner.Movement.Resolve();
            }
            else if (!_goingToLocomotion)
            {
                if (_owner.AnimControl.IsIn("LadderIdle"))
                {
                    Vector3 closestPoint2 = Ladder.transform.position;

                    Vector3 targetPosition = closestPoint2
                        - Ladder.transform.forward * _owner.settings.ladderBackOffset
                        + Vector3.up * Ladder.Rungs.GetHeightAt(RungCount)
                        + Vector3.up * _owner.settings.ladderUpOffset;

                    _owner.LerpTo(targetPosition);

                    if (_owner.UInput.MoveInputRaw.x > 0.1f)
                    {
                        if (Ladder.LedgeAt(RungCount))
                        {
                            _owner.AnimControl.FadeTo("LadderOffRight", 0.1f);
                            _goingToLocomotion = true;
                        }
                    }
                    else if (_owner.UInput.MoveInputRaw.x < -0.1f)
                    {
                        if (Ladder.LedgeAt(-RungCount))
                        {
                            _owner.AnimControl.FadeTo("LadderOffLeft", 0.1f);
                            _goingToLocomotion = true;
                        }
                    }
                }
            }

            _owner.Movement.Resolve();

            base.Update();
        }

        private bool ShouldTransitionToLocomotion()
        {
            return _owner.AnimControl.IsIn("Idle") || _owner.AnimControl.IsInTrans("LadderOffRight -> Idle") || _owner.AnimControl.IsInTrans("LadderOffLeft -> Idle");
        }

        public override void RungIncrement(int amount)
        {
            base.RungIncrement(amount);

            if (_owner.AnimControl.IsIn("LadderUp") || _owner.AnimControl.IsIn("LadderUp 0"))
            {
                if (_owner.AnimControl.NormTime < 0.5f)
                {
                    int nextRung = RungCount + 1;

                    Vector3 targetPosition = Ladder.transform.position
                        + Vector3.up * Ladder.Rungs.GetHeightAt(nextRung);
                    targetPosition = _owner.settings.GetLadderPos(targetPosition, Ladder.transform.forward);

                    MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.up, 0.0f);

                    // Had to duplicate ladder up state to avoid unity target match bug
                    string name = _owner.AnimControl.IsIn("LadderUp") ? "LadderUp" : "LadderUp 0";
                    _owner.AnimControl.TargetMatch(targetPosition, Quaternion.identity, mask, 0.5f, 0.99f);
                }
            }
        }

        private bool CanGoUp(float amount)
        {
            if (amount > 0.1f)
            {
                if (Ladder.CanClimbOff)
                {
                    return true;
                }
                else
                {
                    return RungCount != GetTopRung();
                }
            }
            else if (amount < -0.1f)
            {
                if (Ladder.CanStepOff)
                {
                    return true;
                }
                else
                {
                    return RungCount != GetBottomRung();
                }
            }

            return false;
        }

        public override int GetTopRung()
        {
            // Transition to climb off is during up anim
            int offset = Ladder.CanClimbOff ? 6 : 5;
            return Ladder.Rungs.GetTopRung() - 5;
        }

        public override int GetBottomRung()
        {
            return 0;
        }
    }
}
