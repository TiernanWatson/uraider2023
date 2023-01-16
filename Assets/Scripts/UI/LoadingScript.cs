using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LoadingScript : MonoBehaviour
{
    [SerializeField] private Text _titleTxt;
    [SerializeField] private Text _descTxt;
    [SerializeField] private Image _bgMaterial;
    [SerializeField] private Image _loadingBar;
    [SerializeField] private Text _continueTxt;
    [SerializeField] private CanvasFader _fader;

    private int _sceneIndex = 0;
    private bool _showingText = false;
    private AsyncOperation _operation;

    private void Start()
    {
        if (LevelInfo.Active)
        {
            _sceneIndex = LevelInfo.Active.levelIndex;
            _titleTxt.text = LevelInfo.Active.levelName;
            _descTxt.text = LevelInfo.Active.levelInfo;

            if (LevelInfo.Active.loadingImage)
            {
                _bgMaterial.sprite = LevelInfo.Active.loadingImage;
            }
        }

        _operation = SceneManager.LoadSceneAsync(_sceneIndex);
        _operation.allowSceneActivation = !LevelInfo.UseEnter;

        _continueTxt.gameObject.SetActive(false);
    }

    private void Update()
    {
        InputSystem.Update();

        Vector3 scale = _loadingBar.rectTransform.localScale;
        scale.x = Mathf.Clamp01(_operation.progress / 0.9f);
        _loadingBar.rectTransform.localScale = scale;

        if (LevelInfo.UseEnter && _operation.progress >= 0.9f && !_showingText)
        {
            _continueTxt.gameObject.SetActive(true);
            _showingText = true;
        }
    }

    public void OnEnter(InputValue val)
    {
        if (!LevelInfo.UseEnter)
        {
            return;
        }

        if (!_fader.IsFading && _operation.progress >= 0.9f)
        {
            _fader.FadeIn();
            _fader.FadedIn += () => _operation.allowSceneActivation = true;
        }
    }

    /// <summary>
    /// Convenience function to load a level with fading
    /// </summary>
    /// <param name="info">Level details</param>
    /// <param name="fader">Black out fader</param>
    public static void Load(LevelInfo info, CanvasFader fader, bool useEnter = true, Action onLoad = null)
    {
        LevelInfo.Active = info;
        LevelInfo.UseEnter = useEnter;
        
        var _op = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        _op.allowSceneActivation = false;

        fader.FadeIn();
        fader.FadedIn += () => _op.allowSceneActivation = true;
        if (onLoad != null)
        {
            fader.FadedIn += onLoad;
        }
    }

    /// <summary>
    /// Convenience function to load a level with no fading
    /// </summary>
    /// <param name="info"></param>
    public static void Load(LevelInfo info)
    {
        LevelInfo.Active = info;
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}
