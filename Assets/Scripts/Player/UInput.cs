using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UInput 
{
    [SerializeField] private float keyboardInterp = 5.0f;
    [SerializeField] private float gamepadInterp = 20.0f;

    private bool _isKeyboard = true;

    // Player Actions
    public InputAction ClimbUp { get; private set; }
    public InputAction Move { get; private set; }
    public InputAction Jump { get; private set; }
    public InputAction Interact { get; private set; }
    public InputAction Fire { get; private set; }
    public InputAction Crouch { get; private set; }
    public InputAction Walk { get; private set; }
    public InputAction Holster { get; private set; }
    public InputAction Light { get; private set; }
    // Camera Actions
    public InputAction Look { get; private set; }
    // UI Actions
    public InputAction Escape { get; private set; }

    public Vector3 MoveInput { get; private set; }
    public Vector3 MoveInputRaw { get; private set; }
    public Vector3 UniversalRotation { get; private set; }
    public Vector3 LastNonZeroInput { get; private set; }

    public UInput(PlayerInput input)
    {
        ClimbUp = input.actions.FindAction("ClimbUp", true);
        Move = input.actions.FindAction("Move", true);
        Jump = input.actions.FindAction("Jump", true);
        Interact = input.actions.FindAction("Interact", true);
        Fire = input.actions.FindAction("Fire", true);
        Holster = input.actions.FindAction("Holster", true);
        Crouch = input.actions.FindAction("Crouch", true);
        Walk = input.actions.FindAction("Walk", true);
        Light = input.actions.FindAction("Light", true);
        Look = input.actions.FindAction("Look", true);
        Escape = input.actions.FindAction("Escape", true);
    }

    public bool IsKeyboardMove()
    {
        if (Move.activeControl != null)
        {
            string name = Move.activeControl.displayName;
            if (name.Equals("w") || name.Equals("s") || name.Equals("a") || name.Equals("d"))
            {
                return true;
            }
        }

        return false;
    }

    public void UpdateMove()
    {
        InputSystem.Update();

        if (!LevelManager.Instance || (!LevelManager.Instance.IsPaused && !LevelManager.Instance.IsInventory))
        {
            TestIfIsKeyboard();

            Vector2 move = Move.ReadValue<Vector2>();
            MoveInputRaw = new Vector3(move.x, 0.0f, move.y);

            if (MoveInputRaw.sqrMagnitude > 0.01f)
            {
                LastNonZeroInput = MoveInputRaw;
            }
            else
            {
                // Only want to do this on way down hence the else
                if (MoveInput.sqrMagnitude < 0.01f)
                {
                    MoveInput = Vector3.zero;
                }
            }

            float interp = _isKeyboard ? keyboardInterp : gamepadInterp;
            MoveInput = Vector3.Slerp(MoveInput, MoveInputRaw, Time.deltaTime * interp);
            UniversalRotation = Vector3.Slerp(UniversalRotation, LastNonZeroInput, Time.deltaTime * interp);
        }
    }

    public void SetSmoothedInput(Vector3 move)
    {
        MoveInput = move;
    }

    private void TestIfIsKeyboard()
    {
        if (Move.activeControl != null)
        {
            string name = Move.activeControl.displayName;
            _isKeyboard = name.Equals("w") || name.Equals("s") || name.Equals("a") || name.Equals("d");
        }
    }
}
