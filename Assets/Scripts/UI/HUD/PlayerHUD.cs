using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Image _healthImg;
    [SerializeField] private PlayerStats _stats;

    private void OnEnable()
    {
        _stats.OnHealthChanged += HealthUpdate;
    }

    private void OnDisable()
    {
        _stats.OnHealthChanged -= HealthUpdate;
    }

    private void HealthUpdate(float health)
    {
        Vector3 scale = _healthImg.rectTransform.localScale;
        scale.x = Mathf.Clamp(_stats.Health / 100.0f, 0.0f, 1.0f);
        _healthImg.rectTransform.localScale = scale;
    }
}
