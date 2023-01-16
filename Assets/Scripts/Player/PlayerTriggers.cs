using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggers : MonoBehaviour
{
    public event Action<GrappleZone> GrappleFound;

    public Drainpipe Drainpipe { get; private set; }
    public Ladder Ladder { get; private set; }
    public WallclimbSurface Wallclimb { get; private set; }
    public GrappleZone Grapple { get; private set; }

    public void Test(GrappleZone grapple)
    {
        Vector3 playerToGrapple = (grapple.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, playerToGrapple);

        if (angle > 90.0f)
        {
            return;
        }

        if (Grapple)
        {
            Vector3 playerToOldGrapple = (Grapple.transform.position - transform.position).normalized;
            float angleOld = Vector3.Angle(transform.forward, playerToOldGrapple);

            float oldDistance = Vector3.Distance(Grapple.transform.position, transform.position);
            float newDistance = Vector3.Distance(grapple.transform.position, transform.position);

            if (oldDistance < newDistance && angleOld < 90.0f)
            {
                return;
            }
        }

        Grapple = grapple;
        GrappleFound?.Invoke(grapple);
    }

    public void Test(Drainpipe drainpipe)
    {
        if (Drainpipe)
        {
            float oldDistance = Vector3.Distance(Drainpipe.transform.position, transform.position);
            float newDistance = Vector3.Distance(drainpipe.transform.position, transform.position);
            if (oldDistance < newDistance)
            {
                return;
            }
        }

        Drainpipe = drainpipe;
    }

    public void Test(Ladder ladder)
    {
        if (Ladder)
        {
            float oldDistance = Vector3.Distance(Ladder.transform.position, transform.position);
            float newDistance = Vector3.Distance(ladder.transform.position, transform.position);
            if (oldDistance < newDistance)
            {
                return;
            }
        }

        Ladder = ladder;
    }

    public void Test(WallclimbSurface wallclimb)
    {
        if (Wallclimb)
        {
            float oldDistance = Vector3.Distance(Wallclimb.transform.position, transform.position);
            float newDistance = Vector3.Distance(wallclimb.transform.position, transform.position);
            if (oldDistance < newDistance)
            {
                return;
            }
        }

        Wallclimb = wallclimb;
    }

    public void Leave(GrappleZone grapple)
    {
        if (Grapple == grapple)
        {
            Grapple = null;
            GrappleFound?.Invoke(null);
        }
    }

    public void Leave(Drainpipe drainpipe)
    {
        if (Drainpipe == drainpipe)
        {
            Drainpipe = null;
        }
    }

    public void Leave(Ladder ladder)
    {
        if (Ladder == ladder)
        {
            Ladder = null;
        }
    }

    public void Leave(WallclimbSurface wallclimb)
    {
        if (Wallclimb == wallclimb)
        {
            Wallclimb = null;
        }
    }

}
