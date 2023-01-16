using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public UnityEvent OnOverlap;

    public Door[] Doors => _doors;
    public InventoryItem[] Items => _items;
    public Human[] Enemies => _enemies;

    public Checkpoint Current => _current;

    private Checkpoint _current;

    [SerializeField] private Door[] _doors;
    [SerializeField] private InventoryItem[] _items;
    [SerializeField] private Human[] _enemies;

    private void Awake()
    {
        Instance = this;
    }

    public void Overlap(Checkpoint checkpoint)
    {
        if (checkpoint != _current)
        {
            _current = checkpoint;
            OnOverlap?.Invoke();
            LevelManager.Instance.SaveLevel();
        }
    }
}
