using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region Public Methods

    public void Initialise(WeaponProfile weaponProfile)
    {
        // ASSIGN REQUIRED MEMORY HERE

        Reconfigure(weaponProfile);
    }

    public void Reconfigure(WeaponProfile weaponProfile)
    {
        // SET BULLET STATS HERE
    }

    public void Simulate(float deltaTime)
    {

    }

    public void ResetBullet()
    {
        // RESET BULLET TO CURRENT WEAPON PROFILE
    }

    #endregion
}
