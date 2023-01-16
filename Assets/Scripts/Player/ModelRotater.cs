using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates the character model on the x-axis, so CC is upright
/// </summary>
public class ModelRotater : MonoBehaviour
{
    public bool UseLocal { get; set; } = false;
    public bool UseInterpolation { get; set; } = true;

    public Quaternion TargetRotation
    {
        get { return _targetRotation; }
        set
        {
            if (IsValidRotation(value))
            {
                _targetRotation = value;
            }
            else
            {
                Debug.LogError("Tried to set NaN quat on model rotater");
            }
        }
    }

    [SerializeField] private PlayerController _player;
    [SerializeField] private float _interp;
    [SerializeField] private Transform _model;

    private Quaternion _localRot = Quaternion.identity;
    private Quaternion _targetRotation = Quaternion.identity;

    private void LateUpdate()
    {
        if (UseInterpolation)
        {
            _localRot = Quaternion.Slerp(_localRot, TargetRotation, Time.deltaTime * _interp);
            if (UseLocal)
                _model.localRotation = _localRot;
            else
                _model.rotation = _localRot;
        }
        else
        {
            if (UseLocal)
                _model.localRotation = TargetRotation;
            else
                _model.rotation = TargetRotation;
        }
    }

    public void ForceInterpolationCompletion()
    {
        _localRot = TargetRotation;
    }

    private bool IsValidRotation(Quaternion quat)
    {
        return !(float.IsNaN(quat.x) || float.IsNaN(quat.y) || float.IsNaN(quat.z) || float.IsNaN(quat.w));
    }
}
