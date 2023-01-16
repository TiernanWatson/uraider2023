using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decorator class for Unity's Character Controller.  Fixes issues 
/// such as snapping down off a ledge and head intersecting when crawling.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMotorCC : MonoBehaviour, IMotor
{
    public struct GroundInfo
    {
        public readonly Vector3 normal;
        public readonly Vector3 point;
        public readonly string tag;

        public GroundInfo(Vector3 normal, Vector3 point, string tag)
        {
            this.normal = normal;
            this.point = point;
            this.tag = tag;
        }
    }

    /// <summary>
    /// Invoked right before a move is attempted with desired move
    /// </summary>
    public event Action<Vector3> OnPreMove;
    /// <summary>
    /// Invoked right after a move with the actual amount moved
    /// </summary>
    public event Action<Vector3> OnPostMove;

    public bool UseGroundingForce { get; set; } = true;
    public bool IsGrounded { get; private set; } = true;
    public bool UseHeadCollision { get; set; } = false;
    public float StepOffset
    {
        get { return _stepOffset; }
        set
        {
            _stepOffset = value;

            // The code for when this is true sets this
            if (!_testSmallCeilings)
            {
                CharControl.stepOffset = _stepOffset;
            }
        }
    }
    public float CapsuleHeight
    {
        get { return CharControl.height; }
        set
        {
            CharControl.height = value;
            CharControl.center = new Vector3(0.0f, value * 0.5f, 0.0f);
        }
    }
    public int HitCount => _numHitsLastMove;
    public LayerMask GroundLayers => _groundLayers;
    public CharacterController CharControl { get; private set; }
    public GroundInfo Ground { get; private set; }
    public ControllerColliderHit[] Hits => _hitsLastMove;

#pragma warning disable 0649

    [SerializeField] private bool _testSmallCeilings = true;
    [SerializeField] private float _degroundDistance;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private SphereCollider _headCollider;

#pragma warning restore 0649

    private const float SweepEpsilon = 0.01f;

    private bool _groundedLastFrame = true;
    private bool _overrideGrounding = false;
    private bool _overrideGroundValue = true;
    private bool _overrideStepOffset = false;
    private float _stepOffset;
    private float _oldStepOffset;
    private int _numHitsLastMove = 0;
    private ControllerColliderHit[] _hitsLastMove = new ControllerColliderHit[8];
    private RaycastHit[] _headHits = new RaycastHit[8];

    private void Awake()
    {
        CharControl = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _stepOffset = _oldStepOffset = CharControl.stepOffset;

        Physics.IgnoreCollision(_headCollider, CharControl, true);

        ResetGround();
        GetGroundFromRay();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_numHitsLastMove <= 7)
        {
            _hitsLastMove[_numHitsLastMove] = hit;
            _numHitsLastMove++;
        }

        if (!_overrideGrounding)
        {
            // Gather ground info to be used if downwards ray is fruitless
            if (hit.normal.y > 0.0f)
            {
                Ground = new GroundInfo(hit.normal, hit.point, hit.collider.tag);
            }
        }
    }

    /// <summary>
    /// Move the character and handle ledge snapdowns and ground info
    /// </summary>
    /// <param name="amount"></param>
    public void Move(Vector3 amount)
    {
        if (CharControl.enabled)
        {
            _groundedLastFrame = IsGrounded;
            _numHitsLastMove = 0;

            ResetGround();

            if (_testSmallCeilings)
            {
                if (SweepTopSphere(transform.position, transform.up, _stepOffset, out RaycastHit hit))
                {
                    // Stop getting stuck under small ceilings
                    CharControl.stepOffset = 0.1f;
                }
                else
                {
                    CharControl.stepOffset = _stepOffset;
                }
            }

            if (UseHeadCollision)
            {
                amount += HandleHeadCollisions(amount);
            }

            OnPreMove?.Invoke(amount);

            Vector3 finalMovement = BuildMovementStep(amount);
            CharControl.Move(finalMovement);

            OnPostMove?.Invoke(amount);

            if (!_overrideGrounding)
            {
                FixLedgeSnapDown();

                // Allows player to run down step offset
                // TODO: INVESTIGATE ITS WORKING
                if (_groundedLastFrame)
                {
                    GetGroundFromRay();
                }

                float angle = Vector3.Angle(transform.up, Ground.normal);
                bool normalGrounded = CharControl.isGrounded && angle <= CharControl.slopeLimit;
                bool groundedOnSlope = CharControl.isGrounded && Ground.tag.Equals("Slope");
                IsGrounded = normalGrounded || groundedOnSlope;
            }
            else
            {
                IsGrounded = _overrideGroundValue;
            }
        }
    }

    public bool TestHeadCollision(Collider other)
    {
        Vector3 headCenter = _headCollider.transform.position + _headCollider.center;
        Collider[] cols = Physics.OverlapSphere(headCenter, _headCollider.radius);

        foreach (var col in cols)
        {
            if (col.Equals(other))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Stops player head going into walls when its horizontal
    /// </summary>
    private Vector3 HandleHeadCollisions(Vector3 moveAmount)
    {
        if (moveAmount.magnitude < CharControl.minMoveDistance)
        {
            return Vector3.zero;
        }

        Vector3 headCenter = _headCollider.transform.TransformPoint(_headCollider.center) + moveAmount;
        Collider[] cols = Physics.OverlapSphere(headCenter, _headCollider.radius + SweepEpsilon, _groundLayers.value, QueryTriggerInteraction.Ignore);

        Vector3 correction = Vector3.zero;

        foreach (Collider c in cols)
        {
            if (c.CompareTag("Player") || c == _headCollider)
            {
                continue;
            }

            if (Physics.ComputePenetration(_headCollider, _headCollider.transform.position, _headCollider.transform.rotation, c, c.transform.position, c.transform.rotation, out Vector3 dir, out float dist))
            {
                correction += dir * dist;
            }
        }

        return correction;
    }

    private void HeadSweep(ref Vector3 moveAmount)
    {
        Vector3 headCenter = _headCollider.transform.TransformPoint(_headCollider.center);
        float headRadius = _headCollider.radius;
        float sweepDistance = Mathf.Max(CharControl.minMoveDistance, moveAmount.magnitude) + SweepEpsilon;

        if (Physics.SphereCast(headCenter, headRadius, moveAmount.normalized, out RaycastHit hit, sweepDistance, _groundLayers.value, QueryTriggerInteraction.Ignore))
        {
            float newDistance = hit.distance - (SweepEpsilon * 0.5f);
            moveAmount = moveAmount.normalized * newDistance;
        }
    }

    private void GetGroundFromRay()
    {
        Ray downRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(downRay, out RaycastHit hit, _degroundDistance, _groundLayers.value, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.tag.Equals("Slope"))
            {
                // Avoid issues with walking off onto a non-slide slope
                if (Vector3.Angle(Ground.normal, transform.up) <= CharControl.slopeLimit)
                {
                    return;
                }
            }

            Ground = new GroundInfo(hit.normal, hit.point, hit.collider.tag);
        }
    }

    public void OverrideGrounding(bool on, bool status = true)
    {
        _overrideGrounding = on;
        _overrideGroundValue = status;
    }

    public bool SweepCapsule(Vector3 from, Vector3 amount, out RaycastHit hit)
    {
        Vector3 halfToSphere = transform.up * (CharControl.height * 0.5f - CharControl.radius);
        Vector3 capsuleStart = from + CharControl.center - halfToSphere;
        Vector3 capsuleEnd = from + CharControl.center + halfToSphere /*+ amount.normalized * (CharControl.skinWidth + 0.01f)*/;

        UMath.DrawCapsule(capsuleStart, capsuleEnd, CharControl.radius);
        UMath.DrawCapsule(capsuleStart + amount, capsuleEnd + amount, CharControl.radius);
        Debug.DrawLine(capsuleStart, capsuleEnd + amount, Color.red);

        return Physics.CapsuleCast(
            capsuleStart,
            capsuleEnd,
            CharControl.radius,
            amount.normalized,
            out hit,
            amount.magnitude,
            _groundLayers.value,
            QueryTriggerInteraction.Ignore);
    }

    public bool SweepTopSphere(Vector3 position, Vector3 dir, float dist, out RaycastHit hit)
    {
        Vector3 halfToSphere = transform.up * (CharControl.height * 0.5f - CharControl.radius);
        Vector3 start = position + CharControl.center + halfToSphere;
        float radius = CharControl.radius + CharControl.skinWidth;

        return Physics.SphereCast(
            start,
            radius,
            dir,
            out hit,
            dist,
            _groundLayers.value,
            QueryTriggerInteraction.Ignore);
    }

    private Vector3 BuildMovementStep(Vector3 velocity)
    {
        // Apply grounding to stop fluctuations in ground state
        if (UseGroundingForce)
        {
            velocity += Vector3.down * CharControl.stepOffset;
        }

        return velocity;
    }

    private void FixLedgeSnapDown()
    {
        if (UseGroundingForce)
        {
            if (_groundedLastFrame && !IsGrounded)
            {
                CharControl.Move(Vector3.up * CharControl.stepOffset);
            }
        }
    }

    private void ResetGround()
    {
        Ground = new GroundInfo(Vector3.up, Vector3.zero, "Untagged");
    }
}
