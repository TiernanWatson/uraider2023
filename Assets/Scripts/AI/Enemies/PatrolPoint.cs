using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    public float WaitTime => _waitTime;

    [SerializeField] private float _waitTime = 0.0f;
}
