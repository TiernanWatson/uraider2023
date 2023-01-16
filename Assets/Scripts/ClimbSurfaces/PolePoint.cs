using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SplineCollider))]
public class PolePoint : MonoBehaviour
{
    public SplineCollider Collider => _collider;

    // Collider used for detection by raycasts
    private SplineCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<SplineCollider>();
    }

    public static bool FindPole(Vector3 position, float maxHeight, int layers, out PolePoint pole)
    {
        Ray ray = new Ray(position, Vector3.up);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxHeight,
            layers,
            QueryTriggerInteraction.Collide))
        {
            if (hit.collider.CompareTag("Pole"))
            {
                pole = hit.collider.GetComponent<PolePoint>();
                return true;
            }
        }

        pole = null;
        return false;
    }

    public static PolePoint FindPoleInRange(
        Vector3 start,
        Vector3 direction,
        float distance,
        float maxHeight,
        float maxAngle,
        int layers,
        float increment = 0.25f)
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
                float angle = Vector3.Angle(direction, hit.transform.right);

                if (hit.collider.CompareTag("Pole") && angle < maxAngle)
                {
                    return hit.collider.GetComponent<PolePoint>();
                }
            }
        }

        return null;
    }
}
