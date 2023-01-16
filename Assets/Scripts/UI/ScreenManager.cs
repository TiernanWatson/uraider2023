using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenManager : MonoBehaviour
{
    public bool IsPaused { get; private set; }
    public bool IsInventory { get; private set; }
    public bool IsEnd { get; private set; }

    [SerializeField] private PauseMenu _pauseMenu;
    [SerializeField] private RingMenu _ringMenu;
    [SerializeField] private EndScreen _endScreen;

    private void Start()
    {
        HidePause();
        HideInventory();
        HideEnd();
    }

    public void OnInventory(InputValue value)
    {
        return;
        if (!IsInventory && !IsPaused && !IsEnd)
        {
            ShowInventory();
        }
        else if (IsInventory)
        {
            HideInventory();
        }
    }

    private void HideInventory()
    {
        _ringMenu.HideRing();
        Time.timeScale = 1.0f;
        IsInventory = false;
    }

    private void ShowInventory()
    {
        _ringMenu.DisplayRing();
        Time.timeScale = 0.0f;
        IsInventory = true;
    }

    public void ShowPause()
    {
        Cursor.visible = true;
        _pauseMenu.DisplayMenu();
        IsPaused = true;
    }

    public void HidePause()
    {
        Cursor.visible = false;
        _pauseMenu.HideMenu();
        IsPaused = false;
    }

    public void ShowEnd()
    {
        Time.timeScale = 0.0f;
        _endScreen.Show();
        Cursor.visible = true;
        IsEnd = true;
    }

    public void HideEnd()
    {
        _endScreen.Hide();
        Cursor.visible = false;
        IsEnd = false;
    }
}
