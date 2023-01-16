using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class NumericalButton : MonoBehaviour
{
    public UnityEvent OnChange;

    [SerializeField] private Text _textBox;
    [SerializeField] private float _defaultValue = 1.0f;
    [SerializeField] private float _increment = 0.25f;
    [SerializeField] private float _minValue = 0.25f;
    [SerializeField] private float _maxValue = 4.00f;
    [SerializeField] private string _prefName;

    private void Start()
    {
        float value = PlayerPrefs.GetFloat(_prefName, _defaultValue);
        _textBox.text = "[" + value.ToString("F2") + "]";
        enabled = false; // Enable with select event
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        // Event fires twice otherwise
        if (!ctx.performed)
        {
            return;
        }

        if (enabled)
        {
            Vector2 amount = ctx.ReadValue<Vector2>();
            if (Mathf.Abs(amount.x) < 0.01f)
            {
                // Stop W/S causing a change as sign will still be non-zero
                return;
            }

            float current = PlayerPrefs.GetFloat(_prefName);
            float newAmount = Verify(current + _increment * Mathf.Sign(amount.x));
            PlayerPrefs.SetFloat(_prefName, newAmount);

            _textBox.text = "[" + newAmount.ToString("F2") + "]";

            OnChange?.Invoke();
        }
    }

    private float Verify(float amount)
    {
        return Mathf.Clamp(amount, _minValue, _maxValue);
    }
}
