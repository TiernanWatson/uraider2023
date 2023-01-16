using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimedText : MonoBehaviour
{
    private bool _active;
    private Text _text;
    private Image _bg;
    private Color _bgColorOn;
    private Color _textColorOn;

    private void Awake()
    {
        _active = false;
        _bg = GetComponent<Image>();
        _bgColorOn = _bg.color;
        _text = GetComponentInChildren<Text>();
        _textColorOn = _text.color;

        Color startBg = _bg.color;
        startBg.a = 0.0f;
        _bg.color = startBg;

        Color startTxt = _text.color;
        startTxt.a = 0.0f;
        _text.color = startTxt;
    }

    private void Update()
    {
        if (_active)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }

    private void FadeOut()
    {
        _bg.GetComponent<ContentSizeFitter>().SetLayoutVertical();

        Color current = _bg.color;
        current.a = Mathf.Lerp(current.a, 0.0f, Time.deltaTime * 5.0f);
        _bg.color = current;

        current = _text.color;
        current.a = Mathf.Lerp(current.a, 0.0f, Time.deltaTime * 5.0f);
        _text.color = current;
    }

    private void FadeIn()
    {
        Color current = _bg.color;
        current.a = Mathf.Lerp(current.a, _bgColorOn.a, Time.deltaTime * 5.0f);
        _bg.color = current;

        current = _text.color;
        current.a = Mathf.Lerp(current.a, _textColorOn.a, Time.deltaTime * 5.0f);
        _text.color = current;
    }

    public void SetText(string text, float time)
    {
        _active = true;
        _text.text = text;
    }

    public void SetText(string text)
    {
        _active = true;
        _text.text = text;
    }

    public void Fade()
    {
        _active = false;
    }
}
