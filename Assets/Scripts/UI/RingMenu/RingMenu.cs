using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RingMenu : MonoBehaviour
{
    public event Action<InventoryItem> ItemUsed;

    private PlayerInventory Inventory { get; set; }

    [Tooltip("Length from center to each object in ring")]
    [SerializeField] private float _radius = 4.0f;
    [Tooltip("Amount to scale each inventory object by for presentation")]
    [SerializeField] private float _itemScale = 4.0f;
    [SerializeField] private float _rotateRate = 10.0f;
    [SerializeField] private GameObject _ringContainer;
    [SerializeField] private CanvasFader _ui;
    [SerializeField] private Text _itemNameTxt;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _useText;

    private bool _isActive = false;
    private float _angleChange = 0.0f;
    private int _selectedIndex = 0;
    private InventoryItem _selectedItem = null;
    private PlayerController _player;

    private void Start()
    {
        _ui.FadedOut += () => _camera.enabled = false;

        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (!_player)
        {
            Debug.LogError("Could not find player controller for inventory!");
        }

        Inventory = _player.Inventory;
        if (!Inventory)
        {
            Debug.LogError("Could not find player inventory script for ring menu!");
        }

        _camera.enabled = false;
        HideRing();
    }

    private void Update()
    {
        if (_isActive)
        {
            // Decide and interpolate to object rotation
            float targetAngle = _angleChange * _selectedIndex;
            Quaternion targetRotation = Quaternion.Euler(0.0f, -targetAngle, 0.0f);
            _ringContainer.transform.rotation = Quaternion.Lerp(_ringContainer.transform.rotation, targetRotation, _rotateRate * Time.unscaledDeltaTime);

            // Keep all items facing camera
            foreach (var item in Inventory.Items)
            {
                item.Item.transform.rotation = Quaternion.LookRotation(Vector3.back);
            }
        }
    }

    public void OnInteract(InputValue value)
    {
        if (_isActive && _selectedItem.CanUseFromUI)
        {
            _selectedItem.Click(_player);
            ItemUsed?.Invoke(_selectedItem);

            if (_selectedItem.DestroyOnUse)
            {
                if (!Inventory.Remove(_selectedItem))
                {
                    Debug.LogWarning("Could not delete item from inventory");
                }
                Destroy(_selectedItem.gameObject);
            }
        }
    }

    public void OnMove(InputValue value)
    {
        if (_isActive)
        {
            Vector2 input = value.Get<Vector2>();
            if (Mathf.Abs(input.x) > 0.25f)
            {
                // Unity left-handed, so opposite rotation direction
                int direction = input.x > 0.0f ? -1 : 1;
                int index = _selectedIndex + direction;
                if (index < 0)
                {
                    index = Inventory.Items.Count - 1;
                }
                else if (index >= Inventory.Items.Count)
                {
                    index = 0;
                }

                _selectedIndex = index;
                _selectedItem = Inventory.Items[_selectedIndex].Item;
                SetSelectedText();
            }
        }
    }

    public void ArrangeObjects()
    {
        int itemCount = Inventory.Items.Count;

        if (itemCount == 0)
        {
            return;
        }

        _angleChange = 360.0f / itemCount;
        Vector3 basePosition = Vector3.back * _radius;

        for (int i = 0; i < itemCount; i++)
        {
            var info = Inventory.Items[i];
            var item = info.Item;

            if (i == 0)
            {
                _selectedIndex = 0;
                _selectedItem = item;
            }

            item.transform.parent = _ringContainer.transform;
            item.transform.localPosition = Quaternion.Euler(0.0f, _angleChange * i, 0.0f) * basePosition;
            item.transform.localRotation = Quaternion.LookRotation(Vector3.back);
            item.transform.localScale = Vector3.one * _itemScale;
            item.gameObject.SetActive(true);

            // Only render on ring camera
            item.gameObject.layer = 12;
            foreach (Transform t in item.transform)
            {
                t.gameObject.layer = 12;
            }

            // Only let ring menu light affect it
            foreach (var renderer in item.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.renderingLayerMask = 2;
            }
        }

        // Reset from any previous spinning
        _ringContainer.transform.rotation = Quaternion.identity;
    }

    public void DisplayRing()
    {
        _isActive = true;
        _selectedIndex = 0;
        _ui.FadeIn();
        _ringContainer.SetActive(true);
        _camera.enabled = true;

        ArrangeObjects();
        SetSelectedText();
    }

    public void HideRing()
    {
        _isActive = false;
        _ui.FadeOut();
        _ringContainer.SetActive(false);
    }

    private void SetSelectedText()
    {
        var info = Inventory.Items[_selectedIndex];
        _itemNameTxt.text = info.Item.ItemName + " - " + info.Count;
        _useText.SetActive(info.Item.CanUseFromUI);
    }
}
