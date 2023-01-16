using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelFadeTo : MonoBehaviour
{
    [SerializeField] private CanvasFader _fadeTo;
    [SerializeField] private GameObject _firstSelected;
    [SerializeField] private EventSystem _eventSystem;

    private CanvasFader _thisFader;

    private void Start()
    {
        _thisFader = GetComponent<CanvasFader>();
    }

    public void FadeTo()
    {
        _thisFader.FadeOut();
        _thisFader.Interactable = false;
        _thisFader.BlockRaycasts = false;
        _thisFader.FadedOut += _fadeTo.FadeIn;
        _thisFader.FadedOut += () => { _thisFader.FadedOut -= _fadeTo.FadeIn; };

        _fadeTo.Interactable = true;
        _fadeTo.BlockRaycasts = true;

        _eventSystem.SetSelectedGameObject(_firstSelected);
    }
}
