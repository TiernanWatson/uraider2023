using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    [SerializeField] private float _changeRate = 1.0f;
    [SerializeField] private Light _beam;

    private bool _active = false;
    private float _beamIntensity;
    private PlayerController _player;

    private void Start()
    {
        _player = GetComponent<PlayerController>();
        _player.UInput.Light.performed += (ctx) => _active = !_active;
        _beamIntensity = _beam.intensity;
        _beam.intensity = 0.0f;
    }

    private void Update()
    {
        float intensityTarget = _active ? _beamIntensity : 0.0f;

        _beam.intensity = Mathf.Lerp(_beam.intensity, intensityTarget, Time.deltaTime * _changeRate);
    }
}
