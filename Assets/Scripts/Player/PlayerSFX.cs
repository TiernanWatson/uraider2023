using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundSFX
{
    Default,
    Metal,
    Wood,
    Water
}

[RequireComponent(typeof(AudioSource))]
public class PlayerSFX : MonoBehaviour
{
    public GroundSFX GroundType { get; set; } = GroundSFX.Default;
    public AudioSource Vox => _voxSource;

#pragma warning disable 0649
    [SerializeField] private AudioClip[] feetSounds;
    [SerializeField] private AudioClip[] _metalFeetSounds;
    [SerializeField] private AudioClip[] _woodFeetSounds;
    [SerializeField] private AudioClip[] _waterFeetSounds;
    [SerializeField] private AudioClip[] _scuffSounds;
    [SerializeField] private AudioClip[] jumpSounds;
    [SerializeField] private AudioClip[] vaultSounds;
    [SerializeField] private AudioClip[] gruntSounds;
    [SerializeField] private AudioClip[] _climbExertSounds;
    [SerializeField] private AudioClip[] _splashSounds;
    [SerializeField] private AudioClip[] _wadeSounds;
    [SerializeField] private AudioClip[] _swimSounds;
    [SerializeField] private AudioClip[] _surfaceSwimSounds;
    [SerializeField] private AudioClip[] _flopSounds;
    [SerializeField] private AudioClip[] _shimmySounds;
    [SerializeField] private AudioClip[] _ladderSounds;
    [SerializeField] private AudioClip[] _kneeScuffSounds;
    [SerializeField] private AudioClip screamSound;
    [SerializeField] private AudioClip swooshSound;
    [SerializeField] private AudioClip slapSound;
    [SerializeField] private AudioClip _checkpointSound;
    [SerializeField] private AudioClip _grappleWhindSound;
    [SerializeField] private AudioSource _voxSource;
#pragma warning restore 0649

    private AudioSource source;
    private Animator anim;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    public void Halt()
    {
        source.Stop();
    }

    public void PlayGrappleWhind()
    {
        source.PlayOneShot(_grappleWhindSound);
    }

    public void PlayFootRun()
    {
        if (anim.GetFloat("TargetSpeed") > 2f)
            PlayFoot();
    }

    public void PlayFootWalk()
    {
        if (anim.GetFloat("TargetSpeed") <= 2f)
            PlayFoot();
    }

    public void PlayFoot()
    {
        AudioClip[] array;
        switch (GroundType)
        {
            case GroundSFX.Metal:
                array = _metalFeetSounds;
                break;
            case GroundSFX.Wood:
                array = _woodFeetSounds;
                break;
            case GroundSFX.Water:
                array = _waterFeetSounds;
                break;
            default:
                array = feetSounds;
                break;
        }

        PlayRandom(array);
    }

    public void PlayScuff()
    {
        PlayRandom(_scuffSounds);
    }

    public void PlayClimbExert()
    {
        PlayRandomVox(_climbExertSounds);
    }

    public void PlayShimmy()
    {
        PlayRandom(_shimmySounds);
    }

    public void PlayLadder()
    {
        PlayRandom(_ladderSounds);
    }

    public void PlayJump()
    {
        PlayRandomVox(jumpSounds);
    }

    public void PlayVault()
    {
        PlayRandomVox(vaultSounds);
    }

    public void PlayScream()
    {
        _voxSource.PlayOneShot(screamSound);
    }

    public void PlaySplash()
    {
        PlayRandom(_splashSounds);
    }

    public void PlaySwoosh()
    {
        source.PlayOneShot(swooshSound);
    }

    public void PlaySlap()
    {
        source.PlayOneShot(slapSound);
    }

    public void PlayGrunt()
    {
        PlayRandomVox(gruntSounds);
    }

    public void PlaySwim()
    {
        PlayRandom(_swimSounds);
    }

    public void PlaySurfaceSwim()
    {
        PlayRandom(_surfaceSwimSounds);
    }

    public void PlayKneeScuff()
    {
        PlayRandom(_kneeScuffSounds);
    }

    public void PlayWade()
    {
        PlayRandom(_wadeSounds);
    }

    public void PlayFlop()
    {
        PlayRandom(_flopSounds);
    }

    public void PlayCheckpoint()
    {
        PlayOnce(_checkpointSound);
    }

    public void PlayOnce(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips.Length == 0)
            return;

        int num = Random.Range(0, clips.Length - 1);
        source.PlayOneShot(clips[num]);
    }

    private void PlayRandomVox(AudioClip[] clips)
    {
        if (clips.Length == 0)
            return;

        int num = Random.Range(0, clips.Length - 1);
        _voxSource.PlayOneShot(clips[num]);
    }
}
