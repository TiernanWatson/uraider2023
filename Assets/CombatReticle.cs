using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasFader))]
public class CombatReticle : MonoBehaviour
{
    [SerializeField] private PlayerController _player;

    private CanvasFader _fader;
    private Transform _target;

    private void Start()
    {
        _fader = GetComponent<CanvasFader>();

        // Want to allow the fader time to fade out before we stop updating position
        _fader.FadedOut += StopUpdating;
        _player.TargetFound += OnTargetFound;

        // No target by default
        enabled = false;
    }

    private void LateUpdate()
    {
        if (_target)
        {
            transform.position = Camera.main.WorldToScreenPoint(_target.transform.position);
            transform.position = ClampPosition(transform.position);
        }
    }

    private void OnTargetFound(Transform target)
    {
        _target = target;

        if (target)
        {
            enabled = true;
            _fader.FadeIn();
        }
        else
        {
            _fader.FadeOut();
        }
    }

    private void StopUpdating()
    {
        enabled = false;
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
