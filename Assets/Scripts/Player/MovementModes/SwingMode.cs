using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    public class SwingMode : MovementMode
    {
        private const float Gravity = 18.0f;
        private const float RotateRate = 50.0f;  // Degree/second
        private const float MaxSwingAngle = 60.0f;
        private const float TwoPi = 2.0f * Mathf.PI;
        private const float PiTwo = Mathf.PI / 2.0f;

        public float TetherDistance { get; set; }
        public bool IsChangingHeight { get; set; } = false;
        public GrappleZone TetherPoint { get; set; }

        private bool _canIncreaseSpeed;
        private bool _isSwinging;
        private float _referenceTime;  // Time used as "start time" in angle calculations
        private float _currentTime;  // Time.time of this frame
        private float _targetAngularDistance;  // Size of angle at top of swing
        private float _angularDistance;  // Used for interpolation of above
        private float _frequency;  // Multiplier inside Cos - Sqrt(g/L)
        private float _deviationVelocity = 0.0f;  // Used by smooth damp
        private float _deviationAmplitude = 0.0f;  // Size of the original x-deviation (goes 1->0 for smooth grab)
        private float _angularDampVelocity = 0.0f;
        private float _firstPeakTime = 0.0f;  // Time at which player reaches highest point before initating forward swing
        private float _startTime = 0.0f;  // Time.time that player started swing mode
        private Vector3 _startXDeviation = Vector3.zero;  // Left-right offset when starting the swing
        private Quaternion _moveRotation;  // Rotation used in movement calculations

        public SwingMode(PlayerMovement movement) : base(movement)
        {
        }

        public override void Begin()
        {
            _movement._modelRotater.TargetRotation = _movement.ModelRotation = _movement.transform.rotation;
            _movement._modelRotater.UseLocal = false;
            _movement._modelRotater.ForceInterpolationCompletion();
            _canIncreaseSpeed = true;
            _isSwinging = false;
            _frequency = Mathf.Sqrt(Gravity / TetherDistance);  // From equation for a simple pendulum
            _moveRotation = _movement.transform.rotation;

            Vector3 tetherToPlayer = _movement.transform.position - TetherPoint.transform.position;
            CalculateSwingVariables();  // For the front-back main movement
            CalculateXDeviationValues(tetherToPlayer);  // For the left-to-right swing before returning to the centre smoothly

            // So resolve doesn't destroy momentum with wasSwinging
            _isSwinging = true;
        }

        private void CalculateSwingVariables()
        {
            // Preserve player speed
            Vector3 tetherToPlayer = _movement.transform.position - TetherPoint.transform.position;
            Vector3 verticalToPlayer = Vector3.ProjectOnPlane(tetherToPlayer, _movement.transform.right);
            Vector3 tangentDirection = Vector3.Cross(-tetherToPlayer.normalized, _movement.transform.right);
            float currentAngle = Vector3.SignedAngle(Vector3.down, verticalToPlayer, _movement.transform.right);
            float linearSpeed = Vector3.Project(_movement.Velocity, tangentDirection).magnitude;
            //float linearSpeed = Vector3.ProjectOnPlane(_movement.Velocity, -verticalToPlayer.normalized).magnitude;
            float potentialHeight = _movement.transform.position.y - (TetherPoint.transform.position.y - TetherDistance);
            float speed = Mathf.Sqrt(2.0f * Gravity * potentialHeight + linearSpeed * linearSpeed);  // Derived from conservation of energy

            _angularDistance = _targetAngularDistance = CalculateAmplitudeFromSpeed(speed);

            Debug.Log("Attempted amplitude: " + _angularDistance + " from max speed: " + speed);

            if (_angularDistance < currentAngle)
            {
                Debug.LogWarning("Calculated amplitude was less than start angle (shouldn't happen energy-wise from a jump)");
                _targetAngularDistance = _angularDistance = currentAngle;
            }

            Debug.Log("Hook Angle: " + currentAngle + " with final amplitude: " + _angularDistance);

            // Find corresponding time in equation
            Vector3 horizontalVel = _movement.Velocity;
            horizontalVel.y = 0.0f;
            bool negative = Vector3.Dot(horizontalVel, _movement.transform.forward) < 0.0f;
            float relativeTime = GetTimeAt(currentAngle, _targetAngularDistance, negative);
            _currentTime = _startTime = Time.time;
            _referenceTime = _currentTime - relativeTime;
        }

        private void CalculateXDeviationValues(Vector3 tetherToPlayer)
        {
            bool isGoingBack = Vector3.Dot(_movement.Velocity, _movement.transform.forward) < 0.0f;
            _firstPeakTime = (isGoingBack ? 1.0f : 2.0f) * Mathf.PI / _frequency;
            _deviationAmplitude = 1.0f;
            _deviationVelocity = 0.0f;
            _startXDeviation = Vector3.Project(tetherToPlayer, _movement.transform.right);
        }

        public override void End()
        {
            _movement._modelRotater.TargetRotation = _movement.ModelRotation = Quaternion.identity;
            _movement._modelRotater.UseLocal = true;
            _movement._modelRotater.ForceInterpolationCompletion();
            _movement.UseDisplacement = false;
        }

        public override void Resolve()
        {
            bool wasSwinging = _isSwinging;

            _movement.UseDisplacement = true;

            _isSwinging = !IsChangingHeight && _movement.UInput.MoveInput.z > 0.1f;
            _currentTime = Time.time;

            // Start swinging from idle
            if (!wasSwinging && _isSwinging)
            {
                if (_angularDistance < 10.0f)
                {
                    // Don't take forever to build up momentum
                    _targetAngularDistance = 10.0f;
                }
                else
                {
                    Vector3 tetherToPlayer = _movement.transform.position - TetherPoint.transform.position;
                    CalculateSwingVariables();
                    CalculateXDeviationValues(tetherToPlayer);

                    //Debug.Log("LinSpeed: " + linearSpeed2 + /*" AngVel: " + angularVelocity +*/ " CurAng: " + currentAngle2);
                }

                _moveRotation = _movement.transform.rotation;
            }

            if (!_isSwinging && Mathf.Abs(_movement.UInput.MoveInput.x) > 0.01f)
            {
                Vector3 eulerAngles = _movement.Rotation.eulerAngles;
                eulerAngles.y += RotateRate * _movement.UInput.MoveInput.x * Time.deltaTime;
                _movement.TargetRotation = _movement.Rotation = Quaternion.Euler(eulerAngles);
            }

            float relativeTime = _currentTime - _referenceTime;

            UpdateSwingDistance(relativeTime);

            float currentAngle = GetAngleAt(_angularDistance, relativeTime);

            _movement.AnimControl.SwingAngle = currentAngle;

            // Adjust model, not player, so camera doesn't move funny
            Quaternion targetPlayerRotation = Quaternion.Euler(currentAngle, 0.0f, 0.0f);

            // Find the side-to-side movement
            //float xDeviationCorrectionTime = _firstPeakTime - (_referenceTime % _firstPeakTime);
            float xDeviationCorrectionTime = 5.0f;
            Vector3 xDeviation = _deviationAmplitude * Mathf.Cos(_currentTime - _startTime) * _startXDeviation;
            _deviationAmplitude = Mathf.SmoothDamp(_deviationAmplitude, 0.0f, ref _deviationVelocity, xDeviationCorrectionTime);

            // Adjust tether distance to account for the x-deviation (tether dist always vertical which could make player snap down)
            float effectiveTetherDistance = Mathf.Sqrt(TetherDistance * TetherDistance - xDeviation.magnitude * xDeviation.magnitude);

            // Set position to avoid floating-point error building up if we used velocity
            Vector3 toPlayer = (_moveRotation * targetPlayerRotation * Vector3.down) * effectiveTetherDistance;
            Vector3 targetPosition = TetherPoint.transform.position + toPlayer;
            targetPosition += xDeviation;

            // Calculate sideways rotation
            Vector3 actualToPlayer = _movement.transform.position - TetherPoint.transform.position;
            Vector3 swingForward = -Vector3.Cross(-actualToPlayer.normalized, _movement.transform.right);
            _movement.ModelRotation = Quaternion.LookRotation(swingForward, -actualToPlayer.normalized);

            // Use position instead of velocity to avoid f.p. error build up
            _movement.SetPosition(GetNeutralPosition());  // To handle collisions we need to sweep as if we were at the centre
            _movement.MoveAmount = targetPosition - _movement.transform.position;

            // Still set velocity so transitions to other modes are smooth
            Vector3 direction = _movement.transform.rotation * Quaternion.Euler(currentAngle, 0.0f, 0.0f) * Vector3.forward;
            float angularVelocity = GetAngularVelocityAt(_angularDistance, relativeTime);
            float linearSpeed = GetLinearFromAngular(Mathf.Deg2Rad * angularVelocity);
            _movement.TargetVelocity = _movement.Velocity = direction * linearSpeed;

            //Debug.Log("LS: " + linearSpeed + " AV: " + angularVelocity);
        }
        

        private void UpdateSwingDistance(float time)
        {
            float wrappedTime = (_frequency * time) % TwoPi;

            if (!_isSwinging)
            {
                _targetAngularDistance = 0.0f;
            }
            else if (wrappedTime >= PiTwo && _canIncreaseSpeed)
            {
                _targetAngularDistance += 20.0f;
                _targetAngularDistance = Mathf.Clamp(_targetAngularDistance, 0.0f, MaxSwingAngle);
                _canIncreaseSpeed = false;
            }
            else if (wrappedTime > 0.0f && wrappedTime < PiTwo)
            {
                _canIncreaseSpeed = true;
            }

            float interpSpeed = _isSwinging ? 1.0f : 2.0f;
            //float interpSpeed = _isSwinging ? 0.8f : 0.2f;
            _angularDistance = Mathf.SmoothDamp(_angularDistance, _targetAngularDistance, ref _angularDampVelocity, interpSpeed);
            //_angularDistance = Mathf.Lerp(_angularDistance, _targetAngularDistance, Time.deltaTime * interpSpeed);
        }

        private float CalculateAmplitudeFromSpeed(float velocity)
        {
            // Re-arranged derivative of the displacement equation (angular.vel. = ..ASin..)
            return (Mathf.Rad2Deg * velocity / _frequency) / TetherDistance;
            //return Mathf.Acos(velocity * velocity / (2.0f * gravity * TetherDistance) + Mathf.Cos(angle)) * Mathf.Rad2Deg;
        }

        private Vector3 GetNeutralPosition()
        {
            return TetherPoint.transform.position + Vector3.down * TetherDistance;
        }

        private float _offset = 0.0f;

        private float GetAngleAt(float maxAngle, float t)
        {
            return maxAngle * Mathf.Cos(_frequency * t + _offset);
        }

        private float GetTimeAt(float angle, float maxAngle, bool negative = false)
        {
            float time = Mathf.Acos(angle / maxAngle);
            if (negative)
            {
                // If we start swinging while going backwards
                time = -time;
            }
            return time / _frequency;
        }

        private float GetAngularVelocityAt(float maxAngle, float t)
        {
            return _frequency * maxAngle * Mathf.Sin(_frequency * t);
        }

        private float GetAngularFromLinear(float velocity)
        {
            return velocity / TetherDistance;
        }

        private float GetLinearFromAngular(float speed)
        {
            return TetherDistance * speed;
        }
    }
}
