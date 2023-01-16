using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstracts away Mecanim and allows controller to issue animation commands
/// </summary>
public class PlayerAnim : MonoBehaviour
{
    public enum JumpType
    {
        Run,
        Dive,
        Stand,
        Up,
        Backflip,
        Sideflip,
        LedgeBack,
        LedgeUp,
        LedgeSide,
        Pole,
        Grapple,
        None
    }

    // Invoked by Unity animation events
    public event Action HandInteract;
    public event Action HandPullDoor;
    public event Action HandPushDoor;
    public event Action GunsDrawn;
    public event Action GunsHolstered;
    public event Action LeftEquipUsed;
    public event Action RightEquipUsed;
    public event Action PickUp;

    public bool Enabled
    {
        get { return _anim.enabled; }
        set { _anim.enabled = value; }
    }

    public bool IsTargetMatching { get; private set; }

    public bool ApplyRootMotion
    {
        get { return _anim.applyRootMotion; }
        set { _anim.applyRootMotion = value; }
    }

    public bool HasLegRoom
    {
        get { return _anim.GetBool("HasLegRoom"); }
        set { _anim.SetBool("HasLegRoom", value); }
    }

    public bool IsGrounded
    {
        get { return _anim.GetBool("IsGrounded"); }
        set { _anim.SetBool("IsGrounded", value); }
    }

    public bool InCombat
    {
        get { return _anim.GetBool("IsCombat"); }
        set { _anim.SetBool("IsCombat", value); }
    }

    public bool IsCrouching
    {
        get { return _anim.GetBool("IsCrouching"); }
        set { _anim.SetBool("IsCrouching", value); }
    }

    public float AimAngle
    {
        get { return _anim.GetFloat("AimAngle"); }
        set { _anim.SetFloat("AimAngle", value); }
    }

    public float AimAngleUp
    {
        get { return _anim.GetFloat("AimAngleUp"); }
        set { _anim.SetFloat("AimAngleUp", value); }
    }

    public float GrabAngle
    {
        get { return _anim.GetFloat("GrabAngle"); }
        set { _anim.SetFloat("GrabAngle", value); }
    }

    public int RungsFromTop
    {
        get { return _anim.GetInteger("RungsFromTop"); }
        set { _anim.SetInteger("RungsFromTop", value); }
    }

    public int RungsFromBottom
    {
        get { return _anim.GetInteger("RungsFromBottom"); }
        set { _anim.SetInteger("RungsFromBottom", value); }
    }

    public bool IsFiring
    {
        get { return _anim.GetBool("IsFiring"); }
        set { _anim.SetBool("IsFiring", value); }
    }

    public bool IsLedgeEnd
    {
        get { return _anim.GetBool("IsLedgeEnd"); }
        set { _anim.SetBool("IsLedgeEnd", value); }
    }

    public bool IsDiving
    {
        get { return _anim.GetBool("IsDiving"); }
        set { _anim.SetBool("IsDiving", value); }
    }

    public bool IsReaching
    {
        get { return _anim.GetBool("IsReaching"); }
        set { _anim.SetBool("IsReaching", value); }
    }

    public bool IsSwimming
    {
        get { return _anim.GetBool("IsSwimming"); }
        set { _anim.SetBool("IsSwimming", value); }
    }

    public bool IsJumping
    {
        get { return _anim.GetBool("IsJumping"); }
        set { _anim.SetBool("IsJumping", value); }
    }

    public bool IsPushing
    {
        get { return _anim.GetBool("IsPushing"); }
        set { _anim.SetBool("IsPushing", value); }
    }

    public bool IsWading
    {
        get { return _anim.GetBool("IsWading"); }
        set { _anim.SetBool("IsWading", value); }
    }

    public bool IsWallclimb
    {
        get { return _anim.GetBool("IsWallclimb"); }
        set { _anim.SetBool("IsWallclimb", value); }
    }

    public bool OnTopRung
    {
        get { return _anim.GetBool("OnTopRung"); }
        set { _anim.SetBool("OnTopRung", value); }
    }

    public bool OnBottomRung
    {
        get { return _anim.GetBool("OnBottomRung"); }
        set { _anim.SetBool("OnBottomRung", value); }
    }

    public bool CanClimbUp
    {
        get { return _anim.GetBool("CanClimbUp"); }
        set { _anim.SetBool("CanClimbUp", value); }
    }

    public bool CanClimbDown
    {
        get { return _anim.GetBool("CanClimbDown"); }
        set { _anim.SetBool("CanClimbDown", value); }
    }

    public Vector3 DeltaPosition
    {
        get { return _anim.deltaPosition; }
    }

    public float SpeedMultiplier
    {
        get { return _anim.GetFloat("SpeedMultiplier"); }
        set { _anim.SetFloat("SpeedMultiplier", value); }
    }

    public float Speed
    {
        get { return _anim.GetFloat("Speed"); }
        set { _anim.SetFloat("Speed", value); }
    }

    public float Forward
    {
        get { return _anim.GetFloat("Forward"); }
        set { _anim.SetFloat("Forward", value); }
    }

    public float Right
    {
        get { return _anim.GetFloat("Right"); }
        set { _anim.SetFloat("Right", value); }
    }

    public float Horizontal => _anim.GetFloat("Horizontal");
    public float Vertical => _anim.GetFloat("Vertical");

    public float SwimAngle
    {
        get { return _anim.GetFloat("SwimAngle"); }
        set { _anim.SetFloat("SwimAngle", value); }
    }

    public float SwingAngle
    {
        get { return _anim.GetFloat("SwingAngle"); }
        set { _anim.SetFloat("SwingAngle", value); }
    }

    public int ReachDirection
    {
        get { return _anim.GetInteger("ReachDirection"); }
        set { _anim.SetInteger("ReachDirection", value); }
    }

    public float TargetAngle
    {
        get { return _anim.GetFloat("TargetAngle"); }
    }

    public float TargetSpeed
    {
        get { return _anim.GetFloat("TargetSpeed"); }
    }

    public float NormTime
    {
        get { return _normalizedTime; }
    }

    public float RightFootIK => _anim.GetFloat("RightFootIK");
    public float LeftFootIK => _anim.GetFloat("LeftFootIK");

    private float _normalizedTime;
    private Animator _anim;
    private AnimatorStateInfo _animState;
    private AnimatorTransitionInfo _transInfo;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public bool IsTag(string tag)
    {
        return _animState.IsTag(tag);
    }

    public void UpdateState()
    {
        _animState = _anim.GetCurrentAnimatorStateInfo(0);
        _transInfo = _anim.GetAnimatorTransitionInfo(0);
        _normalizedTime = _animState.normalizedTime % 1.0f;
        _anim.SetFloat("NormTime", _normalizedTime);
    }

    public void HandOnInteract()
    {
        HandInteract?.Invoke();
        HandInteract = null;
    }

    public void HandOnPullDoor()
    {
        HandPullDoor?.Invoke();
        HandPullDoor = null;
        HandPushDoor = null;
    }

    public void HandOnPushDoor()
    {
        HandPushDoor?.Invoke();
        HandPushDoor = null;
        HandPullDoor = null;
    }

    public void FadeTo(string name, float fixedTime)
    {
        _anim.CrossFadeInFixedTime(name, fixedTime);
    }

    public void FadeToNormalized(string name, float normTime)
    {
        _anim.CrossFade(name, normTime);
    }

    public void FadeTo(string name, float fixedTime, float offset)
    {
        _anim.CrossFadeInFixedTime(name, fixedTime, -1, offset);
    }

    public void Play(string name)
    {
        _anim.Play(name);
    }

    public void InputUpdate(float horizontal, float vertical)
    {
        //_anim.SetFloat("Horizontal", horizontal);
        //_anim.SetFloat("Vertical", vertical);
    }

    /// <summary>
    /// Called once per frame to update player's velocity variables
    /// </summary>
    public void VelocityUpdate(PlayerMovement movement, Transform player)
    {
        _anim.SetFloat("Speed", movement.Velocity.magnitude);
        _anim.SetFloat("TargetSpeed", movement.TargetVelocity.magnitude);
        _anim.SetFloat("UpVelocity", movement.Velocity.y);

        Vector3 targetForward = movement.TargetRotation * Vector3.forward;
        Vector3 currentForward = movement.Rotation * Vector3.forward;
        float signedAngle = Vector3.SignedAngle(currentForward, targetForward, Vector3.up);

        _anim.SetFloat("SignedTargetAngle", signedAngle);
        float absAngle = Mathf.Abs(signedAngle);
        _anim.SetFloat("TargetAngle", absAngle);

        // Actual movement calculated in move mode
        Vector3 playerRelative = player.InverseTransformVector(movement.TargetVelocity);

        _anim.SetFloat("Forward", playerRelative.z);
        _anim.SetFloat("Right", playerRelative.x);

        // Input values regardless of movement mode calculation
        Vector3 inputRelative = player.InverseTransformVector(movement.GetCameraRotaterY() * movement.UInput.MoveInputRaw);

        _anim.SetFloat("Horizontal", inputRelative.x);
        _anim.SetFloat("Vertical", inputRelative.z);
    }

    public void PistolsHolsterEvent()
    {
        GunsHolstered?.Invoke();
    }

    public void PistolsDrawnEvent()
    {
        GunsDrawn?.Invoke();
    }

    public void LeftEquipUsedEvent()
    {
        LeftEquipUsed?.Invoke();
    }

    public void RightEquipUsedEvent()
    {
        RightEquipUsed?.Invoke();
    }

    public void PushButton()
    {
        
    }

    public void PickUpEvent()
    {
        PickUp?.Invoke();
        PickUp = null;
    }

    public void ClimbUp()
    {
        _anim.Play("ClimbUp");
    }

    public void ClimbUpCool()
    {
        _anim.Play("Handstand");
    }

    public void CrouchToIdle()
    {
        _anim.CrossFadeInFixedTime("CrouchToIdle", 0.1f);
    }

    public void Fall()
    {
        _anim.CrossFadeInFixedTime("FallBlend", 0.1f);
    }

    public Transform GetBone(HumanBodyBones bone)
    {
        return _anim.GetBoneTransform(bone);
    }

    public JumpType GetJumpType()
    {
        if (_animState.IsName("PoleSwingJumpToJump"))
        {
            return JumpType.Pole;
        }
        else if (_animState.IsName("RunJump") || _animState.IsName("RunJumpM")
            || _animState.IsName("RunJumpR") || _animState.IsName("RunJumpL")
            //|| _animState.IsName("JumpToFall")
            || _animState.IsName("Reach"))
        {
            return JumpType.Run;
        }
        else if (_animState.IsName("JumpF"))
        {
            return JumpType.Stand;
        }
        else if (IsInTrans("SideJump -> JumpF") || IsInTrans("SideJumpR -> JumpF"))
        {
            return JumpType.LedgeSide;
        }
        else if (_animState.IsName("JumpU"))
        {
            return JumpType.Up;
        }
        else if (_animState.IsName("Backflip"))
        {
            return JumpType.Backflip;
        }
        else if (_animState.IsName("JumpL") || _animState.IsName("JumpR"))
        {
            return JumpType.Sideflip;
        }
        else if (_animState.IsName("Dive"))
        {
            return JumpType.Dive;
        }
        else
        {
            return JumpType.None;
        }
    }

    public Vector3 GetRootMotionMove()
    {
        return _anim.deltaPosition;
    }

    public Quaternion GetRootMotionRotation()
    {
        return _anim.rootRotation;
    }

    public void IdleToCrouch()
    {
        _anim.CrossFadeInFixedTime("IdleToCrouch", 0.1f);
    }

    public bool IsIn(string state)
    {
        return _animState.IsName(state);
    }

    public bool IsIn(int layer, string state)
    {
        AnimatorStateInfo animState = _anim.GetCurrentAnimatorStateInfo(layer);
        return animState.IsName(state);
    }

    public bool IsInTrans(string name)
    {
        return _transInfo.IsName(name);
    }

    public bool IsInTrans()
    {
        return _anim.IsInTransition(0);
    }

    public void IdleToBlockPush()
    {
        _anim.Play("BlockPushIdle", 0, _animState.normalizedTime % 1.0f);
        //_anim.CrossFadeInFixedTime("BlockIdle", 0.0f, 0, NormTime);
    }

    public void Land(Vector3 velocity, Vector3 targetVelocity)
    {
        if (_animState.IsName("Dive"))
        {
            _anim.Play("DiveLand");
        }
        else if (velocity.y < -12.0f)
        {
            _anim.CrossFadeInFixedTime("HardLand", 0.1f);
        }
        else if (_animState.IsName("JumpU2") || _animState.IsName("JumpU"))
        {
            _anim.CrossFadeInFixedTime("JumpULand", 0.1f);
        }
        else
        {
            float targetAngle = Vector3.Angle(transform.forward, targetVelocity);
            if (targetVelocity.magnitude > 0.1f && targetAngle < 135.0f)
            {
                _anim.CrossFadeInFixedTime("FallToRun", 0.1f);
            }
            else
            {
                _anim.CrossFadeInFixedTime("FallToIdle", 0.1f);
                //_anim.CrossFadeInFixedTime("FallLandIdle", 0.1f);
            }
        }
    }

    public void LandMultiJump(Vector3 velocity, Vector3 targetVelocity)
    {
        JumpType type = GetJumpType();

        if (type == JumpType.Backflip)
        {
            _anim.Play("BackflipLand");
        }
        else if (_animState.IsName("JumpL"))
        {
            _anim.Play("JumpLLand");
        }
        else if (_animState.IsName("JumpR"))
        {
            _anim.Play("JumpRLand");
        }
        else
        {
            Land(velocity, targetVelocity);
        }
    }

    public void JumpF()
    {
        if (_normalizedTime > 0.5f)
        {
            _anim.CrossFadeInFixedTime("RunToJumpR", 0.1f);
        }
        else
        {
            _anim.CrossFadeInFixedTime("RunToJumpL", 0.1f);
        }
    }

    public void JumpU()
    {
        _anim.CrossFadeInFixedTime("CompressToJumpU", 0.1f);
    }

    public void JumpR()
    {
        _anim.CrossFadeInFixedTime("CompressToJumpR", 0.1f);
    }

    public void JumpL()
    {
        _anim.CrossFadeInFixedTime("CompressToJumpL", 0.1f);
    }

    public void JumpB()
    {
        _anim.CrossFadeInFixedTime("CompressToBackflip", 0.1f);
    }

    public void RunBackward()
    {
        _anim.SetFloat("SpeedMultiplier", -1.0f);
    }

    public void RunForward()
    {
        _anim.SetFloat("SpeedMultiplier", 1.0f);
    }

    public void RunJump()
    {
        _anim.CrossFadeInFixedTime("RunJump", 0.1f);
    }

    public void StepUpHalfIdle(Vector3 targetPosition)
    {
        _anim.CrossFadeInFixedTime("StepUpHalfIdle", 0.1f);
        //_anim.Play("StepUpHalfIdle");
        StartCoroutine(TargetMatch("StepUpHalfIdle", targetPosition, 0.1f, 0.9f));
    }

    public void StepUpMaxIdle(Vector3 targetPosition)
    {
        //_anim.Play("StepUpMax");
        _anim.CrossFadeInFixedTime("StepUpMax", 0.1f);
        StartCoroutine(TargetMatch("StepUpMax", targetPosition, 0.1f, 0.9f));
    }

    public void StepUpQtr(Vector3 targetPosition)
    {
        _anim.CrossFadeInFixedTime("StepUpQtr", 0.1f);
        StartCoroutine(TargetMatch("StepUpQtr", targetPosition, 0.1f, 0.9f));
    }

    public void StepUpQtrWalk(Vector3 targetPosition)
    {
        _anim.CrossFadeInFixedTime("StepUpQtrWalk", 0.1f, -1, 0.2f);
        StartCoroutine(TargetMatch("StepUpQtrWalk", targetPosition, 0.5f, 0.8f));
    }

    public void StepUpQtrIdle(Vector3 targetPosition)
    {
        _anim.CrossFadeInFixedTime("StepUpQtrIdle", 0.1f);
        //_anim.Play("StepUpQtrIdle");
        StartCoroutine(TargetMatch("StepUpQtrIdle", targetPosition, 0.1f, 0.9f));
    }

    public void TargetMatch(Vector3 position, Quaternion rotation, MatchTargetWeightMask mask, float start, float end, bool complete = false)
    {
        _anim.MatchTarget(position, rotation, AvatarTarget.Root, mask, start, end, complete);
    }

    public void TargetMatchState(string state, Vector3 position, Quaternion rotation, float startTime, float endTime)
    {
        StartCoroutine(TargetMatch(state, position, rotation, startTime, endTime));
    }

    public void TargetMatchState(string state, Vector3 position, Quaternion rotation, MatchTargetWeightMask mask, float startTime, float endTime)
    {
        StartCoroutine(TargetMatch(state, position, rotation, mask, startTime, endTime));
    }

    private IEnumerator TargetMatch(string state, Vector3 position, Quaternion rotation, MatchTargetWeightMask mask, float startTime, float endTime)
    {
        float initTime = Time.time;

        IsTargetMatching = true;

        while (!_animState.IsName(state))
        {
            // Safety if state doesnt get reached
            if (Time.time - initTime > 2.0f)
            {
                Debug.LogError("Tried to target match: " + state + " but failed!");
                break;
            }

            yield return null;
        }

        while (_animState.IsName(state))
        {
            _anim.MatchTarget(position, rotation, AvatarTarget.Root, mask, startTime, endTime, false);
            yield return null;
        }

        IsTargetMatching = false;
    }

    private IEnumerator TargetMatch(string state, Vector3 position, float startTime, float endTime)
    {
        float initTime = Time.time;

        IsTargetMatching = true;

        while (!_animState.IsName(state))
        {
            // Safety if state doesnt get reached
            if (Time.time - initTime > 2.0f)
            {
                Debug.LogError("Tried to target match: " + state + " but failed!");
                break;
            }

            yield return null;
        }

        while (_animState.IsName(state))
        {
            MatchTargetWeightMask mask = new MatchTargetWeightMask(transform.up, 0.0f);
            _anim.MatchTarget(position, Quaternion.identity, AvatarTarget.Root, mask, startTime, endTime, false);

            yield return null;
        }

        IsTargetMatching = false;
    }

    private IEnumerator TargetMatch(string state, Vector3 position, Quaternion rotation, float startTime, float endTime)
    {
        float initTime = Time.time;

        IsTargetMatching = true;

        while (!_animState.IsName(state))
        {
            // Safety if state doesnt get reached
            if (Time.time - initTime > 2.0f)
            {
                Debug.LogError("Tried to target match: " + state + " but failed!");
                break;
            }

            yield return null;
        }

        while (_animState.IsName(state))
        {
            MatchTargetWeightMask mask = new MatchTargetWeightMask(Vector3.one, 1.0f);
            _anim.MatchTarget(position, rotation, AvatarTarget.Root, mask, startTime, endTime, false);

            yield return null;
        }

        IsTargetMatching = false;
    }
}
