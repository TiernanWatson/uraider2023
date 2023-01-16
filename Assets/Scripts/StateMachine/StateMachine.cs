using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStateful<OwnerType, StateType> where StateType : IState
{
    StateMachine<OwnerType, StateType> StateMachine { get; }
}

public class StateMachine<OwnerType, StateType> where StateType : IState
{
    /// <summary>
    /// Invoked when State is changed, passes both old and new state
    /// </summary>
    public event Action<StateType, StateType> StateChanged;

    /// <summary>
    /// Current state the state machine is executing
    /// </summary>
    public StateType State { get; private set; }

    /// <summary>
    /// State that was last executed
    /// </summary>
    public StateType LastState { get; private set; }

    public StateMachine(StateType state)
    {
        State = state;
    }

    public void Begin()
    {
        State.OnEnter();
    }

    /// <summary>
    /// Runs the Update function of the current state
    /// </summary>
    public void Update()
    {
        State.Update();
    }

    public void ChangeState(StateType state)
    {
        Debug.Assert(state != null);

        LastState = State;

        State.OnExit();
        State = state;
        State.OnEnter();

        StateChanged?.Invoke(LastState, State);
    }
}
