using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    public class PushMode : MovementMode
    {
        public PushMode(PlayerMovement movement) : base(movement)
        {
        }

        public override void Begin()
        {
            base.Begin();

            _movement.AnimControl.RunForward();
            _movement.AnimControl.ApplyRootMotion = true;
        }

        public override void Resolve()
        {
            _movement.Velocity = _movement.Rotation * Vector3.forward * _movement.StickSpeed;
            _movement.TargetVelocity = _movement.Velocity = _movement.GetCameraRotaterY() * _movement.UInput.MoveInputRaw.normalized * _movement.StickRawSpeed;
        }
    }
}
