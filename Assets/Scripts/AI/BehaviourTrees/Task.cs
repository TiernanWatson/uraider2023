using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TaskState
{
    Running,
    Success,
    Fail
}

public interface ITask
{
    TaskState State { get; }

    TaskState Init();

    TaskState Update();

    bool CanUpdate();
}
