using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainFollow : MonoBehaviour
{
    [SerializeField] private Transform _follow;
    [SerializeField] private Vector3 _offset;

    private void Update()
    {
        Vector3 rotated = _follow.rotation * _offset;
        transform.position = _follow.position + rotated;
    }
}
