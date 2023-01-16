using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    public class WallMode : MovementMode
    {
        public Func<float, bool> CanGoUp { get; set; }
        public Func<float, bool> CanGoRight { get; set; }
        public Func<Vector3> GetForward { get; set; }

        public WallMode(PlayerMovement movement) : base(movement)
        {
            CanGoRight = (amount) => true;
            CanGoUp = (amount) => true;
            GetForward = () => _movement.transform.forward;
        }

        public override void Begin()
        {
            _movement.Motor.UseGroundingForce = false;
        }

        public override void Resolve()
        {
            Vector3 camRelative = _movement.GetCameraRotaterY() * _movement.UInput.MoveInputRaw;
            Vector3 playerRelative = _movement.transform.InverseTransformVector(camRelative);
            Vector3 resultant = Vector3.zero;

            if (CanGoUp(playerRelative.z))
            {
                // Always want z to be aligned with up/down for camera changes
                resultant.y = playerRelative.z;
            }
            
            if (CanGoRight(playerRelative.x))
            {
                resultant.x = playerRelative.x;
            }

            _movement.Velocity = _movement.TargetVelocity = _movement.transform.TransformVector(resultant);
            _movement.Rotation = _movement.TargetRotation = Quaternion.LookRotation(GetForward());
        }
    }
}
