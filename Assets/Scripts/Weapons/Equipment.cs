using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public string Name => _name;

    [SerializeField] private string _name;
    [SerializeField] private GameObject _holsteredObj;

    public void Equip()
    {
        if (_holsteredObj != null)
        {
            _holsteredObj.SetActive(false);
        }

        gameObject.SetActive(true);
    }

    public void Holster()
    {
        if (_holsteredObj != null)
        {
            _holsteredObj.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
