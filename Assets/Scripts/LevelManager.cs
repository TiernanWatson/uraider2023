using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    private string _savePath;

    public UnityEvent Paused;
    public UnityEvent Unpaused;
    public UnityEvent Completed;

    public static LevelManager Instance
    {
        get;
        private set;
    }

    public bool IsPaused { get; private set; }
    public bool IsInventory { get; private set; }
    public bool IsEnd { get; private set; }

    [SerializeField] private PlayerController _player;
    [Header("Serialization")]
    [SerializeField] private LevelInfo _levelInfo;
    [Header("UI References")]
    [SerializeField] private RingMenu _ringMenu;
    [SerializeField] private PauseMenu _pauseMenu;
    [SerializeField] private EndScreen _endScreen;
    [SerializeField] private CanvasFader _fader;
    [SerializeField] private GameObject _firstPauseSelected;
    [SerializeField] private EventSystem _uiEventSystem;
    

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("Two Level Managers active");
        }
        else
        {
            Instance = this;
            _savePath = Application.persistentDataPath + "/URaiderSave." + _levelInfo.levelIndex;
        }
    }

    private void Start()
    {
        HidePause();
        HideInventory();
        HideEnd();

        _player.Stats.OnDeath += ReloadLevel;

        _ringMenu.ItemUsed += (item) => HideInventory();

        LoadLevel();
    }

    private void OnDestroy()
    {
        // Make sure when level ends this disappears from static var
        if (Instance == this)
        {
            Instance = null;
        }

        _player.Stats.OnDeath -= ReloadLevel;
    }

    public void SaveLevel()
    {
        LevelInfo.Save = CreateSave();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(_savePath, FileMode.Create);

        bf.Serialize(stream, LevelInfo.Save);
        stream.Close();
    }

    public void LoadLevel()
    {
        if (LevelInfo.Save != null)
        {
            float[] position = LevelInfo.Save.PlayerPosition;
            float[] rotation = LevelInfo.Save.PlayerRotation;

            _player.SetPosition(new Vector3(position[0], position[1], position[2]));
            _player.SetRotation(Quaternion.Euler(rotation[0], rotation[1], rotation[2]));
            _player.CameraControl.transform.position = _player.transform.position;

            for (int i = 0; i < LevelInfo.Save.DeadEnemies.Length; i++)
            {
                if (LevelInfo.Save.DeadEnemies[i])
                {
                    CheckpointManager.Instance.Enemies[i].Kill();
                }
            }

            for (int i = 0; i < LevelInfo.Save.OpenDoors.Length; i++)
            {
                if (LevelInfo.Save.OpenDoors[i])
                {
                    CheckpointManager.Instance.Doors[i].OpenPush();
                }
            }

            for (int i = 0; i < LevelInfo.Save.PickedUp.Length; i++)
            {
                if (LevelInfo.Save.PickedUp[i])
                {
                    _player.Inventory.Add(CheckpointManager.Instance.Items[i]);
                }
            }
        }
    }

    public void EndLevel()
    {
        Time.timeScale = 0.0f;
        ShowEnd();
        Completed.Invoke();
    }

    public void ReloadLevel()
    {
        LoadingScript.Load(_levelInfo, _fader, false);
    }

    public void OnPause(InputValue value)
    {
        if (!IsInventory && !IsEnd)
        {
            if (IsPaused)
            {
                if (!_pauseMenu.HandleEscape() && _pauseMenu.CanExit())
                {
                    HidePause();
                }
            }
            else
            {
                ShowPause();
            }
        }
        else if (IsInventory)
        {
            HideInventory();
        }
    }

    public void OnInventory(InputValue value)
    {
        if (!IsInventory && !IsPaused && !IsEnd)
        {
            ShowInventory();
        }
        else if (IsInventory)
        {
            HideInventory();
        }
    }

    private LevelSave CreateSave()
    {
        return new LevelSave(_levelInfo, CheckpointManager.Instance.Current, CheckpointManager.Instance);
    }

    private void HideInventory()
    {
        _ringMenu.HideRing();
        Time.timeScale = 1.0f;
        IsInventory = false;
    }

    private void ShowInventory()
    {
        _ringMenu.DisplayRing();
        Time.timeScale = 0.0f;
        IsInventory = true;
    }

    public void ShowPause()
    {
        Cursor.visible = true;
        Time.timeScale = 0.0f;

        _pauseMenu.DisplayMenu();

        _uiEventSystem.SetSelectedGameObject(_firstPauseSelected);

        IsPaused = true;
        Paused.Invoke();
    }

    public void HidePause()
    {
        Cursor.visible = false;
        Time.timeScale = 1.0f;

        _pauseMenu.HideMenu();

        IsPaused = false;
        Unpaused.Invoke();
    }

    public void ShowEnd()
    {
        _endScreen.Show();
        Cursor.visible = true;
        IsEnd = true;
    }

    public void HideEnd()
    {
        _endScreen.Hide();
        Cursor.visible = false;
        IsEnd = false;
    }

    public void SetPause(bool value)
    {
        IsPaused = value;
        if (IsPaused)
        {
            Time.timeScale = 0.0f;
            Paused.Invoke();
        }
        else
        {
            Time.timeScale = 1.0f;
            Unpaused.Invoke();
        }
    }
}
