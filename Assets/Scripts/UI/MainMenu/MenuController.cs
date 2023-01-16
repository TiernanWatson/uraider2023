using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject _levelPane;
    [SerializeField] private GameObject _optionsPane;
    [SerializeField] private GameObject _mainPane;
    [SerializeField] private GameObject _firstOptionSelect;
    [SerializeField] private GameObject _firstMainSelect;
    [SerializeField] private GameObject _firstLevelSelect;
    [SerializeField] private Button _newGameBtn;
    [SerializeField] private CanvasFader _fader;
    [SerializeField] private EventSystem _eventSys;

    private AsyncOperation _loadOp;
    private GameObject _activePane;

    private void Start()
    {
        LevelInfo.Save = null;
        _activePane = _mainPane;
        Time.timeScale = 1.0f;
    }

    private void Update()
    {
        InputSystem.Update();
    }

    public void OnEscape(InputValue value)
    {
        if (_activePane == _levelPane)
        {
            LevelBackClick();
        }
    }

    public void NewGameClick()
    {
        FadePanels(_mainPane.GetComponent<CanvasFader>(), _levelPane.GetComponent<CanvasFader>());
        _eventSys.SetSelectedGameObject(_firstLevelSelect);
        _activePane = _levelPane;
    }

    public void LevelBackClick()
    {
        FadePanels(_levelPane.GetComponent<CanvasFader>(), _mainPane.GetComponent<CanvasFader>());
        _eventSys.SetSelectedGameObject(_firstMainSelect);
        _activePane = _mainPane;
    }

    public void OptionsClick()
    {
        FadePanels(_mainPane.GetComponent<CanvasFader>(), _optionsPane.GetComponent<CanvasFader>());
        _eventSys.SetSelectedGameObject(_firstOptionSelect);
        _activePane = _optionsPane;
    }

    public void OptionsBackClick()
    {
        FadePanels(_optionsPane.GetComponent<CanvasFader>(), _mainPane.GetComponent<CanvasFader>());
        _eventSys.SetSelectedGameObject(_firstMainSelect);
        _activePane = _mainPane;
    }

    public void ExitClick()
    {
        Application.Quit();
    }

    public void LoadLevel(LevelInfo info)
    {
        LevelInfo.Active = info;
        LevelInfo.UseEnter = true;

        _loadOp = SceneManager.LoadSceneAsync(1);
        _loadOp.allowSceneActivation = false;

        _fader.FadeIn();
        _fader.FadedIn += () => _loadOp.allowSceneActivation = true;

        // Stop double-clicking
        _newGameBtn.interactable = false;
    }

    public void FadePanels(CanvasFader away, CanvasFader display)
    {
        away.FadeOut();
        away.Interactable = false;
        away.BlockRaycasts = false;
        away.FadedOut += display.FadeIn;
        away.FadedOut += () => { away.FadedOut -= display.FadeIn; };
        //display.FadeIn();
        display.Interactable = true;
        display.BlockRaycasts = true;
    }

    public void SetSelectedBtn(Button btn)
    {
        _eventSys.SetSelectedGameObject(btn.gameObject);
    }
}
