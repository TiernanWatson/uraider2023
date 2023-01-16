using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public abstract class AutoGrabState : AirState
    { 
        private Vector3 _point;
        private Vector3 _grabPoint;
        private Vector3 _hangPoint;

        private float _forwardSpeed;
        private float _time;
        private float _height;
        private bool _standing;

        public AutoGrabState(PlayerController owner) : base(owner)
        {
            _point = Vector3.zero;
            _forwardSpeed = 0.0f;
            _time = 0.0f;
        }

        public virtual void ReceiveContext(Vector3 point, float forwardSpeed)
        {
            _point = point;
            _forwardSpeed = forwardSpeed;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.DrawRay(_grabPoint, Vector3.up, Color.blue, 5.0f);
            Debug.DrawRay(_hangPoint, Vector3.up, Color.green, 5.0f);

            _height = _point.y - _owner.settings.ledgeDownOffset;

            Vector3 difference = _grabPoint - _owner.transform.position;

            float horDist = UMath.HorizontalMag(difference);
            _time = horDist / _forwardSpeed;

            if (_time > 0.7f)
            {
                float ySpeed = UMath.JumpToReach(difference.y, _time, _owner.settings.gravity);

                

                Vector3 jumpVelocity = _owner.transform.forward * _forwardSpeed + Vector3.up * ySpeed;
                _owner.Movement.SetVelocity(jumpVelocity);
            }
            else
            {
                Vector3 timeLimited = UMath.JumpInTime(_owner.transform.position, _grabPoint, _owner.settings.gravity, 0.7f);
                _owner.Movement.SetVelocity(timeLimited);

                _time = 0.7f;
            }

            _owner.AnimControl.IsReaching = true;

            Physics.IgnoreLayerCollision(8, 9, true);
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.AnimControl.IsReaching = false;
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
                OnGrab();
            }
        }

        protected virtual void OnGrab()
        {

        }
    }
}
