using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vault : MonoBehaviour
{
    public bool GroundForward => _groundForward;
    public bool GroundBack => _groundBack;
    public SplineCollider Collider => _collider;

    [SerializeField] private bool _groundForward;
    [SerializeField] private bool _groundBack;

    private SplineCollider _collider;

    private void Start()
    {
        _collider = GetComponent<SplineCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var vaults = other.GetComponent<PlayerVaults>();
            vaults.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var vaults = other.GetComponent<PlayerVaults>();
            vaults.Remove(this);
        }
    }
}
