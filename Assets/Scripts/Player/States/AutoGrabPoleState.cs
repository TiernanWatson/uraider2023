using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class AutoGrabPoleState : AirState
    {
        private PolePoint _pole;
        private Vector3 _point;
        private Vector3 _grabPoint;
        private Vector3 _hangPoint;

        private float _forwardSpeed;
        private float _time;
        private float _height;
        private bool _standing;
        private bool _isForward;

        public AutoGrabPoleState(PlayerController owner) : base(owner)
        {
            _pole = null;
            _point = Vector3.zero;
            _forwardSpeed = 0.0f;
            _time = 0.0f;
        }

        public void ReceiveContext(PolePoint pole, Vector3 point, float forwardSpeed, bool standing)
        {
            _pole = pole;
            _point = point;
            _forwardSpeed = forwardSpeed;
            _standing = standing;
        }

        public void ReceiveDirection(bool isForward)
        {
            _isForward = isForward;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _grabPoint = _point - Vector3.up * (_owner.settings.poleGrabDownOffset - 0.11f);
            _hangPoint = _point - Vector3.up * _owner.settings.poleHandDownOffset;
            _height = _point.y - _owner.settings.poleGrabDownOffset;

            Vector3 difference = _grabPoint - _owner.transform.position;

            float ySpeed = UMath.PeakAt(difference.y, -_owner.settings.gravity, out _time);
            float xzSpeed = UMath.HorizontalMag(difference) / _time;

            Vector3 direction = difference;
            direction.y = 0.0f;
            direction.Normalize();

            Vector3 jumpVelocity = Vector3.up * ySpeed + direction * xzSpeed;
            _owner.Movement.SetVelocity(jumpVelocity);

            _owner.AnimControl.IsReaching = true;

            Physics.IgnoreLayerCollision(8, 9, true);
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.AnimControl.IsReaching = false;

            Physics.IgnoreLayerCollision(8, 9, false);
        }

        public override void Update()
        {
            base.Update();

            _time -= Time.deltaTime;

            float yNextFrame = _owner.transform.position.y + _owner.Movement.Velocity.y * Time.deltaTime;
            bool willPassNextFrame = _owner.Movement.Velocity.y < 0.0f && yNextFrame < _grabPoint.y;

            // If reached the ledge, grab it
            if (_time <= 0.0f || willPassNextFrame)
            {
                Quaternion rotation = Quaternion.LookRotation((_isForward ? 1.0f : -1.0f) * _pole.transform.forward);

                _owner.Movement.SetVelocity(Vector3.zero);
                _owner.Movement.SetRotation(rotation);
                _owner.Movement.SetPosition(_grabPoint);

                _owner.AnimControl.Play("GrabPole");
                _owner.AnimControl.TargetMatchState("GrabPole", _hangPoint, rotation, 0.1f, 0.99f);

                _owner.BaseStates.PoleClimb.ReceivePole(_pole);
                _owner.StateMachine.ChangeState(_owner.BaseStates.PoleClimb);
            }
        }
    }
}
