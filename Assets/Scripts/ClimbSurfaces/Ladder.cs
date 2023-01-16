using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RungMaker))]
public class Ladder : MonoBehaviour
{
    public bool CanClimbOff => _canClimbOff;
    public bool CanStepOff => _canStepOff;
    public int[] LedgeRungs => _ledgeRungs;
    public RungMaker Rungs { get; private set; }

    [SerializeField] private bool _canClimbOff;
    [SerializeField] private bool _canStepOff;
    [SerializeField] private int[] _ledgeRungs;

    private void Awake()
    {
        Rungs = GetComponent<RungMaker>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            //player.StateMachine.State.TriggerLadder(this);
            player.Triggers.Test(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            //player.StateMachine.State.TriggerLadder(this);
            player.Triggers.Leave(this);
        }
    }

    public bool LedgeAt(int rung)
    {
        foreach (int i in _ledgeRungs)
        {
            if (i == rung)
            {
                return true;
            }
        }

        return false;
    }
}
