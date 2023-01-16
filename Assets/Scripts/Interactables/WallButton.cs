using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WallButton : TriggerUseable
{
    public UnityEvent OnPress;

    private bool _isDown = false;
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        if (!_anim)
        {
            Debug.LogError("WallButton: " + gameObject.name + " was not able to find an animator in child objects");
        }
    }

    public void Push()
    {
        if (!_isDown)
        {
            _isDown = true;
            _anim.Play("ButtonDown");
            OnPress.Invoke();
        }
    }

    public override void Interact(PlayerController player)
    {
        player.StateMachine.State.TriggerButton(this);
    }

    public override bool CanInteract(PlayerController player)
    {
        return !_isDown && player.StateMachine.State.CanUseButton(this);
    }
}
