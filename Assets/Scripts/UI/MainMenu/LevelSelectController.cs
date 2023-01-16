using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public class LevelSelectController : MonoBehaviour
{
    [SerializeField] private LevelInfo[] _levels;
    [SerializeField] private Transform _buttonsParent;
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private CanvasFader _fader;

    private void Start()
    {
        for (int i = 0; i < _levels.Length; i++)
        {
            LevelInfo info = _levels[i];

            // Need so lambda copies and doesn't just reference i
            int target = i;

            Button b = Instantiate(_buttonPrefab, _buttonsParent).GetComponent<Button>();
            b.onClick.AddListener(() => PlayLevel(target));

            Text t = b.transform.GetChild(0).GetComponent<Text>();
            t.text = info.levelName;
        }
    }

    public void PlayLevel(int index)
    {
        Debug.Log("Play:" + index);
        LoadingScript.Load(_levels[index], _fader, true);
    }
}
