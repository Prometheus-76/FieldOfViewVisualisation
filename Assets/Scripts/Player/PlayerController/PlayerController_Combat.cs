using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Combat")]
    public List<PlayerWeapon> playerWeapons;
    public WeaponType startingWeapon;

    private void CombatUpdate(float deltaTime)
    {
        // Manage weapon swapping and calling weapon functions
    }
}
