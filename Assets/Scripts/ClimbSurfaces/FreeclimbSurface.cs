using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RungMaker))]
public class FreeclimbSurface : MonoBehaviour
{
    public RungMaker Rungs { get; private set; }
    public WallclimbSurface WallclimbUp => _wallclimbUp;
    public WallclimbSurface WallclimbDown => _wallclimbDown;
    public MonkeySurface MonkeyUp => _monkeyUp;
    public MonkeySurface MonkeyDown => _monkeyDown;

    [SerializeField] private WallclimbSurface _wallclimbUp;
    [SerializeField] private WallclimbSurface _wallclimbDown;
    [SerializeField] private MonkeySurface _monkeyUp;
    [SerializeField] private MonkeySurface _monkeyDown;

    private void Awake()
    {
        Rungs = GetComponent<RungMaker>();
    }
}
