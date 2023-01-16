using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleRenderer : MonoBehaviour
{
    private enum HookMode
    {
        Hooking,
        Hooked,
        Unhooking,
        Unhooked
    }

    public bool IsHooked => _mode == HookMode.Hooked;
    public bool IsUnhooked => _mode == HookMode.Unhooked;

    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _leftHand;
    [SerializeField] private float _hookSpeed = 10.0f;
    [SerializeField] private float _unhookSpeed = 10.0f;
    [SerializeField] private int _waveResolution = 50;

    private LineRenderer _lineRenderer;

    private HookMode _mode;
    private Vector3 _targetPosition;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _mode = HookMode.Unhooked;
    }

    private void Start()
    {
        _lineRenderer.enabled = false;
        _lineRenderer.startWidth = 0.015f;
        _lineRenderer.endWidth = 0.015f;
        _lineRenderer.positionCount = 4;
    }

    private void LateUpdate()
    {
        // Point at waist
        _lineRenderer.SetPosition(0, transform.position);

        Vector3 grappleEndPosition = _lineRenderer.GetPosition(3);

        if (_mode == HookMode.Hooking || _mode == HookMode.Unhooking)
        {
            if (_mode == HookMode.Unhooking)
            {
                _targetPosition = transform.position;
            }

            float speed = _mode == HookMode.Hooking ? _hookSpeed : _unhookSpeed;
            Vector3 currentPosition = _mode == HookMode.Unhooking ? _lineRenderer.GetPosition(_waveResolution - 1) : _lineRenderer.GetPosition(3);
            grappleEndPosition = Vector3.MoveTowards(currentPosition, _targetPosition, Time.deltaTime * speed);

            if (Vector3.Distance(grappleEndPosition, _targetPosition) < 0.25f)
            {
                grappleEndPosition = _targetPosition;
                _mode = _mode == HookMode.Hooking ? HookMode.Hooked : HookMode.Unhooked;

                if (_mode == HookMode.Unhooked)
                {
                    _lineRenderer.enabled = false;
                }
            }

            // The hook of the grapple itself
            _lineRenderer.SetPosition(_mode == HookMode.Unhooking ? _waveResolution - 1 : 3, grappleEndPosition);
        }

        if (_mode == HookMode.Unhooking)
        {
            for (int i = 1; i < _waveResolution - 1; ++i)
            {
                // Use a sine wave to get a wavy effect when retracting
                float sinPointX = 2.0f * Mathf.PI * i / _waveResolution;
                float waveValue = Mathf.Sin(sinPointX + 30.0f * Time.time);
                Vector3 basePoint = Vector3.Lerp(transform.position, grappleEndPosition, (float)i / _waveResolution);
                basePoint += transform.right * waveValue * 0.1f;
                _lineRenderer.SetPosition(i, basePoint);
            }
        }
        else
        {
            _lineRenderer.SetPosition(1, _leftHand.position);
            _lineRenderer.SetPosition(2, _rightHand.position);
        }
    }

    /// <summary>
    /// Have the grapple travel from the start to the specified position
    /// </summary>
    /// <param name="position">Target position</param>
    public void HookTo(Vector3 position)
    {
        _mode = HookMode.Hooking;
        _targetPosition = position;
        _lineRenderer.enabled = true;
        _lineRenderer.positionCount = 4;

        // Need to set this so that the hook doesn't start at origin
        _lineRenderer.SetPosition(3, _rightHand.position);
    }

    /// <summary>
    /// Have the grapple travel back to its holster
    /// </summary>
    public void Unhook()
    {
        _lineRenderer.positionCount = _waveResolution;
        _lineRenderer.SetPosition(_waveResolution - 1, _lineRenderer.GetPosition(3));
        _mode = HookMode.Unhooking;
    }
}
