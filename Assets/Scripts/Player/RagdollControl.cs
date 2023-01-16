using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollControl : MonoBehaviour
{
    [SerializeField] private Collider[] _colliders;

    private PlayerController _owner;
    private Rigidbody[] _rbs;

    private void Awake()
    {
        _owner = GetComponent<PlayerController>();
        _rbs = new Rigidbody[_colliders.Length];

        for (int i = 0; i < _colliders.Length; i++)
        {
            _rbs[i] = _colliders[i].GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        StopRag();
    }

    public void ApplyForceToAll(Vector3 force)
    {
        for (int i = 0; i < _colliders.Length; i++)
        {
            _rbs[i].AddForce(force);
        }
    }

    public void DoRag()
    {
        if (_owner)
        {
            _owner.AnimControl.Enabled = false;
            _owner.CharControl.enabled = false;
        }

        for (int i = 0; i < _colliders.Length; i++)
        {
            _colliders[i].enabled = true;
            _rbs[i].isKinematic = false;
            _rbs[i].useGravity = true;

            if (_owner)
            {
                float speedCap = 7.0f;
                float magnitude = Mathf.Clamp(_owner.Velocity.magnitude, 0.0f, speedCap);
                _rbs[i].velocity = _owner.Velocity.normalized * magnitude + _owner.transform.forward * magnitude;
            }
        }
    }

    public void StopRag()
    {
        for (int i = 0; i < _colliders.Length; i++)
        {
            //_colliders[i].enabled = false;
            _rbs[i].isKinematic = true;
            _rbs[i].useGravity = false;
        }
    }
}
