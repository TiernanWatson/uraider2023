using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sometimes animations can make the ponytail enter a collider and jerk.
/// This script fixes that.
/// </summary>
public class PonytailCollisionFixer : MonoBehaviour
{
    [SerializeField] private Transform _corresponding;
    [SerializeField] private LayerMask _collisionLayers;
    [SerializeField] private Collider[] _ignoreThese;

    private SphereCollider[] _ponytailParts;
    private SphereCollider[] _correspondingParts;

    private void Awake()
    {
        _ponytailParts = transform.GetComponentsInChildren<SphereCollider>();
        _correspondingParts = _corresponding.GetComponentsInChildren<SphereCollider>();
    }

    private void Start()
    {
        foreach (var pony in _ponytailParts)
        {
            foreach (var c in _ignoreThese)
            {
                // Stop compound collider issue
                Physics.IgnoreCollision(c, pony, true);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log(gameObject.name + " hitting: " + collision.gameObject.name);
        //Debug.Log("Collisions off: " + Physics.GetIgnoreLayerCollision(gameObject.layer, collision.gameObject.layer));
        //Debug.Log("Collisions off individual: " + Physics.GetIgnoreCollision(GetComponent<Collider>(), collision.collider));
        //Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
    }

    private void LateUpdate()
    {
        if (_corresponding)
        {
            for (int i = 0; i < _ponytailParts.Length; i++)
            {
                _correspondingParts[i].transform.position = _ponytailParts[i].transform.position;
            }
            //transform.position = _corresponding.position;
        }
        return;
        // This is done in LateUpdate because its the animator moving the colliders into the chest, not physics
        for (int i = 0; i < _ponytailParts.Length; i++)
        {
            var part = _ponytailParts[i];

            foreach (var col in Physics.OverlapSphere(part.transform.position, part.radius, _collisionLayers.value, QueryTriggerInteraction.Ignore))
            {
                if (col.gameObject.name.Contains("PONY"))
                {
                    continue;
                }

                if (Physics.ComputePenetration(part, part.transform.position, part.transform.rotation, col, col.transform.position, col.transform.rotation, out Vector3 dir, out float dist))
                {
                    if (dist < 0.01f)
                        continue;

                    part.transform.position += dir * dist;
                    Debug.Log("Resolved a pony collision with: " + part.gameObject.name + " and " + col.gameObject.name + " by " + dist);
                }
            }
        }
    }
}
