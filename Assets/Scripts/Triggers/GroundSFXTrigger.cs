using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSFXTrigger : MonoBehaviour
{
    [SerializeField] private bool _resetOnExit = true;
    [SerializeField] private GroundSFX _type;

    private GroundSFX _original;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.SFX.GroundType = _type;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_resetOnExit && other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.SFX.GroundType = _original;
        }
    }
}
