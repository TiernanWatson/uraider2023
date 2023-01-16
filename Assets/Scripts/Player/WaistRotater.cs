using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaistRotater : MonoBehaviour
{
    public Transform WaistBone => _waistBone;

    [SerializeField] private float _interpRate = 12.0f;
    [SerializeField] private Transform _waistBone;

    private bool _rotate = false;
    private float _strength = 0.0f;
    private Quaternion _targetRot;
    private Quaternion _result;

    private void LateUpdate()
    {
        /*if (_rotate)
        {
            _waistBone.rotation = _targetRot;
            _rotate = false;
        }*/
        if (_rotate)
        {
            _strength += Time.deltaTime * _interpRate;
            
        }
        else
        {
            _strength -= Time.deltaTime * _interpRate;
        }

        _strength = Mathf.Clamp01(_strength);
        _waistBone.rotation = Quaternion.Slerp(_waistBone.rotation, _targetRot, _strength);
    }

    public void Rotate(Quaternion rotation)
    {
        //_strength = 0.0f;
        _rotate = true;
        _targetRot = rotation;
    }

    public void Stop()
    {
        _rotate = false;
    }
}
