using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    [SerializeField] private float _initialHealth = 100.0f;
    [SerializeField] private float _initialBreath = 100.0f;

    public float Health { get; private set; }
    public float Breath { get; private set; }

    private void Awake()
    {
        Health = _initialHealth;
        Breath = _initialBreath;
    }

    public void ChangeHealth(float points)
    {
        Health += points;
        Health = Mathf.Clamp(Health, 0.0f, _initialHealth);

        OnHealthChanged?.Invoke(points);

        if (Health <= Mathf.Epsilon)
        {
            OnDeath?.Invoke();
        }
    }

    public void ChangeBreath(float points)
    {
        Breath += points;
        Breath = Mathf.Clamp(Breath, 0, _initialBreath);
    }

    public void DepleteHealth()
    {
        float change = -Health;
        Health = 0;

        OnHealthChanged?.Invoke(change);
        OnDeath?.Invoke();
    }

    public void Damage(float strength)
    {
        ChangeHealth(-strength);
    }
}
