using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class MonkeyState : PlayerState
    {
        public MonkeySurface Surface { get; set; }

        private bool _goingToFreeclimb;

        public MonkeyState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _goingToFreeclimb = false;
            _owner.Movement.GoToRoam();
            _owner.Movement.Motor.UseGroundingForce = false;
            _owner.Movement.RootMotionRotate = false;
        }

        public override void OnExit()
        {
        }

        public override void Update()
        {
            if (_owner.UInput.Crouch.triggered)
            {
                _owner.AnimControl.Fall();
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else if (_goingToFreeclimb)
            {
                if (_owner.AnimControl.IsIn("FreeclimbIdle") || _owner.AnimControl.IsIn("FreeclimbIdleM"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Freeclimb);
                }
            }
            else
            {
                Vector3 testPoint = _owner.transform.position;
                testPoint.y = Surface.transform.position.y;

                bool inBounds = Surface.GetBounds().Contains(testPoint);

                if (!inBounds)
                {
                    Vector3 closestPoint = Surface.ClosetPointTo(_owner.transform.position);
                    Vector3 correction = closestPoint + Vector3.down * _owner.settings.monkeyOffset;
                    _owner.SetPosition(correction);

                    if (_owner.AnimControl.IsTag("Moving"))
                    {
                        Vector3 playerLocal = Surface.transform.InverseTransformPoint(_owner.transform.position);
                        playerLocal.y = 0.0f;
                        playerLocal.Normalize();

                        float forwardDot = Vector3.Dot(Vector3.forward, playerLocal);
                        float rightDot = Vector3.Dot(Vector3.right, playerLocal);

                        ConnectionSide side;
                        if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
                        {
                            side = Mathf.Sign(forwardDot) > 0.0f ? ConnectionSide.Z : ConnectionSide.MinusZ;
                        }
                        else
                        {
                            side = Mathf.Sign(rightDot) > 0.0f ? ConnectionSide.X : ConnectionSide.MinusX;
                        }

                        foreach (MonkeyConnection c in Surface.Connections)
                        {
                            if (side != c.Side)
                            {
                                continue;
                            }

                            FreeclimbSurface hit = c.Surface;
                            int rungNumber = c.IsUp ? 1 : hit.Rungs.GetTopRung();

                            Vector3 targetRung = hit.transform.position + hit.transform.up * hit.Rungs.GetHeightAt(rungNumber);
                            Vector3 targetPos = _owner.settings.GetFreeclimbPosition(targetRung, hit.transform.forward, hit.transform.up);

                            // Stop player floating over to the middle of freeclimb, keep their sideways distance
                            Vector3 sideways = Vector3.Project(_owner.transform.position, hit.transform.right);
                            Vector3 upwards = Vector3.ProjectOnPlane(targetPos, hit.transform.right);

                            targetPos = sideways + upwards;

                            Vector3 targetDir = hit.transform.forward;
                            targetDir.y = 0.0f;
                            targetDir.Normalize();

                            Quaternion targetRot = Quaternion.LookRotation(targetDir);

                            string anim = c.IsUp ? "MonkeyToFreeclimbUp" : "MonkeyToFreeclimbDown";

                            _owner.AnimControl.TargetMatchState(anim, targetPos, targetRot, 0.1f, 0.999f);
                            _owner.AnimControl.FadeTo(anim, 0.05f);
                            _owner.BaseStates.Freeclimb.Surface = hit;
                            _owner.BaseStates.Freeclimb.RungCount = rungNumber;
                            _goingToFreeclimb = true;

                            break;
                        }
                    }
                }

                _owner.Movement.Resolve();
            }
        }

        private bool TryFindFreeclimb(out FreeclimbSurface hit)
        {
            Vector3 start = _owner.transform.position + Vector3.up * 1.8f;
            Ray ray = new Ray(start, Vector3.up);

            if (Physics.Raycast(ray, out RaycastHit hit2, 2.0f, _owner.settings.ledgeLayers, QueryTriggerInteraction.Ignore))
            {
                hit = hit2.collider.GetComponent<FreeclimbSurface>();
                return hit;
            }

            hit = null;
            return false;
        }
    }
}
