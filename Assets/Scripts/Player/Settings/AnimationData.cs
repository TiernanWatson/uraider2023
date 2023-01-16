using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains all the various offsets used for animations.
/// Vector3 members are generally local, e.g. for ledges x is right along ledge
/// </summary>
[CreateAssetMenu(fileName = "AnimationData", menuName = "URaider/AnimationData", order = 2)]
public class AnimationData : ScriptableObject
{
    [Header("Auto Grab From Locomotion")]
    public AnimationCurve grabOffsetForward;
    public AnimationCurve grabOffsetRight;
    public AnimationCurve grabOffsetUp;
    public float grabOffsetStandForward;
    public float grabOffsetStandUp;
    public float grabOffsetPoleForward;
    public float grabOffsetPoleUp;

    [Header("Auto Grab From Ledge")]
    public float grabOffsetJumpUpUp;
    public float grabOffsetJumpUpForward;
    public float grabOffsetDropDownUp;
    public float grabOffsetDropDownForward;
    public float grabOffsetDropDownNoWallUp;
    public float grabOffsetDropDownNoWallForward;

    public Vector3 GetGrabLocation(Vector3 grabPoint, Vector3 forward, Vector3 right, float grabAngle)
    {
        grabAngle = Mathf.Abs(grabAngle);

        float forwardOffset = grabOffsetForward.Evaluate(grabAngle);
        float rightOffset = grabOffsetRight.Evaluate(grabAngle);
        float upOffset = grabOffsetUp.Evaluate(grabAngle);

        return grabPoint + forward * forwardOffset + right * rightOffset + Vector3.up * upOffset;
    }

    public Vector3 GetStandGrabLocation(Vector3 grabPoint, Vector3 forward)
    {
        return grabPoint + forward * grabOffsetStandForward + Vector3.up * grabOffsetStandUp;
    }

    public Vector3 GetGrabNoWallLocation(Vector3 grabPoint, Vector3 forward, Vector3 right, float grabAngle)
    {
        grabAngle = Mathf.Abs(grabAngle);

        float forwardOffset = grabOffsetForward.Evaluate(grabAngle);
        float rightOffset = grabOffsetRight.Evaluate(grabAngle);
        float upOffset = grabOffsetStandUp;

        return grabPoint + forward * forwardOffset + right * rightOffset + Vector3.up * upOffset;
    }

    public Vector3 GetJumpUpLocation(Vector3 grabPoint, Vector3 forward)
    {
        return grabPoint + Vector3.up * grabOffsetJumpUpUp + forward * grabOffsetJumpUpForward;
    }

    public Vector3 GetDropDownLocation(Vector3 grabPoint, Vector3 forward)
    {
        return grabPoint + Vector3.up * grabOffsetDropDownUp + forward * grabOffsetDropDownForward;
    }

    public Vector3 GetDropDownNoWallLocation(Vector3 grabPoint, Vector3 forward)
    {
        return grabPoint + Vector3.up * grabOffsetDropDownNoWallUp + forward * grabOffsetDropDownNoWallForward;
    }

    public Vector3 GetPoleGrabLocation(Vector3 grabPoint, Vector3 forward)
    {
        return grabPoint + Vector3.up * grabOffsetPoleUp + forward * grabOffsetPoleForward;
    }
}
