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

    // PROPERTIES
    public bool isFiring { get; protected set; }

    // PROTECTED
    protected PlayerController playerController = null;

    public virtual void WeaponInitialise(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public abstract void WeaponUpdate(float deltaTime, bool isCurrentlyHeld, bool isTriggerHeld, Vector2 aimDirection);

    public abstract void WeaponReset(bool resetBullets);
}
