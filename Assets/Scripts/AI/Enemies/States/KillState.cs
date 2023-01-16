using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Human : MonoBehaviour
{
    public class KillState : EnemyState
    {
        public PlayerController Target { get; set; }

        public KillState(Human owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _owner.NavAgent.stoppingDistance = _owner._engagedStop;
            _owner.NavAgent.speed = _owner._patrolSpeed;
            _owner.Anim.applyRootMotion = true;
            _owner._weapon.TargetPosition = Target.transform.position;
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.Anim.SetBool("IsFiring", false);
        }

        public override void Update()
        {
            if (_owner._health <= 0.0f)
            {
                _owner.StateMachine.ChangeState(_owner.States.Dead);
            }
            else if (Target.Stats.Health == 0.0f)
            {
                _owner.StateMachine.ChangeState(_owner.States.Idle);
            }
            else
            {
                Vector3 toTarget = Target.transform.position - _owner.transform.position;

                if (toTarget.magnitude > _owner._engagedStop * 2.0f)
                {
                    _owner.StateMachine.ChangeState(_owner.States.Alert);
                }
                else
                {
                    float angle = Vector3.SignedAngle(_owner.transform.forward, toTarget, Vector3.up);
                    _owner.Anim.SetFloat("TargetAngle", angle);
                    _owner.Anim.SetBool("IsFiring", Mathf.Abs(angle) < 70.0f);

                    if (Mathf.Abs(angle) > 135.0f)
                    {
                        _owner.transform.rotation = Quaternion.LookRotation(toTarget);
                        _owner.Anim.Play("Locomotion"); // Get out of turn if in one
                    }
                }
            }
        }
    }
}
