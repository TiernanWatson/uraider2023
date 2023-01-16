using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom character movement and collision code for URaider.  Moves the player with 
/// movement deltas and stores ground information.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMotor : MonoBehaviour
{
    public struct GroundInfo
    {
        public readonly Vector3 normal;
        public readonly Vector3 point;

        public GroundInfo(Vector3 normal, Vector3 point)
        {
            this.normal = normal;
            this.point = point;
        }
    }

    public struct CapsuleInfo
    {
        public readonly Vector3 start;
        public readonly Vector3 end;

        public CapsuleInfo(CapsuleCollider capsule)
        {
            Vector3 halfDist = Vector3.up * (capsule.height * 0.5f - capsule.radius);
            Vector3 capsuleCenter = capsule.center;

            start = capsuleCenter - halfDist;
            end = capsuleCenter + halfDist;
        }
    }

    private const int MaxMoveLoops = 8;
    private const float CollisionOffset = 0.001f;

    public bool IsGrounded { get; private set; } = true;
    public GroundInfo Ground { get; private set; }
    public Vector3 LastMoveAmount { get; private set; }

    /// <summary>
    /// Maximum slope that the player can run up
    /// </summary>
    [SerializeField]
    private float _maxSlope;
    /// <summary>
    /// Minimum distance the player can move each frame
    /// </summary>
    [SerializeField]
    private float _minMoveDistance;
    /// <summary>
    /// Highest height the player can step up
    /// </summary>
    [SerializeField] 
    private float _stepOffset;
    /// <summary>
    /// All layers that can be used in collision detection
    /// </summary>
    [SerializeField] 
    private LayerMask _groundLayers;

    private bool _disableJumpGrounding = false;
    private bool _groundedLastFrame = true;
    private float _sqrMinMove;
    private CapsuleCollider _capsule;
    private CapsuleInfo _capsuleInfo;
    private Collider[] _overlaps = new Collider[8];
    private RaycastHit[] _moveSweep = new RaycastHit[8];
    private RaycastHit _capsuleGroundHit;
    private Rigidbody _rb;

    private void Awake()
    {
        _sqrMinMove = _minMoveDistance * _minMoveDistance;
        _capsule = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Application.targetFrameRate = -1;
    }

    /// <summary>
    /// Move the character by amount, handling steps and resolving collisions
    /// </summary>
    /// <param name="amount">Amount in world-space to move character</param>
    public void Move(Vector3 amount)
    {
        _capsuleInfo = new CapsuleInfo(_capsule);

        //ResolveOverlapCollisions();

        if (amount.sqrMagnitude > _sqrMinMove)
        {
            Vector3 oldPosition = transform.position;
            transform.position = MoveLoop(amount);
            LastMoveAmount = transform.position - oldPosition;
        }
        else
        {
            LastMoveAmount = Vector3.zero;
        }

        UpdateGround();
    }

    /// <summary>
    /// Forces player to leave grounded state and jump
    /// </summary>
    /// <param name="amount">Move amount</param>
    public void Jump(Vector3 amount)
    {
        _disableJumpGrounding = true;

        Move(amount);
    }

    private bool CapsuleFits(Vector3 position)
    {
        Vector3 capsuleStart = position + _capsuleInfo.start;
        Vector3 capsuleEnd = position + _capsuleInfo.end;

        UMath.DrawCapsule(capsuleStart, capsuleEnd, _capsule.radius);

        return !Physics.CheckCapsule(
            capsuleStart,
            capsuleEnd,
            _capsule.radius,
            _groundLayers.value,
            QueryTriggerInteraction.Ignore);
    }

    private int CapsuleSweep(Vector3 position, Vector3 amount, RaycastHit[] results)
    {
        Vector3 capsuleStart = position + _capsuleInfo.start;
        Vector3 capsuleEnd = position + _capsuleInfo.end;

        return Physics.CapsuleCastNonAlloc(
            capsuleStart, 
            capsuleEnd, 
            _capsule.radius, 
            amount.normalized, 
            results, 
            amount.magnitude, 
            _groundLayers.value, 
            QueryTriggerInteraction.Ignore);
    }

    private bool CapsuleSweepSingle(Vector3 position, Vector3 amount, out RaycastHit hit)
    {
        Vector3 capsuleStart = position + _capsuleInfo.start;
        Vector3 capsuleEnd = position + _capsuleInfo.end;

        return Physics.CapsuleCast(
            capsuleStart, 
            capsuleEnd,  
            _capsule.radius, 
            amount.normalized, 
            out hit, 
            amount.magnitude, 
            _groundLayers.value, 
            QueryTriggerInteraction.Ignore);
    }

    private bool CanGroundOn(Vector3 normal)
    {
        return Vector3.Angle(normal, transform.up) < _maxSlope;
    }

    private bool IsStep(Vector3 position, RaycastHit hit, out float height)
    {
        if (CanGroundOn(hit.normal))
        {
            height = 0.0f;
            return false;
        }

        Vector3 straightNormal = hit.normal;
        straightNormal.y = 0.0f;
        straightNormal.Normalize();

        Vector3 downCastStart = position
            + transform.up * (_stepOffset + 0.01f)
            + -straightNormal * (_capsule.radius + 0.01f);

        Ray ray = new Ray(downCastStart, -transform.up);

        Debug.DrawRay(downCastStart, -transform.up * _stepOffset, Color.green);

        // Check there is something to step onto
        if (Physics.Raycast(ray, out RaycastHit flatHit, _stepOffset + 0.01f, _groundLayers.value, QueryTriggerInteraction.Ignore))
        {
            Vector3 testPosition = flatHit.point + transform.up * 0.01f;

            if (CanGroundOn(flatHit.normal) && CapsuleFits(testPosition))
            {
                height = flatHit.point.y - position.y;
                return true;
            }
        }

        height = 0.0f;
        return false;
    }

    private float CorrectSidewaysOverlap(Vector3 wallNormal, Vector3 rayDirection, float hitDistance)
    {
        Vector3 wallToPlayer = -rayDirection;

        float apexAngle = Vector3.Angle(wallToPlayer, wallNormal);

        if (apexAngle == 0.0f)
        {
            return 0.0f;
        }

        float distanceToApex = (hitDistance + _capsule.radius - CollisionOffset) / Mathf.Tan(apexAngle * Mathf.Deg2Rad);

        float desiredDistance = (CollisionOffset + _capsule.radius) / Mathf.Tan(apexAngle * Mathf.Deg2Rad);

        return desiredDistance - distanceToApex;
    }

    /// <summary>
    /// Takes the character's current position and moves them taking into account continuous collision detection and stepping
    /// </summary>
    /// <param name="remaining">Desired move amount</param>
    /// <returns>The resulting position from the move</returns>
    private Vector3 MoveLoop(Vector3 remaining)
    {
        int loopCount = 0;
        bool collided = true;
        Vector3 result = transform.position;
        Vector3 previousHitNormal = Vector3.zero;
        Vector3 moveDir = remaining;

        while (collided && loopCount < MaxMoveLoops && remaining.sqrMagnitude > 0.0f)
        {
            Vector3 sweepAmount = remaining + remaining.normalized * CollisionOffset;
            collided = CapsuleSweepSingle(result, sweepAmount, out RaycastHit closestHit);

            if (collided)
            {
                Debug.Log("Hit: " + closestHit.collider.name);
                Debug.DrawRay(closestHit.point, closestHit.normal, Color.red, 2.0f);

                bool couldGetPushedIntoWall = closestHit.distance < CollisionOffset;
                float moveMagnitude = couldGetPushedIntoWall ? 0.0f : (closestHit.distance - CollisionOffset);

                Vector3 testOverlapsPosition = result + remaining.normalized * moveMagnitude;
                moveMagnitude = CorrectSideOverlaps(moveMagnitude, testOverlapsPosition);

                Vector3 moveUsed = remaining.normalized * moveMagnitude;
                remaining -= moveUsed;
                result += moveUsed;

                if (IsGrounded && IsStep(result, closestHit, out float height))
                {
                    bool willAlreadyRunOver = height < _capsule.radius - CollisionOffset;

                    if (willAlreadyRunOver)
                    {
                        remaining = ReorientMoveOnPlane(remaining, closestHit.normal);
                    }
                    else
                    {
                        result.y = closestHit.point.y - _capsule.radius + 0.1f;
                    }
                }
                // Hit a ramp, project velocity up it and maintain speed
                else if (CanGroundOn(closestHit.normal))
                {
                    _disableJumpGrounding = false;  // Can start ground check again after a jump

                    if (IsGrounded)
                    {
                        remaining = ReorientMoveOnPlane(remaining, closestHit.normal);
                    }
                    else
                    {
                        remaining = Vector3.ProjectOnPlane(remaining, closestHit.normal);
                    }
                }
                // Hit a wall or steep slope
                else
                {
                    float upDistance = closestHit.point.y - result.y;
                    if (upDistance > _capsule.height - _capsule.radius && closestHit.normal.y < -float.Epsilon)
                    {
                        // Hit ceiling potentially right above, don't fall through floor
                        _disableJumpGrounding = false;
                    }

                    // Check for corners
                    if (loopCount > 0 && !CanGroundOn(previousHitNormal))
                    {
                        Vector3 playerRight = Vector3.Cross(transform.up, moveDir);
                        float normalDotSign = Mathf.Sign(Vector3.Dot(playerRight, closestHit.normal));
                        float previousDotSign = Mathf.Sign(Vector3.Dot(playerRight, previousHitNormal));
                        bool inCorner = normalDotSign != previousDotSign;

                        if (inCorner)
                        {
                            if (IsGrounded)
                            {
                                remaining = Vector3.zero;
                            }
                            else
                            {
                                Vector3 cornerDown = Vector3.Cross(previousHitNormal, closestHit.normal);
                                remaining = Vector3.Project(remaining, cornerDown.normalized);
                            }
                        }
                        else
                        {
                            remaining = Vector3.ProjectOnPlane(remaining, closestHit.normal);
                        }
                    }
                    // At a normal wall or steep slope
                    else
                    {
                        Vector3 slideNormal;
                        if (IsGrounded)
                        {
                            slideNormal = Vector3.ProjectOnPlane(closestHit.normal, transform.up);
                        }
                        else
                        {
                            slideNormal = closestHit.normal;
                        }

                        remaining = Vector3.ProjectOnPlane(remaining, slideNormal.normalized);
                    }
                }

                previousHitNormal = closestHit.normal;
            }
            else
            {
                // Nothing in way of move, simply perform it
                result += remaining;
                remaining = Vector3.zero;
                collided = false;
            }

            loopCount++;
        }

        return result;
    }

    private float CorrectSideOverlaps(float moveMagnitude, Vector3 testOverlapsPosition)
    {
        Vector3 start = testOverlapsPosition + _capsuleInfo.start;
        Vector3 end = testOverlapsPosition + _capsuleInfo.end;
        Collider[] overlaps = Physics.OverlapCapsule(start, end, _capsule.radius, _groundLayers.value, QueryTriggerInteraction.Ignore);

        foreach (Collider overlap in overlaps)
        {
            if (moveMagnitude < CollisionOffset)
            {
                moveMagnitude = 0.0f;
            }
            else
            {
                moveMagnitude -= CollisionOffset;
            }
        }

        return moveMagnitude;
    }

    /// <summary>
    /// Removes any hits that the player already is overlapping
    /// </summary>
    /// <param name="hits">Hit results</param>
    /// <param name="hitCount">Number of results</param>
    /// <returns>New hitCount number</returns>
    private int FilterSweepHits(RaycastHit[] hits, int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i].distance <= 0.0f)
            {
                hits[i] = hits[hitCount - 1];

                hitCount--;
                i--;
            }
        }

        return hitCount;
    }

    private RaycastHit GetClosestHit(RaycastHit[] hits, int hitCount)
    {
        RaycastHit closest = hits[0];

        for (int i = 1; i < hitCount; i++)
        {
            if (hits[i].distance < closest.distance)
            {
                closest = hits[i];
            }
        }

        return closest;
    }

    private bool RaycastSingle(Vector3 position, Vector3 amount, out RaycastHit hit)
    {
        Ray ray = new Ray(position, amount.normalized);

        return Physics.Raycast(ray, out hit, amount.magnitude, _groundLayers.value, QueryTriggerInteraction.Ignore);
    }

    private Vector3 ReorientMoveOnPlane(Vector3 move, Vector3 groundNormal)
    {
        Vector3 left = Vector3.Cross(move, transform.up);
        return Vector3.Cross(groundNormal, left).normalized * move.magnitude;
    }

    private void ResolveOverlapCollisions()
    {
        Vector3 capsuleStart = transform.position + _capsuleInfo.start;
        Vector3 capsuleEnd = transform.position + _capsuleInfo.end;

        for (int j = 0; j < MaxMoveLoops; j++)
        {
            int collisionCount = Physics.OverlapCapsuleNonAlloc(capsuleStart, capsuleEnd, _capsule.radius,
                _overlaps, _groundLayers.value, QueryTriggerInteraction.Ignore);

            if (collisionCount > 0)
            {
                for (int i = 0; i < collisionCount; i++)
                {
                    Collider col = _overlaps[i];
                    if (Physics.ComputePenetration(
                        _capsule,
                        transform.position,
                        transform.rotation,
                        col,
                        col.transform.position,
                        col.transform.rotation,
                        out Vector3 dir,
                        out float dist))
                    {
                        Vector3 collisionCorrection = dir * dist;
                        transform.position += collisionCorrection;
                    }
                }
            }
            else
            {
                break;
            }
        }
    }

    private bool SphereCastSingle(Vector3 position, Vector3 amount, out RaycastHit hit)
    {
        return Physics.SphereCast(position, _capsule.radius, amount.normalized, out hit, amount.magnitude, _groundLayers.value, QueryTriggerInteraction.Ignore);
    }

    private void UpdateGround()
    {
        IsGrounded = false;
        Ground = new GroundInfo(Vector3.up, Vector3.zero);

        // Currently jumping and haven't hit something groundable yet
        if (_disableJumpGrounding)
        {
            return;
        }

        float checkDistance = Mathf.Abs(_capsule.center.y) + (_groundedLastFrame ? _stepOffset : 0.001f) + CollisionOffset;
        Vector3 bodyHalf = transform.position + _capsule.center;

        Debug.DrawRay(bodyHalf, Vector3.down * (checkDistance + _capsule.radius));

        float capsuleHitAngle = 90.0f;
        float correctionToGround = 0.0f;
        if (SphereCastSingle(bodyHalf, Vector3.down * checkDistance, out _capsuleGroundHit))
        {
            Ground = new GroundInfo(_capsuleGroundHit.normal, _capsuleGroundHit.point);

            capsuleHitAngle = Vector3.Angle(Ground.normal, transform.up);
            IsGrounded = capsuleHitAngle < _maxSlope;

            correctionToGround = _capsule.center.y - _capsuleGroundHit.distance - _capsule.radius;
        }

        if (_groundedLastFrame && RaycastSingle(bodyHalf, Vector3.down * checkDistance, out RaycastHit hit))
        {
            Ground = new GroundInfo(hit.normal, hit.point);

            float rayAngle = Vector3.Angle(Ground.normal, transform.up);
            if (capsuleHitAngle > rayAngle)
            {
                IsGrounded = rayAngle < _maxSlope;
            }
        }

        // Keep player at good distance to ground when grounded
        if (IsGrounded)
        {
            Vector3 newPosition = transform.position;
            newPosition.y += correctionToGround;
            transform.position = newPosition;
        }

        _groundedLastFrame = IsGrounded;
    }
}
