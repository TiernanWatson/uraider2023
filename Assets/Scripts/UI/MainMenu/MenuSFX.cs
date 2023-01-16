using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSFX : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;

    private AudioSource _source;

    public void Sound()
    {
        _source.PlayOneShot(_clip);
    }
}
