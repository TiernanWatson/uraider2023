using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : TriggerUseable
{
    public bool IsOpen => _isOpen;
    public bool IsPush => _isPush;
    public bool IsLocked => _isLocked;
    public bool CanUseCrowbar => _canUseCrowbar;
    public KeyItem Key => _key;

    [SerializeField] private bool _useable = true;
    [SerializeField] private bool _isPush = false;
    [SerializeField] private bool _isLocked;
    [SerializeField] private bool _canUseCrowbar;
    [SerializeField] private KeyItem _key;
    [SerializeField] private Animator _doorAnim;
    [SerializeField] private AudioClip _sound;
    [SerializeField] private AudioClip _handleSound;

    private bool _isOpen = false;
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    public void PlayOpenSound()
    {
        if (_sound)
        {
            _source.PlayOneShot(_sound);
        }
    }

    public override void Interact(PlayerController player)
    {
        player.UseDoor(this);
    }

    public void OpenPull()
    {
        if (_handleSound)
        {
            _source.PlayOneShot(_handleSound);
        }
        _doorAnim.Play("DoorPullR");
        _isOpen = true;
    }

    public void OpenPush()
    {
        if (_handleSound)
        {
            _source.PlayOneShot(_handleSound);
        }
        _doorAnim.Play("DoorOpenBack");
        _isOpen = true;
    }

    public void Unlock()
    {
        _isLocked = false;
    }

    public override bool CanInteract(PlayerController player)
    {
        return _useable && !_isOpen && player.StateMachine.State.CanUseDoor(this);
    }
}
