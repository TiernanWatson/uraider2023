using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private CanvasFader _fader;
    [SerializeField] private CanvasFader _screenFader;
    [SerializeField] private LevelInfo _mainMenuInfo;

    private void Start()
    {
        _fader.FadedOut += () => CanInteract(false);
    }

    public void Show()
    {
        CanInteract(true);
        _fader.FadeIn();
    }

    public void Hide()
    {
        _fader.FadeOut();
    }

    public void GoToMainMenu()
    {
        LoadingScript.Load(_mainMenuInfo, _screenFader, false, () => Time.timeScale = 1.0f);
    }

    private void CanInteract(bool status)
    {
        _fader.Interactable = status;
        _fader.BlockRaycasts = status;
    }
}
