using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PonytailSimulation : MonoBehaviour
{
    [SerializeField] private Transform _followBone;
    [SerializeField] private Rigidbody _anchor;

    private Rigidbody[] _rbs;

    private void Awake()
    {
        _rbs = GetComponentsInChildren<Rigidbody>();

        Debug.Assert(_rbs.Length > 0, "Could not find ponytail rigidbodies");
    }

    private void FixedUpdate()
    {
        _anchor.MovePosition(_followBone.position);
        _anchor.MoveRotation(_followBone.rotation);

        foreach (var rb in _rbs)
        {
            rb.AddForce(Vector3.down * 100.0f, ForceMode.Force);
        }
    }
}
