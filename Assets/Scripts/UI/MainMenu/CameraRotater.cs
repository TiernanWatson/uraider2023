using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraRotater : MonoBehaviour
{
    [SerializeField] private float _inputScale = 0.005f;

    private float _mouseX;
    private float _mouseY;
    private float _startX;
    private float _startY;
    private float _startZ;

    private void Awake()
    {
        Quaternion startRotation = transform.rotation;
        _startX = startRotation.eulerAngles.x;
        _startY = startRotation.eulerAngles.y;
        _startZ = startRotation.eulerAngles.z;
    }

    private void LateUpdate()
    {
        InputSystem.Update();

        Quaternion newRotation = Quaternion.Euler(_startX - _mouseY, _startY + _mouseX, _startZ);
        transform.rotation = newRotation;
    }

    public void OnAim(InputAction.CallbackContext value)
    {
        Vector2 v = value.ReadValue<Vector2>();

        // Want center of screen to be default
        v -= new Vector2(Screen.width, Screen.height) / 2.0f;

        _mouseX = v.x * _inputScale;
        _mouseY = v.y * _inputScale;
    }
}
