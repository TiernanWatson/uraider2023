using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RungMaker : MonoBehaviour
{
    public float Height => GetHeight();
    public BoxCollider Collider => _collider;

    [SerializeField] private bool _startAtTop = false;
    [SerializeField] private float _rungSize = 0.625f;
    [SerializeField] private int _rungCount = 1;
    [SerializeField] private BoxCollider _collider;

    private void Awake()
    {
        if (!_collider)
        {
            Debug.LogError(gameObject.name + " has no collider on rungs");
        }
    }

    void OnDrawGizmosSelected()
    {
        for (int rung = 0; rung <= _rungCount; rung++)
        {
            float height = GetHeightAt(rung);
            Vector3 rayStart = transform.position + transform.up * height;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rayStart, transform.forward);
        }
    }

    public Vector3 ClosestPointTo(Vector3 position)
    {
        return Physics.ClosestPoint(
            position,
            _collider,
            _collider.transform.position,
            _collider.transform.rotation);
    }

    public float GetHeightAt(int rung)
    {
        float start = _startAtTop ? GetHeight() % _rungSize : 0.0f;
        return start + _rungSize * rung;
    }

    public int ClosestRungTo(Vector3 point, bool alwaysRndDown = true)
    {
        float start = _startAtTop ? GetHeight() % _rungSize : 0.0f;
        float height = (point - transform.position).y;
        float rungf = (height - start) / _rungSize;
        int rung = alwaysRndDown ? (int)rungf : Mathf.RoundToInt(rungf);
        return Mathf.Clamp(rung, 0, GetTopRung());
    }

    /// <summary>
    /// Maximum rung number
    /// </summary>
    /// <returns>Top rung number</returns>
    public int GetTopRung()
    {
        return (int)(GetHeight() / _rungSize);
    }

    /// <summary>
    /// Height of collider including left over bits
    /// </summary>
    /// <returns>Height</returns>
    public float GetHeight()
    {
        return _collider.size.y * _collider.transform.lossyScale.y;
    }
}
