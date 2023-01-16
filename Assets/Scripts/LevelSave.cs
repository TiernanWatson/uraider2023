using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSave
{
    public int LevelIndex { get; set; }
    public float[] PlayerPosition { get; set; }
    public float[] PlayerRotation { get; set; }
    public bool[] OpenDoors { get; set; }
    public bool[] PickedUp { get; set; }
    public bool[] DeadEnemies { get; set; }

    public LevelSave(LevelInfo info, Checkpoint checkpoint, CheckpointManager manager)
    {
        LevelIndex = info.levelIndex;

        PlayerPosition = new float[3];
        PlayerPosition[0] = checkpoint.transform.position.x;
        PlayerPosition[1] = checkpoint.transform.position.y;
        PlayerPosition[2] = checkpoint.transform.position.z;

        PlayerRotation = new float[3];
        PlayerRotation[0] = checkpoint.transform.eulerAngles.x;
        PlayerRotation[1] = checkpoint.transform.eulerAngles.y;
        PlayerRotation[2] = checkpoint.transform.eulerAngles.z;

        OpenDoors = new bool[manager.Doors.Length];
        for (int i = 0; i < OpenDoors.Length; i++)
        {
            OpenDoors[i] = manager.Doors[i].IsOpen;
        }

        PickedUp = new bool[manager.Items.Length];
        for (int i = 0; i < PickedUp.Length; i++)
        {
            PickedUp[i] = PlayerController.Local.Inventory.Contains(manager.Items[i], out _);
        }

        DeadEnemies = new bool[manager.Enemies.Length];
        for (int i = 0; i < DeadEnemies.Length; i++)
        {
            DeadEnemies[i] = manager.Enemies[i].IsDead;
        }
    }
}
