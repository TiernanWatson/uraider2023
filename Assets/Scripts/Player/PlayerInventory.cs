using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public class ItemInfo
    {
        public InventoryItem Item { get; private set; }
        public int Count { get; set; }

        public ItemInfo(InventoryItem item, int count)
        {
            this.Item = item;
            this.Count = count;
        }
    }

    public event Action<InventoryItem> OnItemAdded;

    public List<ItemInfo> Items => _items;

    [SerializeField] private int _maxItems;
    [SerializeField] private List<InventoryItem> _startItems;
    
    private List<ItemInfo> _items;

    private void Awake()
    {
        _items = new List<ItemInfo>();

        foreach (var item in _startItems)
        {
            Add(item);
            item.gameObject.SetActive(false);
        }
    }

    public bool Add(InventoryItem item)
    {
        if (_items.Count < _maxItems)
        {
            if (Contains(item, out var details))
            {
                details.Count++;
            }
            else
            {
                _items.Add(new ItemInfo(item, 1));
            }

            OnItemAdded?.Invoke(item);
            return true;
        }

        return false;
    }

    public bool Contains(string name)
    {
        foreach (var i in _items)
        {
            if (i.Item.ItemName.Equals(name))
            {
                return true;
            }
        }

        return false;
    }

    public bool Contains(string name, out ItemInfo details)
    {
        foreach (var i in _items)
        {
            if (i.Item.ItemName.Equals(name))
            {
                details = i;
                return true;
            }
        }

        details = null;
        return false;
    }

    public bool Contains(InventoryItem item, out ItemInfo details)
    {
        foreach (var info in _items)
        {
            if (info.Item.ItemName.Equals(item.ItemName))
            {
                details = info;
                return true;
            }
        }

        details = null;
        return false;
    }

    public bool Remove(InventoryItem item)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Item.ItemName.Equals(item.ItemName))
            {
                if (_items[i].Count > 1)
                {
                    _items[i].Count--;
                }
                else
                {
                    _items.RemoveAt(i);
                }

                return true;
            }
        }

        return false;
    }
}
