using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSFX : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;

    private Weapon _weapon;
    private AudioSource _source;

    private void Start()
    {
        _source = GetComponent<AudioSource>();
        _weapon = GetComponent<Weapon>();
        _weapon.Fired += PlayShot;
    }

    public void PlayShot()
    {
        _source.PlayOneShot(_clip);
    }
}
