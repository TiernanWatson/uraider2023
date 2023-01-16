using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterZone : MonoBehaviour
{
    public float SurfaceHeight => _collider.center.y + _collider.size.y * 0.5f;
    public BoxCollider Collider => _collider;

    private BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enter water");
            var player = other.GetComponent<PlayerController>();
            player.StateMachine.State.OnWaterEnter(this);
        }
        else
        {
            var camSfx = other.GetComponent<CameraSFX>();
            camSfx?.PlayUnderwater();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.StateMachine.State.OnWaterStay(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.StateMachine.State.OnWaterExit(this);
        }
        else
        {
            var camSfx = other.GetComponent<CameraSFX>();
            camSfx?.StopUnderwater();
        }
    }

    public float GetTopPosition()
    {
        return transform.position.y + _collider.center.y + _collider.size.y / 2.0f;
    }
}
