using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Human : MonoBehaviour
{
    public class AlertState : EnemyState
    {
        public PlayerController Target { get; set; }

        public AlertState(Human owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _owner.Anim.SetBool("IsCombat", true);
            _owner.Anim.applyRootMotion = false;
            _owner.NavAgent.stoppingDistance = _owner._engagedStop;
            _owner.NavAgent.speed = _owner._engagedSpeed;
        }

        public override void Update()
        {
            if (_owner._health <= 0.0f)
            {
                _owner.StateMachine.ChangeState(_owner.States.Dead);
            }
            else
            {
                Vector3 toDestination = Target.transform.position - _owner.transform.position;
                if (toDestination.magnitude <= _owner.NavAgent.stoppingDistance)
                {
                    Debug.Log("At player");
                    _owner.States.Kill.Target = Target;
                    _owner.StateMachine.ChangeState(_owner.States.Kill);
                }
                else if (Target)
                {
                    _owner.NavAgent.SetDestination(Target.transform.position);
                }
            }
        }
    }
}
