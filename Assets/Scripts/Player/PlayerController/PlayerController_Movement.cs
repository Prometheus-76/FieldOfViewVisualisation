using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed;
    public float movementAcceleration;

    private void MovementFixedUpdate(float fixedDeltaTime)
    {
        Vector2 currentVelocity = playerRigidbody.velocity;
        Vector2 targetVelocity = movementInputDirection * movementSpeed;
        Vector2 velocityDifference = (targetVelocity - currentVelocity);

        Vector2 appliedAcceleration = velocityDifference * movementAcceleration;
        appliedAcceleration = Vector2.ClampMagnitude(appliedAcceleration, velocityDifference.magnitude / fixedDeltaTime);

        playerRigidbody.AddForce(appliedAcceleration, ForceMode2D.Force);
    }
}
