using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerWeapon
{
    private void InputUpdate(float deltaTime)
    {
        // Button presses
        primaryFireInputHeld = InputManager.GetPrimaryFire();
        secondaryFireInputHeld = InputManager.GetSecondaryFire();

        // Directional input
        Vector2 playerScreenPosition = mainCamera.WorldToScreenPoint(playerTransform.position);
        fireDirection = InputManager.GetAimDirection(playerScreenPosition);

        if (InputManager.GetControlScheme() == InputManager.ControlScheme.Controller)
        {
            // If we're not aiming with the controller
            if (fireDirection.sqrMagnitude <= 0f)
            {
                Vector2 movementDirection = InputManager.GetMovementDirection(true);

                // If we are moving with the controller
                if (movementDirection.sqrMagnitude > 0f)
                {
                    // Make the player aim straight ahead in the direction they're moving
                    fireDirection = movementDirection;
                }
            }
        }

        // Cursor position
        firePosition = InputManager.GetAimPosition();
    }
}
