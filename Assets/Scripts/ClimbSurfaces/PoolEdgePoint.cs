using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SplineCollider))]
public class PoolEdgePoint : MonoBehaviour
{
    private SplineCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<SplineCollider>();
    }
}
