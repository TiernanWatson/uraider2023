using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Human : MonoBehaviour
{
    public class IdleState : EnemyState
    {
        public IdleState(Human owner) : base(owner)
        {
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            _owner.Anim.SetFloat("Speed", _owner.NavAgent.velocity.magnitude);

            Collider[] cols = _owner.VisionOverlap();

            foreach (Collider c in cols)
            {
                if (c.CompareTag("Player"))
                {
                    var player = c.GetComponent<PlayerController>();

                    Vector3 eyePos = _owner._eyePos.position;
                    Vector3 eyeDir = _owner._eyePos.forward;
                    Vector3 eyeRight = _owner._eyePos.right;
                    Vector3 eyeUp = _owner._eyePos.up;

                    float distance = Vector3.Distance(eyePos, c.transform.position);

                    Vector3 toPlayer = (c.transform.position - eyePos).normalized;
                    float angleToPlayer = Vector3.Angle(eyeDir, toPlayer);
                    float angleToPlayerXZ = Vector3.SignedAngle(eyeDir, toPlayer, eyeUp);
                    float angleToPlayerY = Vector3.SignedAngle(eyeDir, toPlayer, eyeRight);

                    bool inFovRange = distance <= _owner._visionDistance 
                        && Mathf.Abs(angleToPlayerXZ) < _owner._fieldOfViewXZ / 2.0f
                        && Mathf.Abs(angleToPlayerY) < _owner._fieldOfViewY / 2.0f;

                    bool canHear = distance <= _owner._hearingDistance && player.Velocity.magnitude > 3.0f;

                    if (inFovRange || canHear)
                    {
                        Vector3 playerMid = c.transform.position + Vector3.up * 1.0f;
                        Vector3 toPlayerMid = playerMid - eyePos;

                        Ray ray = new Ray(eyePos, toPlayerMid.normalized);
                        if (Physics.Raycast(ray, out RaycastHit hit, toPlayerMid.magnitude, _owner._sightLayers.value, QueryTriggerInteraction.Ignore))
                        {
                            if (hit.collider.CompareTag("Player"))
                            {
                                _owner.States.Alert.Target = c.GetComponent<PlayerController>();
                                _owner.StateMachine.ChangeState(_owner.States.Alert);
                                return;
                            }
                        }
                    }

                    /*bool inHearingRange = distance <= _owner._hearingDistance;

                    if (inHearingRange)
                    {
                        if (player.Velocity.magnitude > 3.0f)
                        {
                            _owner.States.Alert.Target = player;
                            _owner.StateMachine.ChangeState(_owner.States.Alert);
                        }
                    }*/
                }
            }
        }

        public override void Update()
        {
        }

        public override void Damage(float strength)
        {
            base.Damage(strength);

            _owner.States.Alert.Target = PlayerController.Local;
            _owner.StateMachine.ChangeState(_owner.States.Alert);
        }
    }
}
