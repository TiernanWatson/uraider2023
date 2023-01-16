using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconScale : MonoBehaviour
{
    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        float size = Mathf.Sin(4.0f * Time.time) / 15.0f;
        Vector3 scale = new Vector3(1.0f + size, 1.0f + size, 1.0f);
        _rect.localScale = scale;
    }
}
