using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour
{
    [SerializeField] private AnimationCurve _curve;
    
    private SkinnedMeshRenderer _faceRenderer;

    private void Start()
    {
        _curve.postWrapMode = WrapMode.Loop;
        _faceRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    private void LateUpdate()
    {
        float weight = _curve.Evaluate(Time.time);
        _faceRenderer.SetBlendShapeWeight(1, weight * 100f);
    }
}
