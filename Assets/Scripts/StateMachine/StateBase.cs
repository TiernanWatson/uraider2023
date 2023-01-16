using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void OnEnter();
    void OnExit();
    void Update();
}

public abstract class StateBase<T> : IState
{
    protected T _owner;

    public StateBase(T owner)
    {
        _owner = owner;
    }

    public virtual void OnEnter() { }

    public virtual void OnExit() { }

    public virtual void FixedUpdate() { }

    public abstract void Update();
}
