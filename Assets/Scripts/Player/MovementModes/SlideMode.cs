using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// MovementMode for most movement in the game
    /// </summary>
    public class SlideMode : MovementMode
    {
        public Vector3 SlopeDirection { get; private set; }

        public SlideMode(PlayerMovement movement) : base(movement)
        {
        }

        public override void Begin()
        {
            base.Begin();

            _movement.Motor.UseGroundingForce = true;
            _movement.TargetRotation = _movement.Rotation;
            _movement.TargetVelocity = _movement.Velocity;
        }

        public override void Resolve()
        {
            Vector3 slopeRight = Vector3.Cross(Vector3.up, _movement.Motor.Ground.normal);
            Vector3 slopeDir = Vector3.Cross(slopeRight, _movement.Motor.Ground.normal);
            Vector3 faceDir = Vector3.Cross(slopeRight, Vector3.up);

            /*bool back = Vector3.Angle(faceDir, _movement.transform.forward) > 90.0f;
            if (back)
            {
                faceDir = -faceDir;
            }

            SlopeDirection = slopeDir;*/

            _movement.TargetRotation = Quaternion.LookRotation(faceDir);
            _movement.Rotation = Quaternion.RotateTowards(_movement.Rotation, _movement.TargetRotation, Time.deltaTime * _movement._slideRotateRate);

            _movement.TargetVelocity = slopeDir.normalized * 4.0f;
            _movement.Velocity = Vector3.MoveTowards(_movement.Velocity, _movement.TargetVelocity, Time.deltaTime * 20.0f);

            Debug.DrawRay(_movement.transform.position, _movement.Velocity, Color.red);
        }
    }
}
