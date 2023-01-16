using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private Equipment[] _items;

    public void Equip(string name)
    {
        foreach (var item in _items)
        {
            if (item.Name.ToLower().Equals(name.ToLower()))
            {
                item.Equip();
            }
        }
    }

    public void Holster(string name)
    {
        foreach (var item in _items)
        {
            if (item.Name.ToLower().Equals(name.ToLower()))
            {
                item.Holster();
            }
        }
    }
}
