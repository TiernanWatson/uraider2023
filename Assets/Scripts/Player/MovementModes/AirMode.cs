using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    public class AirMode : MovementMode
    {
        public AirMode(PlayerMovement movement) : base(movement)
        {
        }

        public override void Begin()
        {
            base.Begin();

            _movement.Motor.UseGroundingForce = false;
            _movement.AnimControl.ApplyRootMotion = false;
        }

        public override void End()
        {
            base.End();
        }

        public override void Resolve()
        {
            // > 0 because step offset can interfere with collision detection
            if (_movement.Velocity.y > 0.0f)
            {
                bool hitCeiling = (_movement.Motor.CharControl.collisionFlags & CollisionFlags.Above) != 0;

                if (hitCeiling)
                {
                    Vector3 vel = _movement.Velocity;
                    vel.y = 0.0f;
                    _movement.Velocity = vel;
                }
            }

            _movement.Velocity += Vector3.down * _movement.Gravity * Time.deltaTime;
            _movement.TargetVelocity = _movement.Velocity;
        }
    }
}
