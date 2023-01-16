using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrappleReticle : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private Color _inRangeColor;
    [SerializeField] private Color _outOfRangeColor;

    private bool _inGrappleZone;
    private CanvasFader _fader;
    private GrappleZone _grapple;
    private Image _sprite;

    private void Start()
    {
        _inGrappleZone = false;

        _fader = GetComponent<CanvasFader>();
        _sprite = GetComponent<Image>();

        // Want to allow the fader time to fade out before we stop updating position
        _fader.FadedOut += StopUpdating;
        _player.Triggers.GrappleFound += OnGrappleFound;
        _player.StateMachine.StateChanged += OnStateChanged;

        // No target by default
        enabled = false;
    }

    private void LateUpdate()
    {
        if (_grapple)
        {
            if (_player.StateMachine.State.IsGrappleInRange(_grapple))
            {
                _sprite.color = _inRangeColor;
            }
            else
            {
                _sprite.color = _outOfRangeColor;
            }

            Vector3 position = Camera.main.WorldToScreenPoint(_grapple.transform.position);
            transform.position = ClampPosition(position);
        }
    }

    private void OnGrappleFound(GrappleZone grapple)
    {
        if (grapple == _grapple)
        {
            return;
        }

        _grapple = grapple;

        if (grapple)
        {
            _inGrappleZone = true;
            enabled = true;

            if (_player.StateMachine.State != _player.BaseStates.Swing)
                _fader.FadeIn();
        }
        else
        {
            _inGrappleZone = false;
            _fader.FadeOut();
        }
    }

    private void OnStateChanged(PlayerState oldState, PlayerState newState)
    {
        if (!_inGrappleZone)
        {
            return;
        }

        // Don't show the reticle while swinging
        if (newState == _player.BaseStates.Swing)
        {
            Debug.Log("Swing");
            _fader.FadeOut();
        }
        else
        {
            Debug.Log("Start grapple 2");
            _fader.FadeIn();
        }
    }

    private void StopUpdating()
    {
        if (!_inGrappleZone)
        {
            enabled = false;
            _grapple = null;
        }
    }

    private bool IsOutsideScreen(Vector3 point)
    {
        return point.x > Camera.main.pixelWidth || point.x < 0.0f || point.y > Camera.main.pixelHeight || point.y < 0.0f;
    }

    private Vector3 ClampPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, 0.0f, Camera.main.pixelWidth);
        pos.y = Mathf.Clamp(pos.y, 0.0f, Camera.main.pixelHeight);
        return pos;
    }
}
