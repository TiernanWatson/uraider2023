using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//
// Obsolete - understanding transitions this way is too confusing without a visual editor
//

public static class Transition
{
    public static Transition<T>.Builder From<T>(params IState[] states)
    {
        return new Transition<T>.Builder().From(states);
    }

    public static Transition<T>.Builder FromAny<T>()
    {
        return new Transition<T>.Builder();
    }
}

public class Transition<T>
{
    /// <summary>
    /// Set of states that this transition can happen from, none means any
    /// </summary>
    public HashSet<IState> From { get; private set; } = new HashSet<IState>();

    /// <summary>
    /// States this transition cannot happen from
    /// </summary>
    public HashSet<IState> Exceptions { get; private set; } = new HashSet<IState>();

    /// <summary>
    /// State this transition leads to
    /// </summary>
    public IState To { get; private set; }

    /// <summary>
    /// Checks if this transition can be executed
    /// </summary>
    public Func<bool> Condition { get; private set; }

    /// <summary>
    /// Any code that should happen during the transition
    /// </summary>
    public Action Code { get; private set; }

    private Transition() 
    { 
    }

    /// <summary>
    /// Check if this transition can happen from a state
    /// </summary>
    /// <param name="state">Possible from state</param>
    /// <returns>True if state can use this transition</returns>
    public bool AppliesTo(IState state)
    {
        return !Exceptions.Contains(state) && (From.Count == 0 || From.Contains(state));
    }

    public class Builder
    {
        private Transition<T> transition;

        public Builder()
        {
            transition = new Transition<T>();
        }

        public Builder From(HashSet<IState> fromStates)
        {
            transition.From = fromStates;
            return this;
        }

        public Builder From(params IState[] fromStates)
        {
            transition.From = new HashSet<IState>(fromStates);
            return this;
        }

        public Builder To(IState toState)
        {
            transition.To = toState;
            return this;
        }

        public Builder Except(HashSet<IState> exceptions)
        {
            transition.Exceptions = exceptions;
            return this;
        }

        public Builder Except(params IState[] exceptions)
        {
            transition.Exceptions = new HashSet<IState>(exceptions);
            return this;
        }

        public Builder When(Func<bool> condition)
        {
            transition.Condition = condition;
            return this;
        }

        public Builder Execute(Action execute)
        {
            transition.Code = execute;
            return this;
        }

        public Transition<T> Get()
        {
            return transition;
        }
    }
}
