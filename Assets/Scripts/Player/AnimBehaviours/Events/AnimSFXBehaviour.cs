using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSFXBehaviour : TimedMethodAnimBehaviour
{
    enum SoundType
    {
        FootRun,
        FootWalk,
        Swim,
        Jump,
        Land,
        HandShimmy,
        Scuff,
        Slap,
        Swoosh,
        Vault,
        KneeScuff,
        HardLand,
        Wade,
        SurfaceSwim,
        ClimbExert,
        Ladder
    }

    [SerializeField] private float _chance = 1.0f;
    [SerializeField] private SoundType _sound;

    private PlayerSFX _sfx;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, animatorStateInfo, layerIndex);

        if (!_sfx)
        {
            _sfx = animator.GetComponent<PlayerController>().SFX;
        }
    }

    protected override void Execute()
    {
        float probability = Random.Range(0.0f, 1.0f);

        if (probability > _chance)
        {
            return;
        }

        switch (_sound)
        {
            case SoundType.FootRun:
                _sfx.PlayFootRun();
                break;
            case SoundType.FootWalk:
                _sfx.PlayFootWalk();
                break;
            case SoundType.Jump:
                _sfx.PlayJump();
                break;
            case SoundType.Scuff:
                _sfx.PlayScuff();
                break;
            case SoundType.Slap:
                _sfx.PlaySlap();
                break;
            case SoundType.Swoosh:
                _sfx.PlaySwoosh();
                break;
            case SoundType.Vault:
                _sfx.PlayVault();
                break;
            case SoundType.HandShimmy:
                _sfx.PlayShimmy();
                break;
            case SoundType.Wade:
                _sfx.PlayWade();
                break;
            case SoundType.Swim:
                _sfx.PlaySwim();
                break;
            case SoundType.KneeScuff:
                _sfx.PlayKneeScuff();
                break;
            case SoundType.SurfaceSwim:
                _sfx.PlaySurfaceSwim();
                break;
            case SoundType.ClimbExert:
                _sfx.PlayClimbExert();
                break;
            case SoundType.Ladder:
                _sfx.PlayLadder();
                break;
            default:
                Debug.LogWarning("Selected sound: " + _sound + " not found");
                break;
        }
    }
}
