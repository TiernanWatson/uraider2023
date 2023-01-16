using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : TriggerUseable
{
    public bool IsLocked => _isLocked;
    public bool CanUseCrowbar => _canUseCrowbar;

    [SerializeField] private bool _isLocked = true;
    [SerializeField] private bool _canUseCrowbar = false;
    [SerializeField] private KeyItem _key;

    public void Unlock()
    {
        _isLocked = false;
    }

    public bool IsKey(KeyItem key)
    {
        return _key == key;
    }

    public override void Interact(PlayerController player)
    {
        throw new System.NotImplementedException();
    }

    public override bool CanInteract(PlayerController player)
    {
        bool hasKey = _key ? player.Inventory.Contains(_key, out var details) : false;
        if (!hasKey)
        {
            hasKey = _canUseCrowbar && player.Inventory.Contains("Crowbar");
        }

        return hasKey && player.StateMachine.State.CanUseLock(this);
    }
}
