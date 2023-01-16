using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RungMaker))]
public class Drainpipe : MonoBehaviour
{
    public LedgePoint LeftLedge => _leftLedge;
    public LedgePoint RightLedge => _rightLedge;
    public RungMaker Rungs { get; private set; }

    #pragma warning disable 0649

    [SerializeField] private LedgePoint _leftLedge;
    [SerializeField] private LedgePoint _rightLedge;

#pragma warning restore 0649

    private void Awake()
    {
        Rungs = GetComponent<RungMaker>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.Triggers.Test(this);

            /*if (player.StateMachine.State == player.BaseStates.Locomotion)
            {
                player.BaseStates.Locomotion.ReceiveDrainpipe(this);
            }*/
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.Triggers.Leave(this);

            /*if (player.StateMachine.State == player.BaseStates.Locomotion)
            {
                player.BaseStates.Locomotion.ReceiveDrainpipe(this);
            }*/
        }
    }
}
