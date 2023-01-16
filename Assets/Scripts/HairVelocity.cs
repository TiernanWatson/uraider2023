using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairVelocity : MonoBehaviour
{
    [SerializeField] private Transform _follow;

    private Rigidbody _rb;
    private Vector3 _lastPos;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _lastPos = _follow.position;
    }

    private void FixedUpdate()
    {
        Vector3 newPos = _follow.position;
        Vector3 change = newPos - _lastPos;
        _lastPos = newPos;
    }
}
