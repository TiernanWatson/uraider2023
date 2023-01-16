using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbMotor : PlayerMotor
{
    public LedgePoint Ledge { get; set; }
    /*
    protected override void AdjustMovementStep(ref Vector3 movement)
    {
       // owner.SetPosition(Ledge.MovePositionToLine(owner.transform.position, owner.transform.forward));

        Vector3 playerLevel = owner.transform.position;
        playerLevel.y = Ledge.transform.position.y;

        float distance = Vector3.Distance(playerLevel, Ledge.transform.position);

        // Stop player going beyond the end of a ledge
        if (Ledge.IsEnd && distance <= owner.Settings.ledgeEndPadding)
        {
            movement = Vector3.zero;
        }

        movement = Ledge.ProjectVelocity(movement);
    }*/
}
