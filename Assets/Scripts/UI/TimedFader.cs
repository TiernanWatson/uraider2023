using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasFader))]
public class TimedFader : MonoBehaviour
{
    private bool _active = false;
    private float _startTime;
    private float _targetTime;
    private CanvasFader _fader;

    private void Awake()
    {
        _fader = GetComponent<CanvasFader>();    
    }

    private void Update()
    {
        if (_active)
        {
            if (Time.time - _startTime > _targetTime)
            {
                _fader.FadeOut();
                _active = false;
            }
        }
    }

    public void ShowFor(float time)
    {
        _active = true;
        _targetTime = time;
        _startTime = Time.time;
        _fader.FadeIn();
    }
}
