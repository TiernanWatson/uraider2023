using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A shimmyable line that can be placed in the game world
/// </summary>
public class LedgePoint : MonoBehaviour, IShimmyable
{
    public SplineCollider Collider => _collider;
    public IShimmyable Next => _collider.Point.Next?.GetComponent<IShimmyable>();
    public IShimmyable Previous => _collider.Point.Previous?.GetComponent<IShimmyable>();
    public bool HasWall => _hasLegRoom;
    public ClimbUpType ClimbUp => _climbUpType;
    public Vector3 Forward => transform.forward;
    public Vector3 Right => transform.right;
    public Vector3 Position => transform.position;
    public Vector3 Gradient => Collider.Point.Gradient;

    public bool IsStart => Collider.Point.IsStart;

    public bool IsEnd => Collider.Point.IsEnd;

#pragma warning disable 0649

    [SerializeField] private bool _hasLegRoom;
    [SerializeField] private ClimbUpType _climbUpType;

#pragma warning restore 0649

    private SplineCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<SplineCollider>();
    }

    void OnDrawGizmosSelected()
    {
        // Only works in play mode due to serialization
        Gizmos.color = Color.green;
        if (_collider && Next != null)
        {
            Gizmos.DrawLine(transform.position, Next.Position);
            for (float i = 0; i < Vector3.Distance(transform.position, Next.Position); i += 0.25f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, transform.forward);
            }
        }
    }

    public static LedgePoint FindLedgeInRange(Vector3 start, 
        Vector3 direction, 
        float distance, 
        float maxHeight, 
        float maxAngle, 
        int layers, 
        float increment = 0.25f,
        LedgePoint exclude = null)
    {
        for (float i = 0; i <= maxHeight; i += increment)
        {
            Vector3 origin = start + Vector3.up * i;

            Debug.DrawRay(origin, direction * distance, Color.blue, 5.0f);

            if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                distance,
                layers,
                QueryTriggerInteraction.Collide))
            {
                float angle = Vector3.Angle(direction, hit.transform.forward);

                if (hit.collider.CompareTag("Ledge") && angle < maxAngle)
                {
                    LedgePoint ledge = hit.collider.GetComponent<LedgePoint>();
                    if (exclude != ledge)
                    {
                        return hit.collider.GetComponent<LedgePoint>();
                    }
                }
            }
        }

        return null;
    }

    public static bool FindLedge(Vector3 position, Vector3 direction, float distance, int layers, out LedgePoint ledge)
    {
        if (Physics.Raycast(
            position,
            direction,
            out RaycastHit hit,
            distance,
            layers,
            QueryTriggerInteraction.Collide))
        {
            if (hit.collider.CompareTag("Ledge"))
            {
                ledge = hit.collider.GetComponent<LedgePoint>();
                return true;
            }
        }

        ledge = null;
        return false;
    }

    public Vector3 GetPoint(float t)
    {
        return Collider.Point.GetPoint(t);
    }

    public float ClosestParamTo(Vector3 point, Vector3 direction)
    {
        return Collider.Point.ClosestParamTo(point, direction);
    }

    public float GetMaxT()
    {
        return Collider.Point.GetMaxT();
    }

    public bool IsBeyondEnd(Vector3 point)
    {
        return Collider.Point.IsBeyondEnd(point);
    }
}
