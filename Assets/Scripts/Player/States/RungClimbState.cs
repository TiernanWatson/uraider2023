using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    public abstract class RungClimbState : PlayerState, IRungCountable
    {
        public int RungCount { get; set; }

        public RungClimbState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _owner.Movement.Wall.Activate();
        }

        public override void Update()
        {
            if (_owner.UInput.Crouch.triggered)
            {
                _owner.AnimControl.Fall();
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
            else if (_owner.UInput.Jump.triggered)
            {
                _owner.JumpBack();
                _owner.AnimControl.JumpB();
                _owner.StateMachine.ChangeState(_owner.BaseStates.Air);
            }
        }

        public override void UpdateAnimation(PlayerAnim animControl)
        {
            base.UpdateAnimation(animControl);

            _owner.AnimControl.RungsFromTop = GetTopRung() - RungCount;
            _owner.AnimControl.RungsFromBottom = RungCount - GetBottomRung();
            _owner.AnimControl.OnTopRung = RungCount == GetTopRung();
            _owner.AnimControl.OnBottomRung = RungCount == GetBottomRung();
        }

        public override void RungIncrement(int amount)
        {
            base.RungIncrement(amount);

            Debug.Log("Incre");

            RungCount += amount;
        }

        /// <summary>
        /// Used by the rung increment function to tell the animator if player can go up
        /// </summary>
        /// <returns>Topmost rung player can stand on</returns>
        public abstract int GetTopRung();

        /// <summary>
        /// Used by the rung increment function to tell the animator if player can go down
        /// </summary>
        /// <returns>Bottommost rung player can stand on</returns>
        public abstract int GetBottomRung();
    }
}
