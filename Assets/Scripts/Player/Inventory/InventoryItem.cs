using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InventoryItem : TriggerUseable
{
    public string ItemName => _itemName;
    public bool DestroyOnUse => _destroyOnUse;
    public bool CanUseFromUI => _canUseFromUI;

    [SerializeField] private string _itemName;
    [SerializeField] private bool _destroyOnUse;
    [SerializeField] private bool _canUseFromUI;

    public override bool CanInteract(PlayerController player)
    {
        return player.StateMachine.State.CanPickup(this);
    }

    public override void Interact(PlayerController player)
    {
        player.StateMachine.State.TriggerPickup(this);
    }

    public void Pickup(PlayerController player)
    {
        if (player.Inventory.Add(this))
        {
            gameObject.layer = 12;
            foreach (Transform t in gameObject.transform)
            {
                t.gameObject.layer = 12;
            }
            gameObject.SetActive(false);

            // Trigger exit doesnt get called when setactive = false
            player.Interactions.Remove(this);
        }
    }

    /// <summary>
    /// Called when the player selects this item in the inventory
    /// </summary>
    /// <param name="player">Player that selected</param>
    public abstract void Click(PlayerController player);
}
