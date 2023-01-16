using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the invisible dummy hair root and applies to real skeleton
/// Not applied to real skeleton because anim system messes with it
/// </summary>
public class HairTrail : MonoBehaviour
{
    [SerializeField] private Transform _realPony;

    private void LateUpdate()
    {
        _realPony.transform.position = transform.position;
        _realPony.transform.rotation = transform.rotation;
        //_realPony.transform.localScale = transform.localScale;
    }
}
