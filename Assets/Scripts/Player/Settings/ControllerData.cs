using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "URaider/ControllerData", order = 1)]
public class ControllerData : ScriptableObject
{
    [Header("Health Settings")]
    public float damageHeight = 6.0f;
    public float deathHeight = 10.0f;

    [Header("Movement Settings")]
    public float runSpeed = 3.501f;
    public float walkSpeed = 1.335f;
    public float maxAngularVel = 360.0f;
    public float groundingDistance = 0.4f;
    public float gravity = 18.0f;
    public float terminalSpeed = 40.0f;
    public float speedInterp = 10.0f;
    public float keyboardInterp = 5.0f;
    public float gamepadInterp = 25.0f;
    public float lerpToRate = 20.0f;
    public float locomotionTurnRate = 120.0f;
    public float crawlTurnRate = 45.0f;
    public float swimTurnRate = 80.0f;

    [Header("Door Anim Settings")]
    public float pullDoorRight = 0.85f;
    public float pullDoorForward = -0.4f;
    public float pushDoorRight = 0.6f;
    public float pushDoorForward = -0.4f;
    public float crowbarRight = 0.85f;
    public float crowbarForward = -0.4f;
    public float useKeyRight = 0.85f;
    public float useKeyForward = -0.4f;
    public float tryDoorRight = 0.85f;
    public float tryDoorForward = -0.4f;

    [Header("Jump Settings")]
    public float jumpHeight = 1.6f;
    public float jumpDistance = 4.5f;
    public float standJumpDistance = 3.5f;

    [Header("Ledge Climb Settings")]
    public float ledgeDownOffset = 1.9f;
    public float ledgeBackOffset = 0.11f;
    public float ledgeEndPadding = 0.15f;
    public float ledgeCornerPadding = 0.1f;
    public float ledgeCornerInPadding = 0.1f;
    public float ledgeJumpUpGrabOffset = 1.988f;
    public float ledgeJumpUpMinSpeed = 6.5f;
    public float ledgeJumpUpMaxDistance = 3.0f;
    public float ledgeJumpUpMaxDepth = 0.25f;

    [Header("Auto Grab Settings")]
    public float grabLedgeUpOffset = -2.09f;
    public float grabDownOffset = 1.75f;
    public float grabBackOffset = 0.03f;
    public float grabDropUpOffset = -1.338f;
    public float grabSDownOffset = 1.9f;
    public float grabSBackOffset = 0.11f;
    public float grabPoleSwingUpOffset = -1.85f;
    public float grabPoleSwingForwardOffset = 0.0f;
    public float maxLedgeAngle = 30.0f;
    public float jumpGrabError = 1.8f;

    [Header("Monkey Settings")]
    public float monkeyOffset = 2.1f;

    [Header("Swing Settings")]
    public float maxSwingJumpSpeed = 5.0f;
    public float minSwingJumpUpSpeed = 1.0f;
    public Material grappleMaterial;

    [Header("Ladder Settings")]
    public float ladderBackOffset = 0.38f;
    public float ladderUpOffset = 0.05f;

    [Header("Drainpipe Climb Settings")]
    public float drainpipeBackOffset = 0.38f;
    public float drainpipeDownOffset = 0.44f;
    public float drainpipeStartBackOffset = 0.38f;
    public float drainpipeStartRightOffset = -0.04371f;
    public float drainpipeStartUpOffset = 0.351608f;

    [Header("Pole Climb Settings")]
    public float poleGrabDownOffset = 2.04f;
    public float poleHandDownOffset = 2.09f;
    public float poleGrabMaxAngle = 30.0f;
    public float poleGrabBodyLength = 2.75f;  // Extra reach

    [Header("Wall Climb Settings")]
    public float wallclimbUpOffset = 0.15f;
    public float wallclimbBackOffset = 0.45f;

    [Header("Freeclimb Settings")]
    public float freeclimbUpOffset = -2.0f;
    public float freeclimbForwardOffset = -0.842f;

    [Header("Swim Settings")]
    public float jumpEnterOffset = 1.7f;
    public float treadYOffset = 1.7f;
    public float diveEnterDrag = 10.0f;
    public float minWadeDepth = 1.0f;
    public float maxWadeDepth = 1.7f;

    [Header("Block Push Settings")]
    public float pushStartOffset = 0.25f;

    [Header("Layer Masks")]
    public LayerMask ledgeLayers;
    public LayerMask groundLayers;

    public Vector3 GetWallclimbPosition(Vector3 point, Vector3 forward)
    {
        return point - forward * wallclimbBackOffset + Vector3.up * wallclimbUpOffset;
    }

    public Vector3 GetLadderPos(Vector3 point, Vector3 forward)
    {
        return point - forward * ladderBackOffset + Vector3.up * ladderUpOffset;
    }

    public Vector3 GetDPipeStartPos(Drainpipe dpipe)
    {
        Vector3 start = dpipe.transform.position;
        start += dpipe.transform.right * drainpipeStartRightOffset;
        start += dpipe.transform.up * drainpipeStartUpOffset;
        start -= dpipe.transform.forward * drainpipeStartBackOffset;

        return start;
    }

    public Vector3 GetLedgePosition(Vector3 point, Vector3 direction)
    {
        Vector3 result = point;
        result -= direction * ledgeBackOffset;
        result -= Vector3.up * ledgeDownOffset;

        return result;
    }

    public Vector3 GetGrabPosition(Vector3 point, Vector3 direction)
    {
        Vector3 result = point;
        result -= direction * grabBackOffset;
        result -= Vector3.up * grabDownOffset;

        return result;
    }

    public Vector3 GetStandGrabPosition(Vector3 point, Vector3 direction)
    {
        Vector3 result = point;
        result -= direction * grabSBackOffset;
        result -= Vector3.up * grabSDownOffset;

        return result;
    }

    public Vector3 GetFreeclimbPosition(Vector3 point, Vector3 forward, Vector3 up)
    {
        Vector3 result = point;
        result -= Vector3.up * freeclimbUpOffset;
        //result += forward * freeclimbForwardOffset;
        return result;
    }

    public Vector3 GetPullDoor(Vector3 point, Vector3 forward, Vector3 right)
    {
        return point + forward * pullDoorForward + right * pullDoorRight;
    }

    public Vector3 GetPushDoor(Vector3 point, Vector3 forward, Vector3 right)
    {
        return point + pushDoorForward * forward + pushDoorRight * right;
    }

    public Vector3 GetTryDoor(Vector3 point, Vector3 forward, Vector3 right)
    {
        return point + tryDoorForward * forward + tryDoorRight * right;
    }

    public Vector3 GetCrowbar(Vector3 point, Vector3 forward, Vector3 right)
    {
        return point + crowbarForward * forward + crowbarRight * right;
    }

    public Vector3 GetUseKey(Vector3 point, Vector3 forward, Vector3 right)
    {
        return point + useKeyForward * forward + useKeyRight * right;
    }
}