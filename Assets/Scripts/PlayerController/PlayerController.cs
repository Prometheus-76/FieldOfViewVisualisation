using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D playerRigidbody;
    public CircleCollider2D playerCollider;

    [Header("Configuration")]
    public float movementSpeed;
    public float movementAcceleration;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 currentVelocity = playerRigidbody.velocity;
        Vector2 targetVelocity = InputManager.GetMovementDirection(true) * movementSpeed;
        Vector2 velocityDifference = (targetVelocity - currentVelocity);

        Vector2 appliedAcceleration = velocityDifference * movementAcceleration;
        appliedAcceleration = Vector2.ClampMagnitude(appliedAcceleration, velocityDifference.magnitude / Time.fixedDeltaTime);

        playerRigidbody.AddForce(appliedAcceleration, ForceMode2D.Force);
    }
}
