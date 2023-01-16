using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public partial class Human : MonoBehaviour, IDamageable
{
    public enum AIType
    {
        Point,
        Patrol
    }

    public class AIStates
    {
        public IdleState Idle { get; private set; }
        public PatrolState Patrol { get; private set; }
        public AlertState Alert { get; private set; }
        public KillState Kill { get; private set; }
        public DeadState Dead { get; private set; }

        public AIStates(Human human)
        {
            Idle = new IdleState(human);
            Patrol = new PatrolState(human);
            Alert = new AlertState(human);
            Kill = new KillState(human);
            Dead = new DeadState(human);
        }
    }

    public bool IsDead => StateMachine.State == States.Dead;
    public Transform ChestAimPoint => _chestAimPoint;
    public Animator Anim { get; private set; }
    public NavMeshAgent NavAgent { get; private set; }
    public RagdollControl Ragdoll { get; private set; }
    public StateMachine<Human, EnemyState> StateMachine { get; private set; }
    public AIStates States { get; private set; }

    [SerializeField] private AIType _type = AIType.Point;
    [SerializeField] private PatrolPoint[] _patrolPoints;
    [SerializeField] private bool _triggerOnStart = true;
    [SerializeField] private float _startHealth = 100.0f;
    [SerializeField] private float _fieldOfViewXZ = 160.0f;
    [SerializeField] private float _fieldOfViewY = 120.0f;
    [SerializeField] private float _visionDistance = 15.0f;
    [SerializeField] private float _hearingDistance = 5.0f;
    [SerializeField] private float _engagedStop = 4.0f;
    [SerializeField] private float _patrolStop = 0.1f;
    [SerializeField] private float _engagedSpeed = 3.2f;
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private LayerMask _sightLayers;
    [SerializeField] private Transform _eyePos;
    [SerializeField] private Transform _chestAimPoint;
    [SerializeField] private Weapon _weapon;

    private int _curPatrolPoint = -1;
    private float _health;
    private CapsuleCollider _collider;
    
    private void Awake()
    {
        _health = _startHealth;
        _collider = GetComponent<CapsuleCollider>();

        Anim = GetComponent<Animator>();
        NavAgent = GetComponent<NavMeshAgent>();
        Ragdoll = GetComponent<RagdollControl>();

        States = new AIStates(this);
        StateMachine = new StateMachine<Human, EnemyState>(_type == AIType.Point ? States.Idle : States.Patrol);
    }

    private void Start()
    {
        StateMachine.Begin();

        if (!_triggerOnStart)
        {
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        StateMachine.State.FixedUpdate();
    }

    private void Update()
    {
        StateMachine.Update();

        float speed = NavAgent.velocity.magnitude;
        Anim.SetFloat("Speed", speed);

        Vector3 velRelative = transform.InverseTransformVector(NavAgent.velocity);
        Anim.SetFloat("Forward", velRelative.z);
        Anim.SetFloat("Right", velRelative.x);
    }

    public void Activate()
    {
        enabled = true;
    }

    public void FireWeapon()
    {
        _weapon.TargetPosition = States.Kill.Target.Waist.WaistBone.position;
        _weapon.Fire();
    }

    public void Kill()
    {
        _health = 0;
        StateMachine.ChangeState(States.Dead);
    }

    public void Damage(float strength)
    {
        StateMachine.State.Damage(strength);
    }

    private Collider[] VisionOverlap()
    {
        return Physics.OverlapBox(transform.position, Vector3.one * _visionDistance);
    }

    private PatrolPoint GetNextPatrol()
    {
        if (_curPatrolPoint == _patrolPoints.Length - 1)
        {
            _curPatrolPoint = 0;
        }
        else
        {
            _curPatrolPoint++;
        }

        return _patrolPoints[_curPatrolPoint];
    }
}
