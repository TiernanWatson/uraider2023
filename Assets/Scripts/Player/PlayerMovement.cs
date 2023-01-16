using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EdgeConstraints
{
    Feet,
    Forward,
    None,

    All = Feet | Forward
}

/// <summary>
/// Handles different player movement states
/// </summary>
[RequireComponent(typeof(PlayerMotorCC))]
public partial class PlayerMovement : MonoBehaviour
{
    

    private const float StickWalkMax = 0.95f;

    public float Gravity { get => _gravity; }
    public bool IsAutoMoving { get; private set; }
    public bool PauseRotation
    {
        get { return _pauseRotation; }
        set 
        {
            if (value == false)
                TargetRotation = Rotation = transform.rotation;

            _pauseRotation = value;
        }
    }
    public bool RootMotionMove => AnimControl.ApplyRootMotion;
    public bool RootMotionRotate
    {
        get { return _rootMotionRotate; }
        set
        {
            _rootMotionRotate = value;
        }
    }
    public bool OverrideMode { get; set; } = false;
    public bool UseDisplacement { get; set; } = false;
    public float StickSpeed { get; private set; }
    public float StickRawSpeed { get; private set; }

    public EdgeConstraints EdgeBehaviour { get; set; } = EdgeConstraints.None;

    public Vector3 TargetVelocity { get; protected set; }
    public Quaternion TargetRotation { get; protected set; }
    public Vector3 Velocity { get; set; }
    public Vector3 MoveAmount { get; set; } = Vector3.zero;
    public Quaternion Rotation { get; set; }
    public Quaternion ModelRotation { get; private set; }

    public AirMode Air { get; private set; }
    public ClimbMode Climb { get; private set; }
    public PushMode Push { get; private set; }
    public RoamMode Roam { get; private set; }
    public SlideMode Slide { get; private set; }
    public StrafeMode Strafe { get; private set; }
    public SwimMode Swim { get; private set; }
    public SwingMode Swing { get; private set; }
    public WallMode Wall { get; private set; }

    public UInput UInput
    {
        get { return _input; }
        set
        {
            _input = value;
            _input.Walk.performed += (ctx) => _isWalk = true;
            _input.Walk.canceled += (ctx) => _isWalk = false;
        }
    }
    public PlayerMotorCC Motor { get; private set; }
    public PlayerAnim AnimControl { get; private set; } // TODO: Remove from this layer

    #pragma warning disable 0649

    [SerializeField] private float _gravity;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _maxTurnAngle;
    [SerializeField] private float _slideRotateRate;
    [SerializeField] private float _swimMaxTurnAngle;
    [SerializeField] private float _roamSpeedInterp;
    [SerializeField] private Transform _cam;
    
    #pragma warning restore 0649

    protected MovementMode _currentMode;

    private bool _canMove;
    private bool _rootMotionRotate;
    private bool _pauseRotation;
    private bool _isWalk;
    private Vector3 _moveInput;
    private Vector3 _autoMoveLocation;
    private Quaternion _autoMoveRotation;
    private UInput _input;
    private ModelRotater _modelRotater;

    private void Awake()
    {
        _canMove = true;
        _rootMotionRotate = false;
        _pauseRotation = false;
        _isWalk = false;

        Rotation = TargetRotation = transform.rotation;

        Roam = new RoamMode(this);
        Strafe = new StrafeMode(this);
        Air = new AirMode(this);
        Climb = new ClimbMode(this);
        Slide = new SlideMode(this);
        Swing = new SwingMode(this);
        Swim = new SwimMode(this);
        Push = new PushMode(this);
        Wall = new WallMode(this);

        AnimControl = GetComponent<PlayerAnim>();
        Motor = GetComponent<PlayerMotorCC>();
        _modelRotater = GetComponent<ModelRotater>();
    }

    private void OnAnimatorMove()
    {
        if (IsAutoMoving)
        {
            IsAutoMoving = _currentMode.AutoMove(_autoMoveLocation, _autoMoveRotation);
        }
        else
        {
            if (!PauseRotation)
            {
                if (RootMotionRotate)
                {
                    Rotation = AnimControl.GetRootMotionRotation();
                }

                transform.rotation = Rotation;
            }

            // This is useful moving in absolute deltas as opposed to a velocity
            if (UseDisplacement)
            {
                Motor.Move(MoveAmount);
            }
            else
            {
                Vector3 moveAmount = RootMotionMove ? AnimControl.GetRootMotionMove() : Velocity * Time.deltaTime;
                Motor.Move(moveAmount);
            }
            
            _modelRotater.TargetRotation = ModelRotation;
        }
    }

    public void PreAnimatorUpdate()
    {
        return;
        if (EdgeBehaviour.HasFlag(EdgeConstraints.Forward))
        {
            Vector3 rayOrigin = transform.position + transform.forward * Roam.EdgeStopDistance;

            UMath.DrawCapsule(rayOrigin, rayOrigin, 0.25f);

            Collider[] colliders = Physics.OverlapSphere(rayOrigin, 0.25f, Motor.GroundLayers, QueryTriggerInteraction.Collide);
            foreach (var collider in colliders)
            {
                LedgePoint ledge = collider.GetComponent<LedgePoint>();
                if (ledge && Vector3.Dot(ledge.transform.forward, transform.forward) < 0.0f)
                {
                    Vector3 ledgePointAhead = ledge.GetPoint(ledge.ClosestParamTo(transform.position, transform.forward));
                    Debug.DrawRay(ledgePointAhead, Vector3.up, Color.red);
                    float distance = UMath.HorizontalMag(ledgePointAhead - transform.position);
                    if (distance + Motor.CharControl.minMoveDistance < Roam.EdgeStopDistance)
                    {
                        Velocity = Vector3.zero;
                        SetPosition(ledgePointAhead - transform.forward * Roam.EdgeStopDistance);
                    }
                }
            }
        }
    }

    public void AutoMoveTo(Vector3 position, Quaternion rotation)
    {
        IsAutoMoving = true;
        _autoMoveLocation = position;
        _autoMoveRotation = rotation;
    }

    /// <summary>
    /// Called once per frame to calculate Velocity and Rotation, but doesn't perform movement
    /// </summary>
    public void Resolve()
    {
        StickRawSpeed = UInput.MoveInputRaw.magnitude;
        if (StickRawSpeed > 0.01f)
        {
            StickRawSpeed = StickRawSpeed < StickWalkMax ? _walkSpeed : _runSpeed;
            if (_isWalk)
            {
                StickRawSpeed = Mathf.Min(StickRawSpeed, _walkSpeed);
            }
        }
        StickSpeed = Mathf.Lerp(StickSpeed, StickRawSpeed, Time.deltaTime * _roamSpeedInterp);

        // Used for special cases if an upper layer wants to directly set velocity
        if (!OverrideMode)
        {
            _currentMode.Resolve();
        }

        Velocity = ProjectOnBigSlopes(Velocity);
    }

    public void SetVelocity(Vector3 velocity)
    {
        Velocity = TargetVelocity = velocity;
    }

    public void SetRotation(Quaternion rotation)
    {
        Rotation = TargetRotation = transform.rotation = rotation;
    }

    public void GoToRoam()
    {
        ChangeMode(Roam);
    }

    public void GoToSlide()
    {
        ChangeMode(Slide);
    }

    public void GoToStrafe()
    {
        ChangeMode(Strafe);
    }

    public void GoToSwim()
    {
        ChangeMode(Swim);
    }

    public void GoToAir()
    {
        ChangeMode(Air);
    }

    public void GoToClimb(IShimmyable ledge)
    {
        Climb.Ledge = ledge;
        ChangeMode(Climb);
    }

    public void GoToSwing(GrappleZone grapple)
    {
        Swing.TetherPoint = grapple;
        ChangeMode(Swing);
    }

    public void GoToPush()
    {
        ChangeMode(Push);
    }

    public bool IsIn(MovementMode mode)
    {
        return _currentMode == mode;
    }

    public bool IsRoam()
    {
        return _currentMode == Roam;
    }

    public bool IsStrafe()
    {
        return _currentMode == Strafe;
    }

    public bool IsClimb()
    {
        return _currentMode == Climb;
    }

    public bool IsAir()
    {
        return _currentMode == Air;
    }

    public bool IsSwing()
    {
        return _currentMode == Swing;
    }

    public void SetPosition(Vector3 position)
    {
        Motor.CharControl.enabled = false;
        transform.position = position;
        Motor.CharControl.enabled = true;
    }

    public Quaternion GetCameraRotaterY()
    {
        return Quaternion.Euler(0.0f, _cam.eulerAngles.y, 0.0f);
    }

    protected Quaternion GetCameraRotater()
    {
        return _cam.rotation;
    }

    protected Vector3 GetCameraForward()
    {
        Vector3 camForward = _cam.forward;
        camForward.y = 0.0f;
        return camForward.normalized;
    }

    private void ChangeMode(MovementMode mode)
    {
        if (_currentMode != null)
        {
            _currentMode.End();
        }

        _currentMode = mode;
        _currentMode.Begin();
    }

    /// <summary>
    /// If the ground is too steep, project the velocity down it to smoothly slide off
    /// </summary>
    /// <param name="movement">Original velocity</param>
    /// <returns>Projected velocity</returns>
    private Vector3 ProjectOnBigSlopes(Vector3 movement)
    {
        float groundAngle = Vector3.Angle(Vector3.up, Motor.Ground.normal);

        if (groundAngle > Motor.CharControl.slopeLimit || Mathf.Approximately(groundAngle, Motor.CharControl.slopeLimit))
        {
            if (!Motor.IsGrounded && Motor.CharControl.isGrounded)
                movement = Vector3.ProjectOnPlane(movement, Motor.Ground.normal);
        }

        return movement;
    }
}
