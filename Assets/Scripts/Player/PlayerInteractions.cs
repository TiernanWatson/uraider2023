using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerController))]
public class PlayerInteractions : MonoBehaviour
{
    public UnityEvent CanInteract;
    public UnityEvent NoInteract;

    public TriggerUseable Closest => _closest;

    private bool _canInteract;
    private PlayerController _player;
    // Overlaps that may or may not be useable
    private List<TriggerUseable> _allOverlaps;
    // Overlaps that are confirmed useable - updated each frame
    private List<TriggerUseable> _useableOverlaps;
    private TriggerUseable _closest;

    private void Awake()
    {
        _canInteract = false;
        _player = GetComponent<PlayerController>();
        _useableOverlaps = new List<TriggerUseable>();
        _allOverlaps = new List<TriggerUseable>();
    }

    private void Update()
    {
        Sort();
        UpdateClosest();
        CheckStatus();
    }

    public bool Use()
    {
        if (_closest)
        {
            _closest.Interact(_player);
            return true;
        }

        return false;
    }

    public void Add(TriggerUseable useable)
    {
        _allOverlaps.Add(useable);
    }

    public void Remove(TriggerUseable useable)
    {
        if (_allOverlaps.Contains(useable))
        {
            _allOverlaps.Remove(useable);
        }
    }

    private void UpdateClosest()
    {
        if (_useableOverlaps.Count == 0)
        {
            _closest = null;
            return;
        }

        _closest = _useableOverlaps[0];
        float distClosest = Mathf.Infinity;

        for (int i = 0; i < _useableOverlaps.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, _useableOverlaps[i].transform.position);
            if (distance < distClosest)
            {
                _closest = _useableOverlaps[i];
                distClosest = distance;
            }
        }
    }

    private void Sort()
    {
        _useableOverlaps.Clear();
        foreach (var u in _allOverlaps)
        {
            if (u.CanInteract(_player))
            {
                _useableOverlaps.Add(u);
            }
        }
    }

    private void CheckStatus()
    {
        if (_canInteract && !_closest)
        {
            _canInteract = false;
            NoInteract.Invoke();
        }
        else if (!_canInteract && _closest)
        {
            _canInteract = true;
            CanInteract.Invoke();
        }
    }
}
