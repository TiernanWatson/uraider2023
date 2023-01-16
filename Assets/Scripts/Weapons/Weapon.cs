using System;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Equipment
{
    public event Action Fired;

    public Vector3 TargetPosition { get; set; }

    [SerializeField] private float _cooldownTime = 0.2f;
    [SerializeField] private float _damage = 10.0f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private LayerMask _fireLayers;

    private float _lastFire = 0.0f;

    public bool CanFire()
    {
        return Time.time - _lastFire > _cooldownTime;
    }

    public bool Fire()
    {
        if (CanFire())
        {
            Vector3 direction = (TargetPosition - _firePoint.position).normalized;
            Ray ray = new Ray(_firePoint.position, direction);

            Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.red, 2.0f);
            if (Physics.Raycast(
                ray, 
                out RaycastHit hit, 
                Mathf.Infinity, 
                _fireLayers.value, 
                QueryTriggerInteraction.Ignore))
            {
                IDamageable obj = hit.transform.GetComponent<IDamageable>();

                if (obj != null)
                {
                    Debug.Log("Hit: " + hit.transform.gameObject.name);
                    obj.Damage(_damage);
                }
            }

            _lastFire = Time.time;
            Fired?.Invoke();

            return true;
        }

        return false;
    }
}
