using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class CameraController : MonoBehaviour
{
    public CameraCollision Collision => _collision;

    public Transform TargetLock { get { return _targetLock; } set { _targetLock = value; } }
    public Transform Follow => target;

    [SerializeField] private float mouseIdleTime = 1.5f;
    [SerializeField] private float mouseRotateRate = 120.0f;
    [SerializeField] private float pitchRestoreRate = 0.5f;
    [SerializeField] private float autoRotateRate = 15.0f;
    [SerializeField] private float pitchMax = 85.0f;
    [SerializeField] private float pitchMin = -85.0f;
    [SerializeField] private float pitchDefault = 15.0f;
    [SerializeField] private float moveInterp = 0.12f;
    [SerializeField] private float rotInterp = 45.0f;
    [SerializeField] private float combatRaise = 0.5f;

    [SerializeField] private Transform target;
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform _camera;

    [SerializeField] private PlayerController _player;
    private CameraCollision _collision;
    [SerializeField] private PlayerMotorCC _motor;
    private Transform _targetLock;

    private bool _clampYaw = false;
    private bool _trackRotation = true;
    private float _yawMax = 80.0f;
    private float _yawMin = -80.0f;

    private float _yaw = 0.0f;
    private float _pitch = 0.0f;
    private float _lastMouseMove = 0.0f;

    private float _mouseX = 0.0f;
    private float _mouseY = 0.0f;
    private float _mouseSenstivity = 0.0f;

    private float _defaultMoveInterp = 0.15f;
    private float _targetMoveInterp = 0.15f;
    private float _currentMoveInterp = 0.15f;

    private Transform _overrideInfo;

    private Vector3 _camVelocity = Vector3.zero;
    private Vector3 _pivotLocalPosition;

    private void Awake()
    {
        _lastMouseMove = -mouseIdleTime;
        _yaw = pivot.rotation.eulerAngles.y;
        _pitch = pivot.rotation.eulerAngles.x;

        _currentMoveInterp = _targetMoveInterp = _defaultMoveInterp = moveInterp;

        _collision = GetComponent<CameraCollision>();
    }

    private void Start()
    {
        _mouseSenstivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);

        _player.EquipedMachine.StateChanged += EquipStateChange;
        _player.StateMachine.StateChanged += PlayerStateChange;
        //_player.TargetFound += TargetFound;

        _pivotLocalPosition = pivot.localPosition;

        SelectPivotHeight(_player.StateMachine.State);
    }

    private void OnApplicationQuit()
    {
        _player.EquipedMachine.StateChanged -= EquipStateChange;
        _player.StateMachine.StateChanged -= PlayerStateChange;
        //_player.TargetFound -= TargetFound;
    }

    private void LateUpdate()
    {
        if (_overrideInfo == null)
        {
            FreeMove();
            //SelectPivotHeight(_player.StateMachine.State);
        }
        else
        {
            Vector3 targetPosition = _overrideInfo.position;
            _camera.position = targetPosition;
            _camera.rotation = _overrideInfo.rotation;
        }

        //_mouseX = 0.0f;
        //_mouseY = 0.0f;
    }

    private void FreeMove()
    {
        Vector3 targetPosition = target.position;

        if (_targetLock)
        {
            _lastMouseMove = Time.time;
            MoveWithMouse(_mouseX, _mouseY);

            targetPosition += Vector3.up * combatRaise;

            Vector3 toTarget = (_targetLock.position - target.position).normalized;
            _yaw = Quaternion.LookRotation(toTarget, Vector3.up).eulerAngles.y;
        }
        else if (!Mathf.Approximately(_mouseX, 0.0f) || !Mathf.Approximately(_mouseY, 0.0f))
        {
            _lastMouseMove = Time.time;
            MoveWithMouse(_mouseX, _mouseY);
        }
        else if (_trackRotation && Time.time - _lastMouseMove > 1.0f)
        {
            TrackPlayerRotation();
        }

        if (_clampYaw)
        {
            float min = target.transform.eulerAngles.y + _yawMin;
            float max = target.transform.eulerAngles.y + _yawMax;

            _yaw = UMath.ClampAngle(_yaw, min, max);
        }

        _currentMoveInterp = Mathf.Lerp(_currentMoveInterp, _targetMoveInterp, Time.deltaTime);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _camVelocity, _currentMoveInterp);

        Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
        pivot.rotation = Quaternion.Slerp(pivot.rotation, targetRotation, Time.deltaTime * rotInterp);

        _collision.SetPosition(_pivotLocalPosition);
    }

    // Called by Unity's input system
    public void OnLook(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        _mouseX = input.x;
        _mouseY = input.y;
    }

    private void SelectPivotHeight(IState newState)
    {
        _pivotLocalPosition = Vector3.zero;
        return;

        if (newState == _player.BaseStates.Crouch)
        {
            _pivotLocalPosition = Vector3.up * 0.75f * 0.7f;
        }
        else if (newState == _player.BaseStates.PoleSwing)
        {
            _pivotLocalPosition = Vector3.up * 1.6f;
        }
        else if (newState == _player.BaseStates.Swim)
        {
            _pivotLocalPosition = Vector3.up * 0.0f;
        }
        else
        {
            _pivotLocalPosition = Vector3.up * 1.25f;
        }
    }

    public void RefreshSensitivity()
    {
        _mouseSenstivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
    }

    public void OverridePosition(Transform info)
    {
        _overrideInfo = info;
    }

    private void EquipStateChange(IState oldState, IState newState)
    {
        _trackRotation = !(newState == _player.EquipedStates.Combat);
    }

    private void PlayerStateChange(IState oldState, IState newState)
    {
        //ClampYaw(newState == _player.BaseStates.Climb);
        //SelectPivotHeight(newState);
        if ( newState == _player.BaseStates.Swing )
        {
            _targetMoveInterp = 0.05f;
        }
        else
        {
            _targetMoveInterp = 0.15f;
        }
    }

    public void TargetFound(Transform target)
    {
        _targetLock = target;
    }

    private void TrackPlayerRotation()
    {
        // Used to increase turn speed
        Vector3 camForward = new Vector3(pivot.forward.x, 0.0f, pivot.forward.z);
        float angle = Vector3.Angle(target.forward, camForward);

        // Controls rotation and stops when running into a wall
        Vector3 moveOnX = Vector3.Project(_motor.CharControl.velocity, pivot.transform.right) * Time.deltaTime;
        Vector3 localMove = Quaternion.Euler(0.0f, -pivot.transform.eulerAngles.y, 0.0f) * moveOnX;

        // Delta time already included when player was moved
        if (Mathf.Abs(_player.MoveInput.x) > 0.125f)
            _yaw += localMove.x * angle * autoRotateRate;

        // Move camera up and down
        float targetPitch = Vector3.SignedAngle(_motor.transform.forward, _player.Velocity, _motor.transform.right);
        if (!float.IsNaN(targetPitch))
        {
            float moveAmount = _motor.CharControl.velocity.magnitude;
            //_pitch = Mathf.LerpAngle(_pitch, targetPitch + pitchDefault, Time.deltaTime * moveAmount * pitchRestoreRate);
            //_pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
        }
    }

    private void MoveWithMouse(float mouseX, float mouseY)
    {
        _yaw += mouseX * mouseRotateRate * _mouseSenstivity * Time.deltaTime;
        _pitch -= mouseY * mouseRotateRate * _mouseSenstivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
    }

    public void ClampYaw(bool state)
    {
        _clampYaw = state;
    }
}
