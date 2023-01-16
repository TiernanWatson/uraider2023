using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowbarItem : InventoryItem
{
    public Equipment HandItem => _handItem;

    [SerializeField] private Equipment _handItem;

    public override void Click(PlayerController player)
    {
        
    }
}
