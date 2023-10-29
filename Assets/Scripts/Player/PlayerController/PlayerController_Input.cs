using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    // PRIVATE
    private Vector2 movementInputDirection = Vector2.zero;

    private bool fireInputHeld = false;
    private Vector2 fireInputDirection = Vector2.up;
    private Vector2? fireInputPosition = null;

    private bool phaseInputHeld = false;

    private void InputUpdate(float deltaTime)
    {
        ReadMovementInput();

        ReadCombatInput();
    }

    private void ReadMovementInput()
    {
        // Button presses
        phaseInputHeld = InputManager.GetPhaseButton();

        // Directional input
        InputManager.ControlScheme controlScheme = InputManager.GetControlScheme();
        if (controlScheme == InputManager.ControlScheme.MouseAndKeyboard)
        {
            movementInputDirection = InputManager.GetMovementDirection(true);
        }
        else if (controlScheme == InputManager.ControlScheme.Controller)
        {
            movementInputDirection = InputManager.GetMovementDirection(false);
        }
    }

    private void ReadCombatInput()
    {
        // Button presses
        fireInputHeld = InputManager.GetFireButton();

        // Directional input
        Vector2 playerScreenPosition = mainCamera.WorldToScreenPoint(playerTransform.position);
        fireInputDirection = InputManager.GetAimDirection(playerScreenPosition);

        if (InputManager.GetControlScheme() == InputManager.ControlScheme.Controller)
        {
            // If we're not aiming with the controller
            if (fireInputDirection.sqrMagnitude <= 0f)
            {
                Vector2 movementDirection = InputManager.GetMovementDirection(true);

                // If we are moving with the controller
                if (movementDirection.sqrMagnitude > 0f)
                {
                    // Make the player aim straight ahead in the direction they're moving
                    fireInputDirection = movementDirection;
                }
            }
        }

        // Cursor position
        fireInputPosition = InputManager.GetAimPosition();
    }
}
