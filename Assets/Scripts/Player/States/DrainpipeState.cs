using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class DrainpipeState : RungClimbState
    {
        public Drainpipe Pipe { get; set; }

        public DrainpipeState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.Movement.Wall.Activate();
            _owner.Movement.Wall.CanGoUp = (a) => true;
            _owner.Movement.Wall.CanGoRight = (a) => false;
            _owner.Movement.Wall.GetForward = () => Pipe.transform.forward;
            _owner.Movement.Motor.UseGroundingForce = false;

            Physics.IgnoreLayerCollision(8, 0);
            Physics.IgnoreLayerCollision(8, 9);
        }

        public override void OnExit()
        {
            Physics.IgnoreLayerCollision(8, 0, false);
            Physics.IgnoreLayerCollision(8, 9, false);
        }

        public override void Update()
        {
            if (_owner.UInput.Jump.triggered)
            {
                _owner.AnimControl.JumpB();
                _owner.Movement.SetVelocity(-_owner.transform.forward * _owner._jumpForwardSpeed + Vector3.up * _owner._jumpUpSpeed);
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else if (_owner.UInput.Crouch.triggered)
            {
                _owner.AnimControl.Fall();
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else
            {
                _owner.Movement.Resolve();

                if (_owner.AnimControl.IsIn("Idle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
                }
                else if (_owner.AnimControl.IsIn("DPipeIdle"))
                {
                    CorrectPosition();

                    Debug.Log("RC:" + RungCount);

                    if (_owner.UInput.MoveInputRaw.x > 0.5f && Pipe.RightLedge)
                    {
                        if (RungCount == Pipe.Rungs.GetTopRung() - 2)
                        {
                            GoToLedge();
                        }
                    }
                    else if (_owner.UInput.MoveInputRaw.x < -0.5f && Pipe.LeftLedge)
                    {
                        if (RungCount == Pipe.Rungs.GetTopRung() - 2)
                        {
                            GoToLedge(true);
                        }
                    }
                }
            }
        }

        private void GoToLedge(bool left = false)
        {
            Vector3 testPoint = _owner.transform.position;
            testPoint += (left ? -_owner.transform.right : _owner.transform.right) * 0.5f;

            var ledge = left ? Pipe.LeftLedge : Pipe.RightLedge;

            float t = ledge.Collider.Point.ClosestParamTo(testPoint, _owner.transform.forward);
            Vector3 point = ledge.Collider.Point.GetPoint(t);

            Vector3 targetPos = _owner.settings.GetLedgePosition(point, ledge.transform.forward);
            Quaternion targetRot = ledge.transform.rotation;
            MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.one, 1.0f);

            string anim = left ? "DPipeToHangLeft" : "DPipeToHangRight";
            _owner.AnimControl.TargetMatchState(anim, targetPos, targetRot, mask, 0.1f, 0.99f);
            _owner.AnimControl.Play(anim);

            _owner.BaseStates.Climb.Ledge = ledge;
            _owner.StateMachine.ChangeState(_owner.BaseStates.Climb);
        }

        private void CorrectPosition()
        {
            Vector3 point = Pipe.transform.position + Vector3.up * Pipe.Rungs.GetHeightAt(RungCount);
            Vector3 correction = _owner.GetDrainpipePosition(point, Pipe.transform.forward);
            _owner.LerpTo(correction);
        }

        public override int GetTopRung()
        {
            return Pipe.Rungs.GetTopRung() - 2;
        }

        public override int GetBottomRung()
        {
            return 0;
        }
    }
}
