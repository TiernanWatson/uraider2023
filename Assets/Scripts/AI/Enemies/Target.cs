using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Transform AimPoint => _aimPoint;

    [SerializeField] private Transform _aimPoint;
}
