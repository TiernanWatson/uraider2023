using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVFX : MonoBehaviour
{
    [SerializeField] private GameObject _flashes;
    [SerializeField] private float _flashDuration = 0.1f;

    private float _showTime;
    private Weapon _weapon;

    private void Awake()
    {
        _weapon = GetComponent<Weapon>();
    }

    private void OnEnable()
    {
        HideFlash();
    }

    private void OnDisable()
    {
        HideFlash();
    }

    private void Start()
    {
        _weapon.Fired += ShowFlash;
    }

    private void Update()
    {
        if (Time.time - _showTime > _flashDuration)
        {
            HideFlash();
        }
    }

    private void ShowFlash()
    {
        _showTime = Time.time;
        _flashes.SetActive(true);
    }

    private void HideFlash()
    {
        _flashes.SetActive(false);
    }
}
