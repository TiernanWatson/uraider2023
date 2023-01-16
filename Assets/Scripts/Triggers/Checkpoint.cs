using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CheckpointManager.Instance.Current != this)
        {
            var player = other.GetComponent<PlayerController>();
            player.StateMachine.State.OnCheckpoint(this);
            CheckpointManager.Instance.Overlap(this);
        }
    }
}
