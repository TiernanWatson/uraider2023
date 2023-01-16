using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    public class NotEquipedState : PlayerState
    {
        public NotEquipedState(PlayerController owner) : base(owner)
        {
        }

        public override void Update()
        {
            if (_owner.UInput.Holster.triggered)
            {
                if (CanTransition())
                {
                    _owner.EquipedMachine.ChangeState(_owner.EquipedStates.Combat);
                }
            }
        }

        private bool CanTransition()
        {
            if (_owner.StateMachine.State == _owner.BaseStates.Locomotion)
            {
                return _owner.AnimControl.IsTag("Moving");
            }
            else
            {
                return _owner.CombatBaseStates.Contains(_owner.StateMachine.State)
                    && !_owner.AnimControl.IsIn("Dive");
            }
        }
    }
}
