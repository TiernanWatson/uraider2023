using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Damages any component implementing IDamageable every second
/// </summary>
public class HurtZone : MonoBehaviour
{
    [SerializeField] private float _hurtPerSecond = 10;

    // Objects currently in trigger and time they were last damaged by this
    private Dictionary<IDamageable, float> _objLastHurt;
    private List<IDamageable> _overlapping;

    private void Awake()
    {
        _objLastHurt = new Dictionary<IDamageable, float>();
        _overlapping = new List<IDamageable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable obj = other.GetComponent<IDamageable>();

        if (obj != null)
        {
            _objLastHurt.Add(obj, Time.time);
            _overlapping.Add(obj);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IDamageable obj = other.GetComponent<IDamageable>();

        if (obj != null)
        {
            _objLastHurt.Remove(obj);
            _overlapping.Remove(obj);
        }
    }

    private void Update()
    {
        foreach (var obj in _overlapping)
        {
            obj.Damage(_hurtPerSecond * Time.deltaTime);
        }
        /*var toUpdate = new List<IDamageable>();

        foreach (var pair in _objLastHurt)
        {
            float timeDifference = Time.time - pair.Value;

            if (timeDifference > 1.0f)
            {
                pair.Key.Damage(_hurtPerSecond);
                toUpdate.Add(pair.Key);
            }
        }

        // Unsafe to update dictionary in above loop
        foreach (var obj in toUpdate)
        {
            _objLastHurt[obj] = Time.time;
        }*/
    }
}
