using System;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    public interface IClimbSettings
    {
        public IShimmyable Ledge { get; set; }
    }

    public class ClimbMode : MovementMode, IClimbSettings
    {
        public IShimmyable Ledge { get; set; }

        public ClimbMode(PlayerMovement movement) : base(movement)
        {
        }

        public override void Begin()
        {
            base.Begin();

            _movement.Motor.UseGroundingForce = false;
            _movement.AnimControl.ApplyRootMotion = true;
        }

        public override void End()
        {
            base.End();
        }

        public override void Resolve()
        {
            Vector3 input = Quaternion.Euler(0.0f, _movement._cam.eulerAngles.y, 0.0f) * _movement.UInput.MoveInputRaw;
            Vector3 desiredVel = Vector3.Project(input, Ledge.Gradient);

            _movement.Velocity = _movement.TargetVelocity = desiredVel;

            if (!_movement.AnimControl.IsIn("GrabBT"))
            {
                _movement.TargetRotation = Quaternion.LookRotation(Ledge.Forward);
                _movement.Rotation = Quaternion.Slerp(_movement.Rotation, _movement.TargetRotation, Time.deltaTime * 24.0f);
            }
        }
    }
}