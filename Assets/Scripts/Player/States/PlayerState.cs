using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState : StateBase<PlayerController>
{
    public PlayerState(PlayerController owner) : base(owner)
    {
    }

    public virtual void LateUpdate()
    {

    }

    public virtual void OnDeath() { }

    public virtual bool IsGrappleInRange(GrappleZone grapple)
    {
        return Vector3.Distance(_owner.transform.position, grapple.transform.position) <= grapple.MaxTetherLength;
    }

    public virtual bool CanUseButton(WallButton button)
    {
        return false;
    }

    public virtual bool CanUseDoor(Door door)
    {
        return false;
    }

    public virtual bool CanUseLock(Lock locked)
    {
        return false;
    }

    public virtual bool CanPickup(InventoryItem item)
    {
        return false;
    }

    public virtual bool CanPushBlock(PushBlock block)
    {
        return false;
    }

    public virtual void TriggerButton(WallButton button) { }

    public virtual void TriggerPickup(InventoryItem item) { }

    public virtual void TriggerDoor(Door door) { }

    public virtual void TriggerLock(Lock locked) { }

    public virtual void TriggerDrainpipe(Drainpipe drainpipe) { }

    public virtual void TriggerLadder(Ladder ladder) { }

    public virtual void TriggerWallclimb(WallclimbSurface surface) { }

    public virtual void OnWaterEnter(WaterZone volume) 
    {
        _owner.SFX.GroundType = GroundSFX.Water;
    }

    public virtual void OnWaterStay(WaterZone volume) { }

    public virtual void OnWaterExit(WaterZone volume) 
    {
        _owner.SFX.GroundType = GroundSFX.Default;
    }

    public virtual void OnSplineEnter(SplineCollider collider) { }

    public virtual void OnSplineStay(SplineCollider collider) { }

    public virtual void OnSplineExit(SplineCollider collider) { }

    public virtual void RungIncrement(int amount) { }

    public virtual void OnCheckpoint(Checkpoint checkpoint) 
    {
        _owner.SFX.PlayCheckpoint();
    }

    public virtual void UpdateAnimation(PlayerAnim animControl) 
    {
        animControl.IsGrounded = _owner.IsGrounded;
        animControl.InputUpdate(_owner.MoveInput.x, _owner.MoveInput.z);
        animControl.VelocityUpdate(_owner.Movement, _owner.transform);
    }
}
