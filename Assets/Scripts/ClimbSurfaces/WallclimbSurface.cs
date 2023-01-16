using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RungMaker))]
public class WallclimbSurface : MonoBehaviour
{
    public RungMaker Rungs { get; private set; }
    public bool CanClimbOff => _canClimbOff;
    public bool CanStepOff => _canStepOff;
    public FreeclimbSurface FreeclimbUp => _freeclimbUp;
    public FreeclimbSurface FreeclimbDown => _freeclimbDown;

    #pragma warning disable 0649

    [SerializeField] private bool _canClimbOff;
    [SerializeField] private bool _canStepOff;
    [SerializeField] private FreeclimbSurface _freeclimbUp;
    [SerializeField] private FreeclimbSurface _freeclimbDown;

    #pragma warning restore 0649

    private void Awake()
    {
        Rungs = GetComponent<RungMaker>();
    }

    /*private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            var velocity = player.Movement.TargetVelocity;

            if (velocity.sqrMagnitude > 0.01f && Vector3.Angle(velocity, transform.forward) < 30.0f)
            {
                player.TriggerWallclimb(this);
            }
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.Triggers.Test(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.Triggers.Leave(this);
        }
    }

    public bool IsOutsideSurface(Vector3 point)
    {
        Vector3 relativePoint = point - transform.position;

        if (relativePoint.y < 0.0f || relativePoint.y > Rungs.Height)
        {
            return true;
        }
        else
        {
            Vector3 surfaceRight = transform.right;
            Vector3 centerToPoint = Vector3.Project(relativePoint, surfaceRight);
            float xDistance = centerToPoint.magnitude;

            return xDistance > (Rungs.Collider.size.x * Rungs.Collider.transform.lossyScale.x / 2.0f);
        }
    }
}
