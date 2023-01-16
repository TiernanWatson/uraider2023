using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewLevelInfo", menuName = "URaider/LevelInfo", order = 2)]
public class LevelInfo : ScriptableObject
{
    public static bool UseEnter { get; set; } = true;
    public static LevelInfo Active { get; set; } = null;
    public static LevelSave Save { get; set; } = null;

    public int levelIndex = 0;
    public string levelName = string.Empty;
    public string levelInfo = string.Empty;
    public Sprite loadingImage;
}
