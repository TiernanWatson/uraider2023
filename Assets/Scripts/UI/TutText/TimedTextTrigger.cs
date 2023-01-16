using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedTextTrigger : MonoBehaviour
{
    [SerializeField] private bool _oneTime = true;
    [SerializeField] private float _time = 5.0f;
    [SerializeField] private string _text;
    [SerializeField] private TimedText _textDisplay;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string parsed = _text.Replace("\\n", "\n");
            _textDisplay.SetText(parsed, _time);

            if (_oneTime)
            {
                enabled = false;
            }
        }
    }
}
