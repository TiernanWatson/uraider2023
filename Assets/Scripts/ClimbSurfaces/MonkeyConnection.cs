using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConnectionSide
{
    Z,
    MinusZ,
    X,
    MinusX
}

public class MonkeyConnection : MonoBehaviour
{
    public ConnectionSide Side => _side;
    public FreeclimbSurface Surface => _surface;
    public bool IsUp => _isUp;

    [SerializeField] private ConnectionSide _side;
    [SerializeField] private FreeclimbSurface _surface;
    [SerializeField] private bool _isUp = true;
}
