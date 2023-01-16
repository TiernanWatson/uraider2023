using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public class CrouchState : GroundState
    {
        private bool _isGoingToAir;
        private float _originalHeight;
        private float _originalStepOffset;

        public CrouchState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _isGoingToAir = false;
            _originalHeight = _owner.Movement.Motor.CapsuleHeight;
            _originalStepOffset = _owner.Movement.Motor.StepOffset;

            _owner.Movement.Motor.CapsuleHeight = 0.75f;
            _owner.Movement.Motor.StepOffset = 0.35f;
            _owner.Movement.Motor.UseHeadCollision = true;
            _owner.Movement.Roam.StopAtLedges = true;
            _owner.Movement.Roam.TurnRate = _owner.settings.crawlTurnRate;
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.Movement.Roam.StopAtLedges = false;
            _owner.Movement.Motor.CapsuleHeight = _originalHeight;
            _owner.Movement.Motor.StepOffset = _originalStepOffset;
            _owner.Movement.Motor.UseHeadCollision = false;
        }

        public override void Update()
        {
            if (_isGoingToAir)
            {
                if (_owner.AnimControl.IsIn("FallBlend") || _owner.AnimControl.IsInTrans("CrawlIdle -> CrawlRollOut"))
                {
                    _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
                }
                return;
            }

            bool stopCrouching = _owner.UInput.Crouch.ReadValue<float>() < 0.1f &&
                !_owner.CheckCapsule(_owner.transform.position, 1.75f);

            if (stopCrouching && !_goingToGrab)
            {
                _owner.AnimControl.CrouchToIdle();
                _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
            }
            else if (_owner.UInput.Jump.triggered && _owner.Movement.Roam.StoppedAtEdge && _owner.AnimControl.IsIn("CrawlIdle"))
            {
                _owner.Movement.Motor.UseGroundingForce = false;
                _owner.AnimControl.Play("CrawlRollOut");
                _isGoingToAir = true;
            }
            else
            {
                base.Update();
            }
        }

        public override bool CanPickup(InventoryItem item)
        {
            return _owner.AnimControl.IsIn("CrouchIdle");
        }

        public override void TriggerPickup(InventoryItem item)
        {
            base.TriggerPickup(item);

            _owner.AnimControl.PickUp += () => item.Pickup(_owner);
            _owner.AnimControl.Play("CrouchPickup");
        }
    }
}
