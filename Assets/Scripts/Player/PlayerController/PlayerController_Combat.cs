using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Combat")]
    public List<PlayerWeapon> playerWeapons;
    public WeaponType startingWeapon;

    public PlayerWeapon primaryWeapon { get; private set; }
    public PlayerWeapon secondaryWeapon { get; private set; }
    public bool holdingPrimaryWeapon { get; private set; }
    
    private void CombatStart()
    {
        InitialiseAllWeapons();

        EquipWeapon(startingWeapon, true);

        holdingPrimaryWeapon = true;
    }

    private void CombatUpdate(float deltaTime)
    {
        // Manage weapon swapping and calling weapon functions

        if (primaryWeapon != null) primaryWeapon.WeaponUpdate(deltaTime, holdingPrimaryWeapon, fireInputHeld, fireInputDirection);
        if (secondaryWeapon != null) secondaryWeapon.WeaponUpdate(deltaTime, !holdingPrimaryWeapon, fireInputHeld, fireInputDirection);
    }

    private void InitialiseAllWeapons()
    {
        for (int i = 0; i < playerWeapons.Count; i++)
        {
            playerWeapons[i].WeaponInitialise(this);
        }
    }

    private void EquipWeapon(WeaponType weaponType, bool equipInPrimarySlot)
    {
        if (CanWeaponBeEquipped(weaponType, equipInPrimarySlot) == false) return;
        PlayerWeapon replacementWeapon = GetWeaponByType(weaponType);

        // Equip the weapon in the desired slot
        if (equipInPrimarySlot)
        {
            // Reset previous weapon before replacing it
            if (primaryWeapon != null) primaryWeapon.WeaponReset(true);

            primaryWeapon = replacementWeapon;
        }
        else
        {
            // Reset previous weapon before replacing it
            if (secondaryWeapon != null) secondaryWeapon.WeaponReset(true);
            
            secondaryWeapon = replacementWeapon;
        }
    }

    private bool CanWeaponBeEquipped(WeaponType weaponType, bool equipInPrimarySlot)
    {
        PlayerWeapon replacementWeapon = GetWeaponByType(weaponType);
        PlayerWeapon otherWeapon = equipInPrimarySlot ? secondaryWeapon : primaryWeapon;

        // The replacement should be valid, and of a different type to the weapon in the other slot
        if (replacementWeapon == null) return false;
        if (otherWeapon != null && otherWeapon.weaponType == replacementWeapon.weaponType) return false;

        // The replacement is valid
        return true;
    }

    private PlayerWeapon GetWeaponByType(WeaponType weaponType)
    {
        for (int i = 0; i < playerWeapons.Count; i++)
        {
            if (playerWeapons[i].weaponType == weaponType)
            {
                return playerWeapons[i];
            }
        }

        return null;
    }
}
