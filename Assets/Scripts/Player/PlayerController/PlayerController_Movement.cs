using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed;
    public float movementAcceleration;

    // PRIVATE
    private float movementSpeedPenalty = 1f;

    private void MovementFixedUpdate(float fixedDeltaTime)
    {
        Move(movementInputDirection, fixedDeltaTime);
    }

    private void Move(Vector2 direction, float fixedDeltaTime)
    {
        Vector2 targetVelocity = direction * movementSpeed * movementSpeedPenalty;
        Vector2 velocityDifference = (targetVelocity - playerRigidbody.velocity);

        Vector2 appliedAcceleration = velocityDifference * movementAcceleration;
        appliedAcceleration = Vector2.ClampMagnitude(appliedAcceleration, velocityDifference.magnitude / fixedDeltaTime);

        playerRigidbody.AddForce(appliedAcceleration, ForceMode2D.Force);
    }

    public void SetMovementPenalty(float amount)
    {
        movementSpeedPenalty = amount;
    }

    public void Knockback(Vector2 force)
    {
        playerRigidbody.AddForce(force, ForceMode2D.Impulse);
    }
}
