using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleZone : MonoBehaviour
{
    public float MaxTetherLength => _maxTetherLength;

    [SerializeField] private float _maxTetherLength = 5.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.Triggers.Test(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.Triggers.Leave(this);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _maxTetherLength);
    }
}
