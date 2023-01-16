using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedipackItem : InventoryItem
{
    [SerializeField] private int _healthPoints;

    public override void Click(PlayerController player)
    {
        player.Stats.ChangeHealth(_healthPoints);
    }
}
