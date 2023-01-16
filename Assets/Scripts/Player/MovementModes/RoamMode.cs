using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// MovementMode for most movement in the game
    /// </summary>
    public class RoamMode : MovementMode
    {
        public const float kDefaultEdgeStopDistance = 0.5f;

        public bool StopAtLedges { get; set; } = false;
        public bool PullFeetIn { get; set; } = false;
        public float EdgeStopDistance { get; set; } = kDefaultEdgeStopDistance;
        public float TurnRate { get; set; } = 1.0f;

        public bool StoppedAtEdge { get; private set; } = false;
        public LedgePoint OverlappedEdge { get; private set; } = null;

        // Used so the keyboard doesn't cause snapping
        private Quaternion _smoothedTarget;

        public RoamMode(PlayerMovement movement) : base(movement)
        {
        }

        public override void Begin()
        {
            base.Begin();

            StoppedAtEdge = false;

            _movement.Motor.UseGroundingForce = true;

            _movement.AnimControl.RunForward();
            _movement.AnimControl.ApplyRootMotion = true;

            _smoothedTarget = _movement.TargetRotation = _movement.Rotation;
            _movement.TargetVelocity = _movement.Velocity;
        }

        public override void Resolve()
        {
            Vector3 direction = _movement.GetCameraRotaterY() * _movement.UInput.MoveInput;
            Vector3 rawDir = _movement.GetCameraRotaterY() * _movement.UInput.MoveInputRaw;

            if (rawDir.sqrMagnitude > 0.01f)
            {
                _movement.TargetRotation = Quaternion.LookRotation(rawDir, Vector3.up);
            }

            // Dont snap Lara when turning with keyboard, but also want to provide quick standing turns
            if (_movement._input.IsKeyboardMove() && direction.sqrMagnitude > 0.01f)
            {
                _smoothedTarget = Quaternion.LookRotation(direction, Vector3.up);
            }
            else
            {
                // Important for tight turns but also to stop turn fighting if raw is different from interped
                _smoothedTarget = _movement.TargetRotation;
            }

            // Outside if to allow player to turn in tight spaces
            _movement.Rotation = Quaternion.RotateTowards(_movement.Rotation, _smoothedTarget, Time.deltaTime * TurnRate);

            if (StopAtLedges)
            {
                Vector3 rayOrigin = _movement.transform.position + _movement.transform.forward * EdgeStopDistance;

                UMath.DrawCapsule(rayOrigin, rayOrigin, 0.25f);

                Collider[] colliders = Physics.OverlapSphere(rayOrigin, 0.25f, _movement.Motor.GroundLayers, QueryTriggerInteraction.Collide);
                foreach (var collider in colliders)
                {
                    LedgePoint ledge = collider.GetComponent<LedgePoint>();
                    if (ledge && Vector3.Dot(ledge.transform.forward, _movement.transform.forward) < 0.0f)
                    {
                        Vector3 ledgePointAhead = ledge.GetPoint(ledge.ClosestParamTo(_movement.transform.position, _movement.transform.forward));
                        Debug.DrawRay(ledgePointAhead, Vector3.up, Color.red);
                        float distance = UMath.HorizontalMag(ledgePointAhead - _movement.transform.position);
                        if (distance - _movement.Motor.CharControl.minMoveDistance < EdgeStopDistance)
                        {
                            StoppedAtEdge = true;
                            _movement.SetPosition(ledgePointAhead - _movement.transform.forward * EdgeStopDistance);
                        }
                    }
                }
            }

            if (direction.sqrMagnitude > 0.01f)
            {
                if (StoppedAtEdge)
                {
                    _movement.Velocity = Vector3.zero;
                }
                else
                {
                    _movement.Velocity = _movement.Rotation * Vector3.forward * _movement.StickSpeed;
                }

                _movement.TargetVelocity = rawDir.normalized * _movement.StickRawSpeed;
            }
            else
            {
                _movement.Velocity = _movement.TargetVelocity = Vector3.zero;
            }
        }
    }
}
