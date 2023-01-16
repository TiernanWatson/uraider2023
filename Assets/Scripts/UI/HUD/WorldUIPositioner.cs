using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUIPositioner : MonoBehaviour
{
    private Transform _position;

    [SerializeField] private Camera _camera;
    [SerializeField] private PlayerInteractions _interactions;

    public void SetPosition(Transform position)
    {
        _position = position;
    }

    public void SetToInteraction()
    {
        _position = _interactions.Closest.IconLocation;
    }

    private void LateUpdate()
    {
        if (!_position)
        {
            return;
        }

        Vector3 screenSpace = _camera.WorldToScreenPoint(_position.position);
        transform.position = ClampPosition(screenSpace);
        /*if (IsOutsideScreen(screenSpace))
        {
        }
        else
        {
            transform.position = screenSpace;
            //transform.rotation = _camera.transform.rotation;
        }*/
    }

    private Vector3 ClampPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, 0.0f, _camera.pixelWidth);
        pos.y = Mathf.Clamp(pos.y, 0.0f, _camera.pixelHeight);
        return pos;
    }

    private bool IsOutsideScreen(Vector3 point)
    {
        return point.x > _camera.pixelWidth || point.x < 0.0f || point.y > _camera.pixelHeight || point.y < 0.0f;
    }
}
