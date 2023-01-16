using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClimbUpType
{
    Stand,
    Crouch,
    Blocked
}

/// <summary>
/// Defines a surface that can shimmy'ed along, abstract interface
/// allows surfaces like freeclimb to create them
/// </summary>
public interface IShimmyable
{
    bool IsStart { get; }
    bool IsEnd { get; }
    bool HasWall { get; }
    ClimbUpType ClimbUp { get; }
    Vector3 Forward { get; }
    Vector3 Right { get; }
    Vector3 Position { get; }
    Vector3 Gradient { get; }
    IShimmyable Previous { get; }
    IShimmyable Next { get; }

    Vector3 GetPoint(float t);
    float ClosestParamTo(Vector3 point, Vector3 direction);
    float GetMaxT();
    bool IsBeyondEnd(Vector3 point);
}
