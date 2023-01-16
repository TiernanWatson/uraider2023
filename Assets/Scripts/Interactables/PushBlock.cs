using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBlock : TriggerUseable
{
    public BoxCollider Collider => _boxCollider;

    #pragma warning disable 0649

    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private LayerMask _layers;

    #pragma warning restore 0649

    public override void Interact(PlayerController player)
    {
        if (player.StateMachine.State == player.BaseStates.Locomotion)
        {
            player.BaseStates.Locomotion.ReceivePushBlock(this);
        }
    }

    public override bool CanInteract(PlayerController player)
    {
        return player.StateMachine.State.CanPushBlock(this);
    }

    /// <summary>
    /// Returns the best normal axis to face for a point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Vector3 GetBestAxis(Vector3 point)
    {
        Vector3 point2 = point;
        point2.y = transform.position.y;
        Vector3 dir = (point2 - transform.position).normalized;

        Vector3 bestAxis = transform.forward;
        float bestAngle = Vector3.Angle(dir, transform.forward);

        Vector3[] axes = { transform.right, -transform.forward, -transform.right };
        foreach (var axis in axes)
        {
            float angle = Vector3.Angle(axis, dir);
            if (angle < bestAngle)
            {
                bestAngle = angle;
                bestAxis = axis;
            }
        }

        return bestAxis;
    }

    /// <summary>
    /// Check if this push block will fit in the position specified
    /// </summary>
    /// <param name="position">position to test</param>
    /// <returns></returns>
    public bool CanFit(Vector3 position)
    {
        Vector3 extents = Collider.size;
        extents.Scale(Collider.transform.lossyScale);
        extents /= 2.0f;
        extents += Vector3.forward * Mathf.Epsilon;
        extents += Vector3.right * Mathf.Epsilon;
        extents -= Vector3.up * 0.1f;

        Collider[] cols = Physics.OverlapBox(
            position,
            extents,
            transform.rotation,
            _layers.value,
            QueryTriggerInteraction.Ignore);

        bool canFit = true;
        foreach (var c in cols)
        {
            if (c != Collider)
            {
                Debug.Log("NO: " + c.gameObject.name);
                
                canFit = false;
            }
        }

        return canFit;
    }

    /// <summary>
    /// Returns the distance between block pivot and collider edge for another 
    /// object at position
    /// </summary>
    /// <param name="localAxis">Axis local to the object to get distance on</param>
    /// <returns></returns>
    public float GetEdgeToCenter(Vector3 localAxis)
    {
        return Vector3.Dot(_boxCollider.size, localAxis) / 2.0f;
    }
}
