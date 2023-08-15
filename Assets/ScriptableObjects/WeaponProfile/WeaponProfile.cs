using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponProfile", menuName = "ScriptableObject/WeaponProfile")]
public class WeaponProfile : ScriptableObject
{
    [Header("Damage")]
    [Range(0f, 1f)]
    public float damageFalloffDistanceScalar;
    [Range(0f, 1f)]
    public float damageFalloffAmountScalar;

    [Header("Magazine")]
    [Range(1, 1000)]
    public int magazineBulletCapacity;
    [Min(1f)]
    public float magazinePaintCapacity;

    [Header("Fire Rate")]
    [Min(1)]
    public int bulletsPerMinute;

    [Header("Accuracy")]
    [Min(0f)]
    public float accuracyAngle;

    [Header("Bullet Travel")]
    [Range(1f, 50f)]
    public float bulletRange;
    [Range(0.1f, 100f)]
    public float bulletVelocity;
    [Min(0.1f)]
    public float bulletMaxUpdateDistance;

    [Header("Bullet Size")]
    [Range(0.1f, 1f)]
    public float bulletDiameter;

    [Header("Knockback")]
    [Min(0f)]
    public float knockbackPerSecond;

    [Header("Movement")]
    [Range(0f, 1f)]
    public float movementSpeedMultiplier;

    #region Public Methods

    public int CalculateMaxSimultaneousBullets()
    {
        int bulletsFiredInOneBulletLifetime = Mathf.CeilToInt(CalculateBulletLifetime() / CalculateFiringInterval());

        return (bulletsFiredInOneBulletLifetime + 1);
    }

    public float CalculateBulletLifetime()
    {
        return (bulletRange / bulletVelocity);
    }

    public float CalculateFiringInterval()
    {
        return (60f / bulletsPerMinute);
    }

    public float CalculateDamagePerBullet(float damagePerSecond)
    {
        float damagePerMinute = damagePerSecond * 60f;

        return (damagePerMinute / bulletsPerMinute);
    }

    public float CalculateKnockbackPerBullet()
    {
        float knockbackPerMinute = knockbackPerSecond * 60f;

        return (knockbackPerMinute / bulletsPerMinute);
    }

    public float CalculateFalloffStartDistance()
    {
        return (bulletRange * damageFalloffDistanceScalar);
    }

    public float CalculateMagazineTotalDamage(float damagePerSecond)
    {
        return (magazineBulletCapacity * CalculateDamagePerBullet(damagePerSecond));
    }

    public float CalculateFalloffMultiplier(float distance)
    {
        // Would cause divide by zero, because there is no damage falloff
        if (damageFalloffDistanceScalar >= 1f) return 1f;

        float falloffStartDistance = CalculateFalloffStartDistance();

        // If the bullet has not hit the falloff yet
        if (distance <= falloffStartDistance)
        {
            return 1f;
        }
        else
        {
            // Increases from 0 to 1 as the bullet travels through the falloff region
            float falloffInterpolant = Mathf.Clamp01(Mathf.InverseLerp(falloffStartDistance, bulletRange, distance));

            // Decreases from 1 as the bullet travels further
            float falloffMultiplier = ((1f - damageFalloffAmountScalar) * (1f - falloffInterpolant)) + damageFalloffAmountScalar;

            return falloffMultiplier;
        }
    }

    #endregion
}
