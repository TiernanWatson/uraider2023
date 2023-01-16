using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public UnityEvent ResumeClicked;

    [SerializeField] private CanvasFader _fader;
    [SerializeField] private PanelFadeTo _pauseFadeTo;
    [SerializeField] private PanelFadeTo _optionsFadeTo;
    
    private bool _isOptions = false;

    private void Start()
    {
        _fader.FadedOut += () => CanInteract(false);
    }

    public bool CanExit()
    {
        return !_isOptions;
    }

    public bool HandleEscape()
    {
        if (LevelManager.Instance.IsPaused)
        {
            if (_isOptions)
            {
                OnOptionsBack();
                return true;
            }
        }

        return false;
    }

    public void OnResume()
    {
        ResumeClicked?.Invoke();
    }

    public void OnOptions()
    {
        _pauseFadeTo.FadeTo();
        _isOptions = true;
    }

    public void OnOptionsBack()
    {
        _optionsFadeTo.FadeTo();
        _isOptions = false;
    }

    public void OnExit()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void OnEscape(InputValue value)
    {
        return;
        if (!LevelManager.Instance.IsPaused)
        {
            return;
        }

        Debug.Log("Escape");
        if (_isOptions)
        {
            Debug.Log("Options back");
            OnOptionsBack();
        }
        /*else
        {
            Debug.Log("Resume bitch");
            OnResume();
        }*/
    }

    public void DisplayMenu()
    {
        CanInteract(true);
        _fader.FadeIn();
    }

    public void HideMenu()
    {
        _fader.FadeOut();
    }

    private void CanInteract(bool status)
    {
        _fader.Interactable = status;
        _fader.BlockRaycasts = status;
    }
}
