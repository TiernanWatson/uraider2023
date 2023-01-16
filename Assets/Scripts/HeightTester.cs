using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeightTester : MonoBehaviour
{
    private float _previousHeight = 0.0f;
    private PlayerController _player;

    private void Start()
    {
        _player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_player.UInput.Interact.triggered &&
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f))
        {
            Debug.Log("Height: " + (hit.point.y - _previousHeight));
            _previousHeight = hit.point.y;
        }
    }
}
