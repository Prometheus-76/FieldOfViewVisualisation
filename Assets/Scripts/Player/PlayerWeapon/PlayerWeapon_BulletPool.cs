using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerWeapon
{
    #region Public Methods

    public void ReturnBulletInstance(Bullet bulletInstance)
    {
        if (preventDuplicateReturns && availableBullets.Contains(bulletInstance)) return;

        // Return it to the pool
        availableBullets.AddLast(bulletInstance);
    }

    #endregion

    private void BulletPoolStart()
    {
        availableBullets = new LinkedList<Bullet>();
        allBullets = new List<Bullet>();
    }

    private Bullet GetBulletInstance()
    {
        Bullet bulletInstance;

        if (availableBullets.Count > 0)
        {
            bulletInstance = availableBullets.First.Value;
            availableBullets.RemoveFirst();
        }
        else
        {
            bulletInstance = CreateBullet(currentProfile);
        }

        return bulletInstance;
    }

    private Bullet CreateBullet(WeaponProfile weaponProfile)
    {
        // Create the bullet object
        GameObject bulletObject = Instantiate(bulletPrefab);
        Bullet bulletComponent = bulletObject.GetComponent<Bullet>();

        // Add to complete pool
        allBullets.Add(bulletComponent);

        // Store as inactive child
        bulletObject.transform.parent = transform;
        bulletObject.SetActive(false);

        // Initialise this bullet
        bulletComponent.Initialise(weaponProfile);

        return bulletComponent;
    }

    private void ReconfigureBulletPool(WeaponProfile weaponProfile)
    {
        // Determine how many bullets we need to adjust the total by (+ or -)
        // Create any extra bullets we need

        // (Optional): Cull excess bullets if we have too many
    }
}
