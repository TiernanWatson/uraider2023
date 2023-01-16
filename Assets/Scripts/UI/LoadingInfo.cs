using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingInfo : MonoBehaviour
{
    public static LoadingInfo Active
    {
        get;
        private set;
    }

    public LevelInfo Info { get; private set; }

    public static void Create(LevelInfo info)
    {

    }
}
