using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [SerializeField] private float zoomRate = 30.0f;
    [SerializeField] private float retreatRate = 14.0f;
    [SerializeField] private float pivotAdjustRate = 14.0f;
    [SerializeField] private float sphereRadius = 0.1f;
    [SerializeField] private float pivotMaxHeight = 1.6f;

    [SerializeField] private Transform pivot;
    [SerializeField] private Transform cam;
    [SerializeField] private LayerMask collisionLayers;

    private float _currentDistance;
    private float _initDistance;
    private float _maxPivotAdjustment;
    private Vector3 _initPivotPosition;

    private void Start()
    {
        _currentDistance = _initDistance = Vector3.Distance(pivot.position, cam.position);
        _initPivotPosition = pivot.localPosition;
        _maxPivotAdjustment = pivotMaxHeight - _initPivotPosition.y;
    }

    public void SetPosition(Vector3 pivotPosition)
    {
        // First find initial collision and pull in
        AdjustCamera(pivot.parent.TransformPoint(pivotPosition));
        AdjustPivotHeight(pivotPosition);

        // Then check if something was jutting out we are clipping now
        AdjustCamera(pivot.transform.position, false);
    }

    public void ChangeDistance(bool active, float distance = 1.0f)
    {
        if (active)
        {
            _currentDistance = distance;
        }
        else
        {
            _currentDistance = _initDistance;
        }
    }

    /// <summary>
    /// Change the height of the camera based on how close we are to character
    /// </summary>
    /// <param name="pivotPosition"></param>
    private void AdjustPivotHeight(Vector3 pivotPosition)
    {
        // 1.0f = normal, 0.0f = fully up
        float camDistancePercent = Vector3.Distance(cam.position, pivot.position) / _currentDistance;

        float newHeight = Mathf.Lerp(_maxPivotAdjustment, 0.0f, camDistancePercent);
        Vector3 targetPivotPos = pivotPosition + Vector3.up * newHeight;

        pivot.localPosition = Vector3.Lerp(pivot.localPosition, targetPivotPos, Time.deltaTime * pivotAdjustRate);
    }

    /// <summary>
    /// Pull camera in from clipping
    /// </summary>
    /// <param name="rayStart"></param>
    private void AdjustCamera(Vector3 rayStart, bool canRetreat = true)
    {
        //Vector3 rayStart = pivot.transform.position;
        Vector3 dir = -pivot.transform.forward;

        int allButPlayer = collisionLayers.value;
        if (Physics.SphereCast(rayStart, sphereRadius, dir, out RaycastHit hit, _currentDistance, allButPlayer, QueryTriggerInteraction.Ignore))
        {
            float pointOffset = (hit.point - pivot.position).magnitude;
            cam.localPosition = Vector3.Lerp(cam.localPosition, cam.localPosition.normalized * pointOffset, Time.deltaTime * zoomRate);
        }
        else if (canRetreat)
        {
            cam.localPosition = Vector3.Lerp(cam.localPosition, cam.localPosition.normalized * _currentDistance, Time.deltaTime * retreatRate);
        }
    }
}
