using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionZone : MonoBehaviour
{
    [SerializeField] private float _newDistance = 2.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            var camControl = player.CameraControl;

            camControl.Collision.ChangeDistance(true, _newDistance);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            var camControl = player.CameraControl;

            camControl.Collision.ChangeDistance(false);
        }
    }
}
