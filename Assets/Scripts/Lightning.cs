using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    [SerializeField] private float _playRate = 10.0f;
    [SerializeField] private AnimationCurve _anim;

    private Light _light;

    private void Start()
    {
        _anim.postWrapMode = WrapMode.Clamp;
        _light = GetComponent<Light>();
    }

    private void LateUpdate()
    {
        _light.intensity = _anim.Evaluate(Time.time % _playRate);
    }

}
