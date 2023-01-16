using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlRemap : MonoBehaviour
{
    [SerializeField] private PlayerInput _input;
    [SerializeField] private string _actionName;
    [SerializeField] private Text _nameTxt;
    [SerializeField] private Text _buttonTxt;

    private bool _wasEnabled = false;
    private InputAction _action;
    private InputActionRebindingExtensions.RebindingOperation _operation;

    private void Start()
    {
        _action = _input.actions.FindAction(_actionName);
        if (_action == null)
        {
            Debug.LogError("Could not find action: " + _actionName);
        }
        _nameTxt.text = _actionName;
        _buttonTxt.text = "[" + GetButtonTxt() + "]";
    }

    private void LateUpdate()
    {
        if (_operation != null)
        {
            if (_operation.completed)
            {
                _buttonTxt.text = "[" + GetButtonTxt() + "]";
                if (_wasEnabled)
                {
                    _action.Disable();
                }
               // var overrides = _input.actions.SaveBindingOverridesAsJson();
               // PlayerPrefs.SetString("InputOverrides", overrides);
                _operation = null;
            }
        }
    }

    public void Remap()
    {
        return; // TODO: Remove and implement
        _wasEnabled = _action.enabled;
        if (_wasEnabled)
        {
            _action.Disable();
        }

        _operation = _action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .Start();

        _buttonTxt.text = "Select...";
    }

    private string GetButtonTxt()
    {
        return _action.controls[0].displayName;
    }
}
