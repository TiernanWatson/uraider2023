using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// MovementMode for combat, player turns and runs backwards at points
    /// </summary>
    public class StrafeMode : MovementMode
    {
        private const float BoundaryMin = -45.0f;
        private const float BoundaryMax = 135.0f;
        private const float DecelRate = 12.0f;
        private const float GreyZone = 5.0f;

        private bool _runBack = false;
        private bool _switching = false;  // From run back <-> run forward

        public StrafeMode(PlayerMovement movement) : base(movement)
        {
        }

        public override void Begin()
        {
            base.Begin();

            _movement.Motor.UseGroundingForce = true;

            float angle = _movement.Velocity.sqrMagnitude > 0.01f 
                ? Vector3.SignedAngle(_movement.GetCameraForward(), _movement.transform.forward, Vector3.up)
                : 0.0f;

            if (angle < BoundaryMin || angle > BoundaryMax)
            {
                _switching = true;
                _runBack = true;
                _movement.AnimControl.SpeedMultiplier = -1.0f;
            }
            else if (angle > BoundaryMin && angle < BoundaryMax)
            {
                _switching = false;
                _runBack = false;
                _movement.AnimControl.SpeedMultiplier = 1.0f;
            }
        }

        public override void End()
        {
            base.End();
        }

        public override void Resolve()
        {
            Vector3 smoothDirection = _movement.GetCameraRotaterY() * _movement.UInput.MoveInput;
            Vector3 rawDirection = _movement.GetCameraRotaterY() * _movement.UInput.MoveInputRaw;
            Vector3 camForward = _movement.GetCameraForward();

            // Check for boundary crosses
            if (rawDirection.sqrMagnitude > 0.01f)
            {
                float angleFromCam = Vector3.SignedAngle(camForward, rawDirection, Vector3.up);

                if (CrossedBoundary(angleFromCam))
                {
                    _runBack = !_runBack;
                    _switching = true;
                }
            }

            // Handle run direction switching
            if (_switching)
            {
                // Create target rotation
                if (rawDirection.sqrMagnitude > 0.01f)
                {
                    _movement.TargetRotation = GetAdjustment() * Quaternion.LookRotation(rawDirection);
                }

                float angleLeft = Quaternion.Angle(_movement.Rotation, _movement.TargetRotation);
                bool changing = angleLeft > 1.0f && _movement.AnimControl.IsTag("Moving");
                _movement.AnimControl.ApplyRootMotion = !changing;

                float dir = _runBack ? -1.0f : 1.0f;

                bool accelerating = _runBack ? _movement.AnimControl.SpeedMultiplier > -0.98f
                    : _movement.AnimControl.SpeedMultiplier < 0.98f;

                if (accelerating)
                {
                    _movement.AnimControl.SpeedMultiplier += dir * DecelRate * Time.deltaTime;
                }
                else
                {
                    _movement.AnimControl.SpeedMultiplier = dir;
                    // Make sure rotation has completed or root motion can cause jerking
                    if (!changing)
                    {
                        // Make sure smoothed aligns avoid jerking
                        Quaternion camRot = Quaternion.LookRotation(camForward);
                        Quaternion inversed = Quaternion.Inverse(camRot);
                        _movement.UInput.SetSmoothedInput(inversed * rawDirection);
                        _switching = false;
                    }
                }
            }
            else
            {
                // Enable smooth keyboard rotation when not switching
                if (smoothDirection.sqrMagnitude > 0.01f)
                {
                    _movement.TargetRotation = GetAdjustment() * Quaternion.LookRotation(smoothDirection);
                }

                _movement.AnimControl.ApplyRootMotion = true;
            }

            _movement.Rotation = Quaternion.RotateTowards(_movement.Rotation, _movement.TargetRotation, _movement._maxTurnAngle * Time.deltaTime);

            // Should move exactly like roam mode, except rotation differs
            if (smoothDirection.sqrMagnitude > 0.01f)
            {
                // Don't use _movement.Rotation because it wouldn't work for running backwards
                _movement.Velocity = Quaternion.LookRotation(smoothDirection) * Vector3.forward * _movement.StickSpeed;
                _movement.TargetVelocity = rawDirection.normalized * _movement.StickRawSpeed;
            }
            else
            {
                _movement.Velocity = _movement.TargetVelocity = Vector3.zero;
            }
        }

        private Quaternion GetAdjustment()
        {
            return _runBack ? Quaternion.Euler(0.0f, 180.0f, 0.0f) : Quaternion.identity;
        }

        private bool CrossedBoundary(float angleFromCam)
        {
            if (_runBack)
            {
                float min = BoundaryMin + GreyZone;
                float max = BoundaryMax - GreyZone;
                return angleFromCam > min && angleFromCam < max;
            }
            else
            {
                float min = BoundaryMin - GreyZone;
                float max = BoundaryMax + GreyZone;
                return angleFromCam < min || angleFromCam > max;
            }
        }
    }
}
