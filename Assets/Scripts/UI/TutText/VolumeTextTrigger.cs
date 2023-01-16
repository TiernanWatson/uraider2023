using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeTextTrigger : MonoBehaviour
{
    [SerializeField] private bool _oneTime = true;
    [TextArea(15, 20)]
    [SerializeField] private string _text;
    [SerializeField] private TimedText _textDisplay;

    private bool _used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_oneTime && _used)
            {
                return;
            }

            _used = true;
            string parsed = _text.Replace("\\n", "\n");
            _textDisplay.SetText(parsed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _textDisplay.Fade();
        }
    }
}
