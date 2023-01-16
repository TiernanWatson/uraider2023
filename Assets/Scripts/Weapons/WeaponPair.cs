using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPair 
{
    public Weapon RightHand => rightHand;
    public Weapon LeftHand => leftHand;

    private Weapon leftHand;
    private Weapon rightHand;

    private bool rightLastFired = false;

    public WeaponPair(Weapon left, Weapon right)
    {
        this.leftHand = left;
        this.rightHand = right;
    }

    public void Holster()
    {
        leftHand.Holster();
        rightHand.Holster();
    }

    public void Equip()
    {
        leftHand.Equip();
        rightHand.Equip();
    }

    public void TryUse()
    {
        if (rightLastFired)
        {
            if (leftHand.Fire())
            {
                rightLastFired = false;
            }
        }
        else
        {
            if (rightHand.Fire())
            {
                rightLastFired = true;
            }
        }
    }
}
