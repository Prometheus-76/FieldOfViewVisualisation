using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : PlayerWeapon
{
    [Header("Damage")]
    [Min(0f)]
    public float damagePerShot;
    [Range(0f, 1f)]
    public float damageFalloffStart;
    [Range(0f, 1f)]
    public float damageFalloffMultiplier;

    [Header("Knockback")]
    [Min(0f)]
    public float knockbackPerShot;

    [Header("Fire Rate")]
    [Min(0)]
    public int minimumRPM;
    [Min(0)]
    public int maximumRPM;

    [Header("Spin")]
    [Min(0f)]
    public float spinUpDuration;
    [Min(0f)]
    public float spinDownDuration;
    public AnimationCurve spinCurve;

    [Header("Magazine")]
    [Min(0f)]
    public float secondsInFullMagazine;

    [Header("Bullet Travel")]
    [Min(0f)]
    public float bulletRange;
    [Min(0f)]
    public float bulletVelocity;

    [Header("Accuracy")]
    [Range(0f, 45f)]
    public float minimumSpreadAngle;
    [Range(0f, 45f)]
    public float maximumSpreadAngle;

    [Header("Movement")]
    [Range(0f, 1f)]
    public float minimumMovementPenalty;
    [Range(0f, 1f)]
    public float maximumMovementPenalty;

    [Header("Rumble")]
    public AnimationCurve lowFrequencyRumble;
    public AnimationCurve highFrequencyRumble;
    public RumbleSystem rumbleSystem;

    // PRIVATE
    private float currentLinearSpin = 0f;
    private float currentRemappedSpin = 0f;

    private float timeUntilNextFire = 0f;

    private float currentRPM;
    private float currentPaintPerBullet;
    private float currentSpreadAngle;
    private float currentMovementPenalty;
    private float currentLowFrequencyRumble;
    private float currentHighFrequencyRumble;

    public override void WeaponInitialise(PlayerController playerController)
    {
        base.WeaponInitialise(playerController);

        // Calculate and create maximum number of bullets
        // bulletLifetime, maxRPM, etc

        WeaponReset(true);
    }

    public override void WeaponUpdate(float deltaTime, bool isCurrentlyHeld, bool isTriggerHeld, Vector2 aimDirection)
    {
        UpdateBullets(deltaTime);

        if (isCurrentlyHeld == false) return;

        // Fire bullet/s as necessary this frame
        UpdateFiring(deltaTime, isTriggerHeld, aimDirection);

        // Modify spin speed, and adjust weapon properties accordingly
        UpdateSpin(deltaTime, isTriggerHeld);
        UpdateSpinProperties();

        // Shake the controller
        rumbleSystem.AddRumbleThisFrame(isTriggerHeld ? currentLowFrequencyRumble : 0f, currentHighFrequencyRumble);
    }

    public override void WeaponReset(bool resetBullets)
    {
        // Recall bullets from the field
        if (resetBullets) ResetBullets();

        // Reset spin speed and properties
        currentLinearSpin = 0f;
        currentRemappedSpin = 0f;
        timeUntilNextFire = 0f;

        UpdateSpinProperties();

        isFiring = false;
    }

    private void UpdateBullets(float deltaTime)
    {
        // Simulate, etc
    }

    private void ResetBullets()
    {
        // Recall all bullets from this gun to the pool
    }

    private void UpdateFiring(float deltaTime, bool isTriggerHeld, Vector2 aimDirection)
    {        
        // If firing is on cooldown, reduce that cooldown
        if (timeUntilNextFire > 0f)
        {
            timeUntilNextFire -= deltaTime;
            isFiring = true;
        }

        // While firing is off cooldown
        while (timeUntilNextFire <= 0f)
        {
            if (isTriggerHeld)
            {
                // Fire another bullet
                
                // Increment cooldown until next bullet is fired
                float firingInterval = (60f / currentRPM);
                timeUntilNextFire += firingInterval;

                // Fire this bullet
                FireBullet(aimDirection, timeUntilNextFire);
            }
            else
            {
                // Reset firing mechanism

                // Don't fire a bullet, cooldown is over so we don't need to refresh it
                timeUntilNextFire = 0f;
                isFiring = false;

                break;
            }
        }
    }

    private void FireBullet(Vector2 direction, float initialSimulationStep)
    {
        
    }

    private void UpdateSpin(float deltaTime, bool isTriggerHeld)
    {
        // Spin up/down
        float linearIncrement = deltaTime / (isTriggerHeld ? spinUpDuration : -spinDownDuration);
        currentLinearSpin = Mathf.Clamp01(currentLinearSpin + linearIncrement);

        // Calculate remapped spin
        currentRemappedSpin = spinCurve.Evaluate(currentLinearSpin);
    }

    private void UpdateSpinProperties()
    {
        currentRPM = Mathf.Lerp(minimumRPM, maximumRPM, currentRemappedSpin);

        float roundsPerSecond = (currentRPM / 60f);
        float bulletsPerMag = roundsPerSecond * secondsInFullMagazine;
        currentPaintPerBullet = 1f / bulletsPerMag;

        currentSpreadAngle = Mathf.Lerp(minimumSpreadAngle, maximumSpreadAngle, currentRemappedSpin);

        currentMovementPenalty = Mathf.Lerp(minimumMovementPenalty, maximumMovementPenalty, currentRemappedSpin);

        currentLowFrequencyRumble = lowFrequencyRumble.Evaluate(currentLinearSpin);
        currentHighFrequencyRumble = highFrequencyRumble.Evaluate(currentLinearSpin);
    }
}
