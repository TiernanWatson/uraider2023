using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Player states form the parts of this partial class

/// <summary>
/// Top-level controller of the player that controls state and communicates 
/// with the motor and movement component.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnim))]
[RequireComponent(typeof(PlayerSFX))]
public partial class PlayerController : MonoBehaviour, IStateful<PlayerController, PlayerState>
{
    public static PlayerController Local { get; private set; }

    public class States
    {
        public LocomotionState Locomotion { get; private set; }
        public BlockPushState BlockPush { get; private set; }
        public CrouchState Crouch { get; private set; }
        public DrainpipeState Drainpipe { get; private set; }
        public FreeclimbState Freeclimb { get; private set; }
        public AirState Air { get; private set; }
        public AutoGrabLedgeState AutoGrabLedge { get; private set; }
        public AutoGrabPoleState AutoGrabPole { get; private set; }
        public DeadState Dead { get; private set; }
        public LadderState Ladder { get; private set; }
        public JumpState Jump { get; private set; }
        public MonkeyState Monkey { get; private set; }
        public MultiJumpState MultiJump { get; private set; }
        public SwingState Swing { get; private set; }
        public ClimbState Climb { get; private set; }
        public PoleClimbState PoleClimb { get; private set; }
        public PoleSwingState PoleSwing { get; private set; }
        public StrafeState Strafe { get; private set; }
        public SwimState Swim { get; private set; }
        public SlideState Slide { get; private set; }
        public WallclimbState Wallclimb { get; private set; }

        public States(PlayerController player)
        {
            Locomotion = new LocomotionState(player);
            BlockPush = new BlockPushState(player);
            Crouch = new CrouchState(player);
            Drainpipe = new DrainpipeState(player);
            Freeclimb = new FreeclimbState(player);
            Air = new AirState(player);
            AutoGrabLedge = new AutoGrabLedgeState(player);
            AutoGrabPole = new AutoGrabPoleState(player);
            Dead = new DeadState(player);
            Ladder = new LadderState(player);
            Jump = new JumpState(player);
            Monkey = new MonkeyState(player);
            MultiJump = new MultiJumpState(player);
            Swing = new SwingState(player);
            Climb = new ClimbState(player);
            PoleClimb = new PoleClimbState(player);
            PoleSwing = new PoleSwingState(player);
            Strafe = new StrafeState(player);
            Swim = new SwimState(player);
            Slide = new SlideState(player);
            Wallclimb = new WallclimbState(player);
        }
    }

    public class EquipmentStates
    {
        public NotEquipedState NotEquiped { get; private set; }
        public CombatState Combat { get; private set; }

        public EquipmentStates(PlayerController player)
        {
            NotEquiped = new NotEquipedState(player);
            Combat = new CombatState(player);
            Combat.ExcludeState(player.BaseStates.Swim);
            Combat.ExcludeState(player.BaseStates.Dead);
        }
    }

    /// <summary>
    /// Event fired when the player locks onto a target (transform of target)
    /// </summary>
    public event Action<Transform> TargetFound;

    public bool IsGrounded => Movement.Motor.IsGrounded; 
    public ControllerData Settings => settings;
    public PlayerMotorCC.GroundInfo Ground => Movement.Motor.Ground;
    public CameraController CameraControl => _cameraControl;
    public Transform Camera => cam.transform; 
    public Transform GrapplePoint  => grappleHolder; 
    public Transform RightHand => _handRight;
    public Transform EyePosition => _eyePos;

    public Vector3 MoveInput { get => UInput.MoveInput; }
    public Vector3 Velocity { get => Movement.Velocity; }
    public Vector3 Forward { get => transform.forward; }

    public AutoGrabDetector GrabDetector { get; private set; }
    public CharacterController CharControl { get; private set; }
    public HashSet<PlayerState> CombatBaseStates { get; private set; }
    public PlayerAnim AnimControl { get; private set; }
    public PlayerEquipment Equipment { get; private set; }
    public PlayerInteractions Interactions { get; private set; }
    public PlayerInventory Inventory { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerSFX SFX { get; private set; }
    public PlayerStats Stats { get; private set; }
    public PlayerTriggers Triggers { get; private set; }
    public PlayerVaults Vaults { get; private set; }
    public UInput UInput { get; private set; }
    public WeaponManager Weapons { get; private set; }
    public ModelRotater Model { get; private set; }
    public WaistRotater Waist { get; private set; }
    public RagdollControl Ragdoll { get; private set; }
    public PlayerIKSolver IKSolver { get; private set; }

    public StateMachine<PlayerController, PlayerState> StateMachine { get; private set; }
    public StateMachine<PlayerController, PlayerState> EquipedMachine { get; private set; }
    public States BaseStates { get; private set; }
    public EquipmentStates EquipedStates { get; private set; }

    /// <summary>
    /// Local rotation of the 3D model (not the main player object)
    /// </summary>
    public Quaternion ModelRotation { get; set; }

    #pragma warning disable 0649

    [Header("Settings")]
    [SerializeField] private ControllerData settings;
    [SerializeField] private AnimationData _animData;
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform waistBone;
    [SerializeField] private Transform grappleHolder;
    [SerializeField] private Transform model;
    [SerializeField] private Transform _handLeft;
    [SerializeField] private Transform _handRight;
    [SerializeField] private Transform _eyePos;
    [SerializeField] private CameraController _cameraControl;
    [SerializeField] private GameObject _aimReticle;
    [SerializeField] private GrappleRenderer _grapple;

    #pragma warning restore 0649

    private bool _overrideWaist;
    private float _jumpUpSpeed;
    private float _jumpForwardSpeed;
    private float _standingJumpSpeed;
    private Quaternion _waistRotation;

    private void Awake()
    {
        Local = this;

        ModelRotation = Quaternion.identity;

        GrabDetector = new AutoGrabDetector(AutoGrabDetectFlags.All, settings.ledgeLayers.value, 0.25f);

        Inventory = GetComponent<PlayerInventory>();
        SFX = GetComponent<PlayerSFX>();
        Stats = GetComponent<PlayerStats>();
        AnimControl = GetComponent<PlayerAnim>();
        Interactions = GetComponent<PlayerInteractions>();
        Equipment = GetComponent<PlayerEquipment>();
        CharControl = GetComponent<CharacterController>();
        Weapons = GetComponent<WeaponManager>();
        Movement = GetComponent<PlayerMovement>();
        Model = GetComponent<ModelRotater>();
        Waist = GetComponent<WaistRotater>();
        Ragdoll = GetComponent<RagdollControl>();
        Triggers = GetComponent<PlayerTriggers>();
        Vaults = GetComponent<PlayerVaults>();
        IKSolver = GetComponent<PlayerIKSolver>();

        BaseStates = new States(this);
        EquipedStates = new EquipmentStates(this);
        //StateMachine = new StateMachine<PlayerController, PlayerState>(BaseStates.Swing);
        StateMachine = new StateMachine<PlayerController, PlayerState>(BaseStates.Locomotion);
        EquipedMachine = new StateMachine<PlayerController, PlayerState>(EquipedStates.NotEquiped);

        CombatBaseStates = new HashSet<PlayerState>
        {
            BaseStates.Locomotion,
            BaseStates.Jump,
            BaseStates.Air,
            //BaseStates.Crouch,
            BaseStates.Slide,
            BaseStates.MultiJump
        };

        // Jump Speeds
        _jumpUpSpeed = Mathf.Sqrt(2.0f * Movement.Gravity * settings.jumpHeight);
        float timeInAir = (2.0f * _jumpUpSpeed) / Movement.Gravity;
        _jumpForwardSpeed = settings.jumpDistance / timeInAir;
        _standingJumpSpeed = settings.standJumpDistance / timeInAir;
    }

    private void OnEnable()
    {
        Stats.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        Stats.OnDeath -= OnDeath;
    }

    private void Start()
    {
        UInput = new UInput(GetComponent<PlayerInput>());

        UInput.Interact.performed += (ctx) => Interactions.Use();

        //GrappleZone grapple = GameObject.Find("GrappleZone").GetComponent<GrappleZone>();
        //BaseStates.Swing.Grapple = grapple;

        StateMachine.Begin();
        EquipedMachine.Begin();

        Movement.UInput = UInput;

        AnimControl.GunsDrawn += Weapons.DrawWeapon;
        AnimControl.GunsHolstered += Weapons.HolsterWeapon;
    }

    private void Update()
    {
        AnimControl.UpdateState();

        UInput.UpdateMove();

        StateMachine.Update();
        EquipedMachine.Update();

        Movement.PreAnimatorUpdate();
        StateMachine.State.UpdateAnimation(AnimControl);
    }

    private void LateUpdate()
    {
        StateMachine.State.LateUpdate();
    }

    private void OnDeath()
    {
        StateMachine.State.OnDeath();
    }

    public void SetIgnoreCollision(bool value)
    {
        Physics.IgnoreLayerCollision(8, 0, value);
        Physics.IgnoreLayerCollision(8, 9, value);
        Physics.IgnoreLayerCollision(8, 11, value);
    }

    public bool CheckCapsule(Vector3 position)
    {
        var charControl = Movement.Motor.CharControl;

        Vector3 halfToSphere = transform.up * (charControl.height * 0.5f - charControl.radius);
        Vector3 capsuleStart = position + charControl.center - halfToSphere + transform.up * charControl.skinWidth;
        Vector3 capsuleEnd = position + charControl.center + halfToSphere + transform.up * charControl.skinWidth;

        return Physics.CheckCapsule(
            capsuleStart,
            capsuleEnd,
            charControl.radius,
            settings.groundLayers.value,
            QueryTriggerInteraction.Ignore);
    }

    public bool CheckCapsule(Vector3 position, float height)
    {
        var charControl = Movement.Motor.CharControl;

        float skinWidth = charControl.skinWidth;
        Vector3 center = Vector3.up * (height * 0.5f + skinWidth);

        Vector3 halfToSphere = transform.up * (height * 0.5f - charControl.radius);
        Vector3 capsuleStart = position + center - halfToSphere;
        Vector3 capsuleEnd = position + center + halfToSphere;

        return Physics.CheckCapsule(
            capsuleStart,
            capsuleEnd,
            charControl.radius,
            settings.groundLayers.value,
            QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// Force a rotation on the waist for this frame
    /// </summary>
    /// <param name="rotation">Rotation to use</param>
    public void OverrideWaist(Quaternion rotation)
    {
        _overrideWaist = true;

        rotation = Quaternion.Euler(
            rotation.eulerAngles.x - 90.0f,
            rotation.eulerAngles.y, 
            rotation.eulerAngles.z);

        Waist.Rotate(rotation);

        _waistRotation = rotation;
    }

    public void JumpBack()
    {
        float jumpSpeed = _jumpForwardSpeed;
        Movement.SetVelocity(-transform.forward * jumpSpeed + Vector3.up * _jumpUpSpeed);
    }

    public void JumpLeft()
    {
        float jumpSpeed = _jumpForwardSpeed;
        Movement.SetVelocity(-transform.right * jumpSpeed + Vector3.up * _jumpUpSpeed);

    }
    
    public void JumpRight()
    {
        float jumpSpeed = _jumpForwardSpeed;
        Movement.SetVelocity(transform.right * jumpSpeed + Vector3.up * _jumpUpSpeed);

    }

    /// <summary>
    /// Jump forward depending on player's velocity
    /// </summary>
    public void JumpForward()
    {
        float jumpSpeed = CalculateJumpSpeed();
        Movement.SetVelocity(transform.forward * jumpSpeed + Vector3.up * _jumpUpSpeed);
    }

    /// <summary>
    /// Jump in the direction of the player input
    /// </summary>
    public void JumpToInput()
    {
        float jumpSpeed = CalculateJumpSpeed();

        Vector3 input = GetCameraRotater() * MoveInput;

        Movement.SetVelocity(input * jumpSpeed + Vector3.up * _jumpUpSpeed);
    }

    /// <summary>
    /// What forward speed should the player have given current animation/velocity
    /// </summary>
    /// <returns></returns>
    public float CalculateJumpSpeed()
    {
        var type = AnimControl.GetJumpType();

        if (type == PlayerAnim.JumpType.Up)
        {
            return 0.0f;
        }
        else if (type == PlayerAnim.JumpType.Stand || type == PlayerAnim.JumpType.LedgeSide)
        {
            return _standingJumpSpeed;
        }
        else if (Ground.tag.Equals("Slope"))
        {
            return _jumpForwardSpeed;
        }

        float percent = UMath.HorizontalMag(Movement.Velocity) / settings.runSpeed;

        return Mathf.Clamp(percent * _jumpForwardSpeed, 0.1f, _jumpForwardSpeed);
    }

    /// <summary>
    /// Gets the forward vector of the camera with no y-component.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCameraForward()
    {
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0.0f;

        return camForward.normalized;
    }

    /// <summary>
    /// Convenience function to get the flat camera rotation
    /// </summary>
    /// <returns>Rotation of camera on yaw</returns>
    public Quaternion GetCameraRotater()
    {
        return Quaternion.Euler(0.0f, cam.transform.eulerAngles.y, 0.0f);
    }

    public Vector3 GetColliderBottom()
    {
        return transform.position + CharControl.center + Vector3.down * CharControl.height * 0.5f;
    }

    /// <summary>
    /// Force set the player's position with no interpolation.
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Vector3 position)
    {
        CharControl.enabled = false;
        transform.position = position;
        CharControl.enabled = true;
    }

    public void SetRotation(Quaternion rotation)
    {
        Movement.SetRotation(rotation);
    }

    public bool SweepCapsule(Vector3 from, Vector3 amount, out RaycastHit hit)
    {
        var charControl = Movement.Motor.CharControl;

        Vector3 halfToSphere = transform.up * (charControl.height - charControl.radius);
        Vector3 capsuleStart = from + charControl.center - halfToSphere;
        Vector3 capsuleEnd = from + charControl.center + halfToSphere;

        return Physics.CapsuleCast(
            capsuleStart,
            capsuleEnd,
            charControl.radius,
            amount.normalized,
            out hit,
            amount.magnitude,
            settings.groundLayers.value,
            QueryTriggerInteraction.Ignore);
    }

    public void UseDoor(Door door)
    {
        StateMachine.State.TriggerDoor(door);
    }

    public void TriggerWallclimb(WallclimbSurface surface)
    {
        StateMachine.State.TriggerWallclimb(surface);
    }

    /// <summary>
    /// Force the player's rotation to be camera-relative input with no interpolation.
    /// </summary>
    public void RotateToInput()
    {
        if (MoveInput.sqrMagnitude < 0.01f)
        {
            return;
        }

        Vector3 input = MoveInput;
        Vector3 targetRot = Quaternion.Euler(0.0f, cam.transform.eulerAngles.y, 0.0f) * input;

        Movement.SetRotation(Quaternion.LookRotation(targetRot));
    }

    public void RotateTo(Vector3 direction)
    {
        Movement.SetRotation(Quaternion.LookRotation(direction));
    }

    public void RotateToCamera()
    {
        Movement.SetRotation(Quaternion.LookRotation(GetCameraForward()));
    }

    public void RungIncrement(int amount)
    {
        StateMachine.State.RungIncrement(amount);
    }

    private bool TryFindClosestGrabbable(Vector3 startPosition, Vector3 testDirection, float maxDistance, float maxHeight, out AutoGrabDetectFlags type, out int closestIndex)
    {
        if (GrabDetector.DetectFrom(startPosition, testDirection, maxDistance, maxHeight))
        {
            closestIndex = 0;
            type = AutoGrabDetectFlags.None;

            float closestDistance = Mathf.Infinity;

            for (int i = 0; i < GrabDetector.Ledges.Count; i++)
            {
                float distance = GetLedgeDistance(GrabDetector.Ledges[i].value);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                    type = AutoGrabDetectFlags.Ledge;
                }
            }

            for (int i = 0; i < GrabDetector.Wallclimbs.Count; i++)
            {
                float distance = GetGenericDistance(GrabDetector.Wallclimbs[i].value.transform);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                    type = AutoGrabDetectFlags.Wallclimb;
                }
            }

            for (int i = 0; i < GrabDetector.Poles.Count; i++)
            {
                float distance = GetPoleDistance(GrabDetector.Poles[i].value);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                    type = AutoGrabDetectFlags.Pole;
                }
            }

            for (int i = 0; i < GrabDetector.Monkeys.Count; i++)
            {
                float distance = GrabDetector.Monkeys[i].value.ClosetPointTo(transform.position).y - transform.position.y;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                    type = AutoGrabDetectFlags.Monkey;
                }
            }

            return true;
        }

        type = AutoGrabDetectFlags.None;
        closestIndex = 0;
        return false;
    }

    private float GetLedgeDistance(LedgePoint ledge)
    {
        float t = ledge.Collider.Point.ClosestParamTo(transform.position, transform.forward);
        Vector3 ledgeIntersectPoint = ledge.Collider.Point.GetPoint(t);
        return Vector3.Distance(transform.position, ledgeIntersectPoint);
    }

    private float GetPoleDistance(PolePoint pole)
    {
        float t = pole.Collider.Point.ClosestParamTo(transform.position, transform.forward);
        Vector3 poleIntersectionPoint = pole.Collider.Point.GetPoint(t);
        return Vector3.Distance(transform.position, poleIntersectionPoint);
    }

    private float GetGenericDistance(Transform obj)
    {
        return Vector3.Distance(transform.position, obj.transform.position);
    }

    private Vector3 GetLedgePosition(Vector3 point, Vector3 forward)
    {
        return point
            - forward * settings.ledgeBackOffset
            + Vector3.down * settings.ledgeDownOffset;
    }

    private Vector3 GetDrainpipePosition(Vector3 point, Vector3 forward)
    {
        return point
            - forward * settings.drainpipeBackOffset
            + Vector3.down * settings.drainpipeDownOffset;
    }

    private void LerpTo(Vector3 position)
    {
        Vector3 interp = Vector3.Lerp(transform.position, position, Time.deltaTime * settings.lerpToRate);
        SetPosition(interp);
    }

    private float PredictYNextFrame()
    {
        return transform.position.y + Velocity.y * Time.deltaTime;
    }
}
