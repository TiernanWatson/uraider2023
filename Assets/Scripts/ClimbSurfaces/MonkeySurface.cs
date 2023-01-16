using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeySurface : MonoBehaviour
{
    public MonkeyConnection[] Connections => _connections;

    [SerializeField] private BoxCollider _collider;

    private MonkeyConnection[] _connections;

    private void Start()
    {
        _connections = GetComponents<MonkeyConnection>();
    }

    public Bounds GetBounds()
    {
        return _collider.bounds;
    }

    public Vector3 ClosetPointTo(Vector3 position)
    {
        return _collider.ClosestPoint(position);
    }
}
