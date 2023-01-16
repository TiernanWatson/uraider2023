using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeclimbLedge : IShimmyable
{
    public static FreeclimbLedge Create(FreeclimbSurface surface, int rung)
    {
        Vector3 startPosition = new Vector3(-surface.Rungs.Collider.size.x / 2.0f, surface.Rungs.GetHeightAt(rung), 0.0f);
        Vector3 endPosition = new Vector3(surface.Rungs.Collider.size.x / 2.0f, surface.Rungs.GetHeightAt(rung), 0.0f);
        startPosition = surface.transform.TransformPoint(startPosition);
        endPosition = surface.transform.TransformPoint(endPosition);

        Vector3 forward = surface.transform.forward;
        forward.y = 0.0f;
        forward.Normalize();

        return Create(surface, startPosition, endPosition, forward, surface.transform.right);
    }

    public static FreeclimbLedge Create(FreeclimbSurface surface, Vector3 position, Vector3 end, Vector3 forward, Vector3 right)
    {
        FreeclimbLedge first = new FreeclimbLedge(true, false, position, forward, right)
        {
            Next = new FreeclimbLedge(false, true, end, forward, right),
        };
        first._surface = surface;
        return first;
    }

    public bool IsStart { get; private set; }

    public bool IsEnd { get; private set; }

    public bool HasWall => true;

    public ClimbUpType ClimbUp => ClimbUpType.Blocked;

    public Vector3 Forward { get; private set; }

    public Vector3 Right { get; private set; }

    public Vector3 Position { get; private set; }

    public Vector3 Gradient => (Next.Position - Position).normalized;

    public IShimmyable Previous { get; set; }

    public IShimmyable Next { get; set; }

    private FreeclimbSurface _surface;

    public FreeclimbLedge(bool isStart, bool isEnd, Vector3 position, Vector3 forward, Vector3 right)
    {
        IsStart = isStart;
        IsEnd = isEnd;
        Position = position;
        Forward = forward;
        Right = right;
        Previous = null;
        Next = null;
    }

    public float ClosestParamTo(Vector3 point, Vector3 direction)
    {
        // Point on ledge line
        Vector3 p1 = Position;

        // Derived from ray p = o + mx and ledge p = o + mx
        float t = direction.x * p1.z
            - direction.x * point.z
            - direction.z * p1.x
            + direction.z * point.x;

        t /= direction.z * Gradient.x - direction.x * Gradient.z;

        return t;
    }

    public float GetMaxT()
    {
        Vector3 p2 = Next.Position;

        float diff = p2.x - Position.x;
        float tX = diff / Gradient.x;

        if (!float.IsNaN(tX))
        {
            return tX;
        }
        else
        {
            diff = p2.z - Position.z;
            return diff / Gradient.z;
        }
    }

    public Vector3 GetPoint(float t)
    {
        return Position + Gradient * t;
    }

    public bool IsBeyondEnd(Vector3 position)
    {
        if (Next != null)
        {
            Vector3 localPositionR = _surface.transform.InverseTransformPoint(position);
            if (localPositionR.x > (_surface.Rungs.Collider.size.x - _surface.Rungs.Collider.center.x))
            {
                return true;
            }
        }

        Vector3 localPositionL = _surface.transform.InverseTransformPoint(position);
        return localPositionL.x < 0.0f;
    }
}

public partial class PlayerController : MonoBehaviour
{
    public class FreeclimbState : RungClimbState
    {
        public FreeclimbSurface Surface { get; set; }

        private bool _goingToWallclimb;
        private bool _goingToMonkey;

        public FreeclimbState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _goingToWallclimb = false;
            _goingToMonkey = false;

            _owner.Movement.GoToRoam();
            _owner.Movement.RootMotionRotate = true;
            _owner.Movement.Motor.UseGroundingForce = false;

            Physics.IgnoreLayerCollision(8, 0);
            Physics.IgnoreLayerCollision(8, 9);
            Physics.IgnoreLayerCollision(8, 11);
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.Movement.RootMotionRotate = false;

            Physics.IgnoreLayerCollision(8, 0, false);
            Physics.IgnoreLayerCollision(8, 9, false);
            Physics.IgnoreLayerCollision(8, 11, false);
        }

        public override void Update()
        {
            if (_goingToWallclimb)
            {
                if (_owner.AnimControl.IsIn("WallclimbIdle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Wallclimb);
                }
            }
            else if (_goingToMonkey)
            {
                if (_owner.AnimControl.IsIn("MonkeyIdle"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Monkey);
                }
            }
            else
            {
                bool isIdle = _owner.AnimControl.IsIn("FreeclimbIdle");
                bool isIdleM = _owner.AnimControl.IsIn("FreeclimbIdleM");

                if (isIdle || isIdleM)
                {
                    if (_owner.UInput.MoveInputRaw.z > 0.1f)
                    {
                        if (RungCount == GetTopRung() && Surface.MonkeyUp)
                        {
                            string anim = isIdle ? "FreeclimbToMonkeyUp" : "FreeclimbToMonkeyUpM";
                            MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.up, 0.0f);
                            _owner.AnimControl.TargetMatchState(anim, Vector3.up * (Surface.MonkeyUp.transform.position.y - 2.0f), Quaternion.identity, mask, 0.05f, 0.99f);
                            _owner.AnimControl.Play(anim);
                            _owner.BaseStates.Monkey.Surface = Surface.MonkeyUp;
                            _goingToMonkey = true;
                        }
                        else if (RungCount == GetTopRung() && Surface.WallclimbUp)
                        {
                            Vector3 targetPosition = Surface.WallclimbUp.transform.position + Vector3.up * Surface.WallclimbUp.Rungs.GetHeightAt(0);
                            targetPosition = _owner.settings.GetWallclimbPosition(targetPosition, Surface.WallclimbUp.transform.forward);

                            // Keep player's sideways position so they don't float to the centre
                            Vector3 sideways = Vector3.Project(_owner.transform.position, Surface.WallclimbUp.transform.right);
                            Vector3 upwards = Vector3.ProjectOnPlane(targetPosition, Surface.WallclimbUp.transform.right);
                            targetPosition = sideways + upwards;

                            Quaternion targetRot = Quaternion.LookRotation(Surface.WallclimbUp.transform.forward);
                            string anim = isIdle ? "FreeclimbToWallclimbUp" : "FreeclimbToWallclimbUpM";
                            _owner.AnimControl.TargetMatchState(anim, targetPosition, targetRot, 0.1f, 0.99f);
                            _owner.AnimControl.Play(anim);
                            _owner.BaseStates.Wallclimb.Surface = Surface.WallclimbUp;
                            _owner.BaseStates.Wallclimb.RungCount = 0;
                            _goingToWallclimb = true;
                        }
                    }
                    else if (_owner.UInput.MoveInputRaw.z < -0.1f)
                    {
                        if (RungCount == GetBottomRung() && Surface.MonkeyDown)
                        {
                            string anim = isIdle ? "FreeclimbToMonkeyDown" : "FreeclimbToMonkeyDownM";
                            MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.up, 0.0f);
                            _owner.AnimControl.TargetMatchState(anim, Vector3.up * (Surface.MonkeyDown.transform.position.y - 2.0f), Quaternion.identity, mask, 0.05f, 0.99f);
                            _owner.AnimControl.Play(anim);
                            _owner.BaseStates.Monkey.Surface = Surface.MonkeyDown;
                            _goingToMonkey = true;
                        }
                        else if (RungCount == GetBottomRung() && Surface.WallclimbDown)
                        {
                            FreeclimbLedge ledge = FreeclimbLedge.Create(Surface, RungCount);
                            _owner.BaseStates.Climb.ReceiveLedge(ledge);
                            _owner.BaseStates.Climb.FreeclimbSurface = Surface;

                            string anim = isIdle ? "FreeclimbToHang" : "FreeclimbToHangM";
                            _owner.AnimControl.Play(anim);

                            _owner.StateMachine.ChangeState(_owner.BaseStates.Climb);
                        }
                    }
                    else if (Mathf.Abs(_owner.UInput.MoveInputRaw.x) > 0.1f)
                    {
                        FreeclimbLedge ledge = FreeclimbLedge.Create(Surface, RungCount);
                        _owner.BaseStates.Climb.ReceiveLedge(ledge);
                        _owner.BaseStates.Climb.FreeclimbSurface = Surface;

                        string anim = isIdle ? "FreeclimbToHang" : "FreeclimbToHangM";
                        _owner.AnimControl.Play(anim);

                        _owner.StateMachine.ChangeState(_owner.BaseStates.Climb);

                    }
                    else
                    {
                        Vector3 rungP = Surface.transform.position + Surface.transform.up * Surface.Rungs.GetHeightAt(RungCount);
                        Vector3 targetPosition = rungP - Vector3.up * _owner.settings.freeclimbUpOffset;
                        Vector3 correction = targetPosition - Vector3.Project(targetPosition, Surface.transform.right);
                        correction += Vector3.Project(_owner.transform.position, Surface.transform.right);
                        _owner.LerpTo(correction);
                    }
                }
            }

            _owner.Movement.Resolve();

            base.Update();
        }

        private bool TryFindMonkey(out RaycastHit hit, out MonkeySurface surface)
        {
            Vector3 start = _owner.transform.position - _owner.transform.forward;
            Ray ray = new Ray(start, Vector3.up);

            if (Physics.Raycast(ray, out hit, 3.0f, _owner.settings.ledgeLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Monkey"))
                {
                    surface = hit.collider.GetComponent<MonkeySurface>();
                    return true;
                }
            }

            surface = null;
            return false;
        }

        private bool TryFindWallclimb(out WallclimbSurface surface)
        {
            surface = Surface.WallclimbUp;
            return surface == true;
        }

        public override void RungIncrement(int amount)
        {
            base.RungIncrement(amount);

            if (_owner.AnimControl.IsIn("FreeclimbUp") || _owner.AnimControl.IsIn("FreeclimbUp 0")
                || _owner.AnimControl.IsIn("FreeclimbDown") || _owner.AnimControl.IsIn("FreeclimbDown 0"))
            {
                Vector3 rungP = Surface.transform.position + Surface.transform.up * Surface.Rungs.GetHeightAt(RungCount);
                Vector3 targetPosition = _owner.settings.GetFreeclimbPosition(rungP, Surface.transform.forward, Surface.transform.up);

                MatchTargetWeightMask mask = new MatchTargetWeightMask(new Vector3(0.0f, 1.0f, 1.0f).normalized, 0.0f);

                // States are duplicated because of a Unity target match bug when looping a clip
                // offset is because in the duplicated clip the second part of the anim plays
                float offset = _owner.AnimControl.IsIn("FreeclimbUp") ? 0.0f : 0.5f;
                float start = 0.02f + offset;
                float end = 0.49f + offset;
                //_owner.AnimControl.TargetMatch(targetPosition, Quaternion.identity, mask, start, end);
            }
        }

        public override int GetTopRung()
        {
            return Surface.Rungs.GetTopRung();
        }

        public override int GetBottomRung()
        {
            return 1;
        }
    }
}
