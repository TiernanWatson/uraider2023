using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class MultiJumpState : JumpState
    {
        public MultiJumpState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.Movement.Motor.UseGroundingForce = false;

            Vector3 jumpDirection = _owner.UInput.MoveInputRaw;
            float angle = Vector3.SignedAngle(Vector3.forward, jumpDirection, Vector3.up);

            if (jumpDirection.sqrMagnitude < 0.01f)
            {
                _owner.AnimControl.JumpU();
            }
            else if (Mathf.Abs(angle) < 60.0f)
            {
                if (_owner.AnimControl.SpeedMultiplier < 0.0f)
                    JumpL(jumpDirection);
                else
                    _owner.AnimControl.JumpF();
            }
            else if (Mathf.Abs(angle) > 135.0f)
            {
                if (_owner.AnimControl.SpeedMultiplier > 0.0f)
                    JumpR(jumpDirection);
                else
                    _owner.AnimControl.JumpB();
            }
            else if (angle < -60.0f)
            {
                JumpL(jumpDirection);
            }
            else
            {
                JumpR(jumpDirection);
            }
        }

        public override void OnExit()
        {
            _owner.Movement.PauseRotation = false;
        }

        public override void Update()
        {
            PlayerAnim.JumpType jumpType = _owner.AnimControl.GetJumpType();

            if (jumpType != PlayerAnim.JumpType.None)
            {
                _owner.JumpToInput();
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else
            {
                _owner.Movement.Resolve();
            }
        }

        private void JumpR(Vector3 jumpDirection)
        {
            Vector3 lookDir = _owner.GetCameraRotater() * Vector3.Cross(jumpDirection, Vector3.up);
            _owner.RotateTo(lookDir);

            _owner.AnimControl.JumpR();

            _owner.Movement.PauseRotation = true;  // Stop player rotating back
        }

        private void JumpL(Vector3 jumpDirection)
        {
            Vector3 lookDir = _owner.GetCameraRotater() * Vector3.Cross(-jumpDirection, Vector3.up);
            _owner.RotateTo(lookDir);

            _owner.AnimControl.JumpL();

            _owner.Movement.PauseRotation = true;  // Stop player rotating back
        }
    }
}
