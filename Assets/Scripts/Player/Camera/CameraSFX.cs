using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSFX : MonoBehaviour
{
    [SerializeField] private AudioClip _underwaterSound;

    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    public void PlayUnderwater()
    {
        Debug.Log("Play underwater");
        _source.PlayOneShot(_underwaterSound);
    }

    public void StopUnderwater()
    {
        _source.Stop();
    }
}
