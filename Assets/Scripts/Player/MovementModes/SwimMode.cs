using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// MovementMode for most movement in the game
    /// </summary>
    public class SwimMode : MovementMode
    {
        private float _jump = 0.0f;
        private float _crouch = 0.0f;

        public SwimMode(PlayerMovement movement) : base(movement)
        {
        }

        public override void Begin()
        {
            base.Begin();

            _movement.UInput.Jump.started += (ctx) => _jump = 1.0f;
            _movement.UInput.Jump.canceled += (ctx) => _jump = 0.0f;
            _movement.UInput.Crouch.started += (ctx) => _crouch = 1.0f;
            _movement.UInput.Crouch.canceled += (ctx) => _crouch = 0.0f;
        }

        public override void End()
        {
            base.End();

            _movement.UInput.Jump.started -= (ctx) => _jump = 1.0f;
            _movement.UInput.Jump.canceled -= (ctx) => _jump = 0.0f;
            _movement.UInput.Crouch.started -= (ctx) => _crouch = 1.0f;
            _movement.UInput.Crouch.canceled -= (ctx) => _crouch = 0.0f;

            _movement.ModelRotation = Quaternion.identity;
        }

        public override void Resolve()
        {
            Vector3 direction = _movement.GetCameraRotater() * _movement.UInput.MoveInput;

            float yAmount = 0f;
            //float yAmount = _jump + -_crouch;

            Vector3 moveDir = direction + yAmount * Vector3.up;
            //moveDir.Normalize();

            if (moveDir.sqrMagnitude > 0.01f)
            {
                if (direction.sqrMagnitude > 0.01f)
                {
                    _movement.TargetRotation = Quaternion.LookRotation(direction);
                }
                _movement.TargetVelocity = moveDir * 3.0f;
                _movement.Velocity = Vector3.Slerp(_movement.Velocity, _movement.TargetVelocity, Time.deltaTime * 20.0f);
            }
            else
            {
                _movement.Velocity = _movement.TargetVelocity = Vector3.zero;
            }

            _movement.Velocity = Vector3.Slerp(_movement.Velocity, _movement.TargetVelocity, Time.deltaTime * 20.0f);

            // Outside if to allow player to turn in tight spaces
            _movement.Rotation = Quaternion.RotateTowards(_movement.Rotation, _movement.TargetRotation, Time.deltaTime * _movement._maxTurnAngle);
        }


    }
}
