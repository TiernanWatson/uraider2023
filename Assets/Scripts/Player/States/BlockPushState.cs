using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    public class BlockPushState : GroundState
    {
        public Vector3 BlockOffset { get; set; }
        public PushBlock PushBlock { get; set; }

        private bool _requestLetGo;

        public BlockPushState(PlayerController owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _requestLetGo = false;
            _owner.UInput.Interact.canceled += LetGoOfBlock;

            _owner.Movement.GoToPush();
            _owner.AnimControl.IsPushing = true;

            //Physics.IgnoreLayerCollision(8, 11);
        }

        public override void OnExit()
        {
            base.OnExit();

            _owner.UInput.Interact.canceled -= LetGoOfBlock;

            _owner.AnimControl.IsPushing = false;

            //Physics.IgnoreLayerCollision(8, 11, false);
        }

        public override void Update()
        {
            base.Update();

            if (_requestLetGo)
            {
                _owner.StateMachine.ChangeState(_owner.BaseStates.Locomotion);
            }
            else
            {
                if (_owner.AnimControl.IsIn("BlockPush") || _owner.AnimControl.IsIn("BlockPull"))
                {
                    Vector3 oldPosition = PushBlock.transform.position;

                    // Have block follow hand with offset
                    Vector3 newPosition = _owner.RightHand.position + BlockOffset;
                    Vector3 projectedOffset = Vector3.Project(newPosition, _owner.transform.forward);
                    Vector3 sideAmount = Vector3.Project(oldPosition, _owner.transform.right);
                    newPosition = sideAmount + projectedOffset;
                    newPosition.y = oldPosition.y;

                    PushBlock.transform.position = newPosition;

                    Vector3 projectedMove = Vector3.Project(_owner.AnimControl.DeltaPosition, _owner.transform.forward);
                    projectedMove += projectedMove.normalized * 0.1f;

                    Vector3 testPos = PushBlock.Collider.transform.position + projectedMove;
                    bool canMove = PushBlock.CanFit(testPos);

                    if (!canMove)
                    {
                        _owner.Movement.SetVelocity(Vector3.zero);
                        PushBlock.transform.position = oldPosition;
                    }
                }
            }
        }

        private void LetGoOfBlock(InputAction.CallbackContext obj)
        {
            _requestLetGo = true;
        }
    }
}