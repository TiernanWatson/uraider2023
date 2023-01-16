using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to have a collider to be used in fixed update follow a transform
/// </summary>
public class PhysicsColliderFollow : MonoBehaviour
{
    [SerializeField] private Transform _follow;

    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();

        Debug.AssertFormat(_collider, "Could not find collider on follow script");
    }

    private void FixedUpdate()
    {
        transform.position = _follow.position;
        transform.rotation = _follow.rotation;
    }
}
