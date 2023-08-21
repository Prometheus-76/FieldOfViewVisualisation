using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    MachineGun
}

public abstract class PlayerWeapon : MonoBehaviour
{
    [Header("Generic Configuration")]
    public WeaponType weaponType;

    public abstract void WeaponInitialise(PlayerController playerController);
    public abstract void WeaponUpdate(float deltaTime, bool isCurrentlyHeld, bool isTriggerHeld, Vector2 aimDirection);
    public abstract void WeaponReset();
}
