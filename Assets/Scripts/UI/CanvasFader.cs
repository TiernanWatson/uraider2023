using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
    private enum FadeType
    {
        Nothing,
        FadeIn,
        FadeOut
    }

    public event Action FadedIn;
    public event Action FadedOut;

    public bool IsFading => _fadeType != FadeType.Nothing;
    public bool IsFadingOut => _fadeType == FadeType.FadeOut;
    public bool IsFadingIn => _fadeType == FadeType.FadeIn;
    public bool Interactable
    {
        get { return _group.interactable; }
        set { _group.interactable = value; }
    }
    public bool BlockRaycasts
    {
        get { return _group.blocksRaycasts; }
        set { _group.blocksRaycasts = value; }
    }

    [SerializeField] private FadeType _awakeBehavior = FadeType.Nothing;
    [SerializeField] private float _fadeRate = 20.0f;

    private FadeType _fadeType;
    private CanvasGroup _group;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _fadeType = _awakeBehavior;
    }

    private void Update()
    {
        if (_fadeType == FadeType.FadeOut)
        {
            float maxDelta = Mathf.Max(Mathf.Epsilon, Time.unscaledDeltaTime * _fadeRate);
            _group.alpha = Mathf.MoveTowards(_group.alpha, 0.0f, maxDelta);

            if (_group.alpha < 0.01f)
            {
                _group.alpha = 0.0f;
                _fadeType = FadeType.Nothing;
                FadedOut?.Invoke();
            }
        }
        else if (_fadeType == FadeType.FadeIn)
        {
            float maxDelta = Mathf.Max(Mathf.Epsilon, Time.unscaledDeltaTime * _fadeRate);
            _group.alpha = Mathf.MoveTowards(_group.alpha, 1.0f, maxDelta);

            if (_group.alpha > 0.99f)
            {
                _group.alpha = 1.0f;
                _fadeType = FadeType.Nothing;
                FadedIn?.Invoke();
            }
        }
    }

    public void FadeIn()
    {
        _fadeType = FadeType.FadeIn;
    }

    public void FadeOut()
    {
        _fadeType = FadeType.FadeOut;
    }
}
