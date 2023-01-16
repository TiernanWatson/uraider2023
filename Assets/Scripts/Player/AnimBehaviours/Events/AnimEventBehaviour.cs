using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventBehaviour : TimedMethodAnimBehaviour
{
    public enum EventType
    {
        HandInteract,
        Pickup,
        Equip,
        Unequip
    }

    [SerializeField] private EventType _event;
    [SerializeField] private string _argument;

    private PlayerController _player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, animatorStateInfo, layerIndex);

        if (!_player)
        {
            _player = animator.GetComponent<PlayerController>();
        }
    }

    protected override void Execute()
    {
        switch (_event)
        {
            case EventType.HandInteract:
                _player.AnimControl.HandOnInteract();
                break;
            case EventType.Pickup:
                _player.AnimControl.PickUpEvent();
                break;
            case EventType.Equip:
                _player.Equipment.Equip(_argument);
                break;
            case EventType.Unequip:
                _player.Equipment.Holster(_argument);
                break;
            default:
                Debug.LogError("Unsupported event type");
                break;
        }
    }
}
