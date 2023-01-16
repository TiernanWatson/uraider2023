using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class TriggerUseable : MonoBehaviour, IUseable
{
    public Transform IconLocation => _iconLocation;

    [SerializeField] private Transform _iconLocation;

    protected PlayerController _player;

    private void Awake()
    {
        if (!_iconLocation)
        {
            _iconLocation = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _player = other.GetComponent<PlayerController>();
            _player.Interactions.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _player.Interactions.Remove(this);
        }
    }

    public virtual bool CanInteract(PlayerController player)
    {
        return true;
    }

    public abstract void Interact(PlayerController player);
}
