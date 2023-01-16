using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePoint : MonoBehaviour
{
    public bool IsEnd => _next == null;
    public bool IsStart => _previous == null;
    public SplinePoint Previous => _previous;
    public SplinePoint Next => _next;
    public Vector3 Gradient { get; private set; }

#pragma warning disable 0649

    private SplinePoint _previous;

    [SerializeField] private SplinePoint _next;

#pragma warning restore 0649

    private void Start()
    {
        if (_next)
        {
            Gradient = (Next.transform.position - transform.position).normalized;
            _next._previous = this;
        }
    }

    public float PaddingClamp(float t, float padding)
    {
        if (t < padding)
        {
            return padding;
        }
        else if (t > GetMaxT() - padding)
        {
            return GetMaxT() - padding;
        }

        return t;
    }

    public float GetMaxT(float padding = 0.0f)
    {
        if (!_next)
        {
            Debug.LogWarning("Tried to get max t from spline with no next");
            return 0.0f;
        }

        Vector3 p2 = _next.transform.position - Gradient * padding;

        float diff = p2.x - transform.position.x;
        float tX = diff / Gradient.x;

        if (!float.IsNaN(tX))
        {
            return tX;
        }
        else
        {
            diff = p2.z - transform.position.z;
            return diff / Gradient.z;
        }
    }

    public float GetMinT(float padding = 0.0f)
    {
        Vector3 p1 = transform.position + Gradient * padding;

        float diff = p1.x - transform.position.x;
        float tX = diff / Gradient.x;

        if (!float.IsNaN(tX))
        {
            return tX;
        }
        else
        {
            diff = p1.z - transform.position.z;
            return diff / Gradient.z;
        }
    }

    public Vector3 ClosestPointTo(Vector3 point, Vector3 direction)
    {
        float t = ClosestParamTo(point, direction);

        return GetPoint(t);
    }

    public float ClosestParamTo(Vector3 point)
    {
        Vector3 direction = -new Vector3(1.0f / Gradient.x, 1.0f / Gradient.y, 1.0f / Gradient.z);

        return ClosestParamTo(point, direction);
    }

    public float ClosestParamTo(Vector3 point, Vector3 direction)
    {
        // Point on ledge line
        Vector3 p1 = transform.position;

        // Derived from ray p = o + mx and ledge p = o + mx
        float t = direction.x * p1.z
            - direction.x * point.z
            - direction.z * p1.x
            + direction.z * point.x;

        t /= direction.z * Gradient.x - direction.x * Gradient.z;

        return t;
    }

    public float ClosestParamTo(Vector3 point, Vector3 direction, float padding)
    {
        float t = ClosestParamTo(point, direction);
        float maxT = GetMaxT(padding);
        float minT = GetMinT(padding);

        if (t > maxT)
        {
            t = maxT;
        }
        else if (t < minT)
        {
            t = minT;
        }

        return t;
    }

    /// <summary>
    /// Test if a position (usually player) has gone beyond the next point
    /// </summary>
    /// <param name="position">Test position</param>
    /// <returns>True if beyond, false otherwise</returns>
    public bool IsBeyondEnd(Vector3 position)
    {
        if (Next)
        {
            Vector3 localPositionR = Next.transform.InverseTransformPoint(position);
            if (localPositionR.x > 0.0f)
            {
                return true;
            }
        }

        Vector3 localPositionL = transform.InverseTransformPoint(position);
        return localPositionL.x < 0.0f;        
    }

    /// <summary>
    /// Get the point t units forward from this point
    /// </summary>
    /// <param name="t">t unity units</param>
    /// <returns>The point</returns>
    public Vector3 GetPoint(float t)
    {
        return transform.position + Gradient * t;
    }
}
