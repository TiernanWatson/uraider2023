using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helps smooth out any animations were there is rapid root movement
/// </summary>
public class PlayerCamTracker : MonoBehaviour
{
    public bool IsOverriding { get; set; } = false;
    public float ForcedHeight { get; set; } = 0.0f;

    [SerializeField] private PlayerController _player;
    [SerializeField] private Transform _hip;
    [Header("Heights")]
    [SerializeField] private float _locomotionHeight = 1.25f;
    [SerializeField] private float _swimHeight = 0.0f;
    [SerializeField] private float _crouchHeight = 0.5f;

    private bool _useMemorizedHeight = false;
    private float _memorizedHeight;
    private Transform _follow;

    private void Awake()
    {
        _follow = _player.transform;
    }

    private void Start()
    {
        _player.StateMachine.StateChanged += OnStateChange;
    }

    private void Update()
    {
        if (IsOverriding)
        {
            transform.position = _follow.position + Vector3.up * ForcedHeight;
            transform.rotation = _follow.rotation;
            return;
        }

        if (_player.StateMachine.State == _player.BaseStates.Dead)
        {
            transform.position = _hip.position;
        }
        else if (_player.StateMachine.State == _player.BaseStates.Swim)
        {
            // The root bone moves up to the hip during anim transition and messes with things
            Vector3 targetPosition = _follow.position + Vector3.up * _swimHeight;
            if (_useMemorizedHeight)
            {
                targetPosition.y = _memorizedHeight;
                if (_player.AnimControl.IsTag("Swimming"))
                {
                    // Now out of transition animation WadeToSurf
                    _useMemorizedHeight = false;
                }
            }

            transform.position = targetPosition;
        }
        else if (_player.StateMachine.State == _player.BaseStates.Locomotion)
        {
            // The root bone moves up to the hip during anim transition and messes with things
            Vector3 targetPosition = _follow.position + Vector3.up * _locomotionHeight;
            if (_useMemorizedHeight)
            {
                targetPosition.y = _memorizedHeight;
                if (_player.AnimControl.IsTag("Moving"))
                {
                    // Now out of transition animation WadeToSurf
                    _useMemorizedHeight = false;
                }
            }

            transform.position = targetPosition;
        }
        else
        {
            transform.position = _follow.position + Vector3.up * _locomotionHeight;
        }

        transform.rotation = _follow.rotation;
    }

    private void OnStateChange(PlayerState oldState, PlayerState newState)
    {
        if ((oldState == _player.BaseStates.Locomotion && newState == _player.BaseStates.Swim)
            /*|| (oldState == _player.BaseStates.Swim && newState == _player.BaseStates.Locomotion)*/)
        {
            _useMemorizedHeight = true;
            _memorizedHeight = transform.position.y;
        }
    }
}
