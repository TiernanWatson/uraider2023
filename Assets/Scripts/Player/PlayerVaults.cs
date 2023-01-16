using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerVaults : MonoBehaviour
{
    private List<Vault> _vaults;

    private void Awake()
    {
        _vaults = new List<Vault>();
    }

    public Vault GetClosest(float within = 30.0f)
    {
        Vault closest = null;
        float distClosest = Mathf.Infinity;

        for (int i = 0; i < _vaults.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, _vaults[i].transform.position);
            if (distance < distClosest)
            {
                float angleF = Vector3.Angle(transform.forward, _vaults[i].transform.forward);
                float angleB = Vector3.Angle(transform.forward, -_vaults[i].transform.forward);
                float angle = Mathf.Min(angleF, angleB);

                if (angle <= within)
                {
                    closest = _vaults[i];
                    distClosest = distance;
                }
            }
        }

        return closest;
    }

    public void Add(Vault useable)
    {
        _vaults.Add(useable);
    }

    public void Remove(Vault useable)
    {
        if (_vaults.Contains(useable))
        {
            _vaults.Remove(useable);
        }
    }
}
