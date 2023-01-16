using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Human : MonoBehaviour
{
    public class PatrolState : IdleState
    {
        private float _time;
        private PatrolPoint _point;

        public PatrolState(Human owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _time = 0.0f;
            _point = _owner.GetNextPatrol();

            _owner.NavAgent.stoppingDistance = _owner._patrolStop;
            _owner.NavAgent.speed = _owner._patrolSpeed;

            _owner.Anim.applyRootMotion = false;
        }

        public override void FixedUpdate()
        {
            Vector3 toDestination = _point.transform.position - _owner.transform.position;
            if (UMath.HorizontalMag(toDestination) <= _owner.NavAgent.stoppingDistance)
            {
                if (Time.time - _time > _point.WaitTime)
                {
                    _point = _owner.GetNextPatrol();
                }
            }
            else
            {
                _owner.NavAgent.SetDestination(_point.transform.position);
                _time = Time.time;
            }

            base.FixedUpdate();
        }

        public override void Damage(float strength)
        {
            base.Damage(strength);

            _owner.States.Alert.Target = PlayerController.Local;
            _owner.StateMachine.ChangeState(_owner.States.Alert);
        }
    }
}