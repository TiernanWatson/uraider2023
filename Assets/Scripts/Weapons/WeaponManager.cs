using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponPair Equiped { get; set; } = null;

    [SerializeField] private bool hasPistols = true;

    [Header("References")]
    [SerializeField] private Weapon leftPistol;
    [SerializeField] private Weapon rightPistol;

    private PlayerAnim _anim;
    private WeaponPair dualPistols;

    private void Awake()
    {
        if (hasPistols)
        {
            dualPistols = new WeaponPair(leftPistol, rightPistol);
            Equiped = dualPistols;
        }
    }

    private void Start()
    {
        HolsterWeapon();

        _anim = GetComponent<PlayerAnim>();
        _anim.LeftEquipUsed += () => Equiped.LeftHand.Fire();
        _anim.RightEquipUsed += () => Equiped.RightHand.Fire();
    }

    public void HolsterWeapon()
    {
        Equiped.Holster();
    }

    public void DrawWeapon()
    {
        Equiped.Equip();
    }

    private void WeaponFired(int hand)
    {
        if (hand == 0)
        {
            Equiped.LeftHand.Fire();
        }
        else
        {
            Debug.Log("fire right");
            Equiped.RightHand.Fire();
        }
    }
}
