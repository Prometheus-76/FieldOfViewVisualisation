using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputManager
{
    private const float minDeadzone = 0.01f;
    private const float maxDeadzone = 0.99f;
    private const float triggerPressThreshold = 0.8f;

    public enum ControlScheme
    {
        MouseAndKeyboard,
        Controller
    }

    // PRIVATE
    private static InputMaster inputMaster = null;
    private static ControlScheme currentControlScheme;

    private static InputMaster Instance
    {
        get
        {
            if (inputMaster == null)
            {
                inputMaster = new InputMaster();
                inputMaster.Enable();
            }

            return inputMaster;
        }
    }

    private static void UpdateControlScheme(InputAction inputAction)
    {
        // Check action and input device are valid
        if (inputAction == null) return;
        if (inputAction.activeControl == null) return;
        if (inputAction.activeControl.device == null) return;

        switch (inputAction.activeControl.device)
        {
            case Keyboard or Mouse:
                currentControlScheme = ControlScheme.MouseAndKeyboard;
                break;

            case Gamepad:
                currentControlScheme = ControlScheme.Controller;
                break;
        }
    }

    private static Vector2 DeadzoneRemap(Vector2 input)
    {
        float inputMagnitude = input.magnitude;

        // Prevent divide by zero
        if (inputMagnitude == 0f) return Vector2.zero;

        // Clamp vector
        if (inputMagnitude < minDeadzone)
        {
            float multiplier = minDeadzone / inputMagnitude;

            input *= multiplier;
            inputMagnitude *= multiplier;
        }
        else if (inputMagnitude > maxDeadzone)
        {
            float multiplier = maxDeadzone / inputMagnitude;

            input *= multiplier;
            inputMagnitude *= multiplier;
        }

        // Remap to 0-1
        float deadzoneRange = maxDeadzone - minDeadzone;
        float magnitude01 = Mathf.Clamp01((inputMagnitude - minDeadzone) / deadzoneRange);

        return input.normalized * magnitude01;
    }

    #region Public Methods

    /// <summary>
    /// Returns the most recently used control scheme
    /// </summary>
    /// <returns>The most recently used control scheme</returns>
    public static ControlScheme GetControlScheme()
    {
        return currentControlScheme;
    }

    /// <summary>
    /// Calculate the current movement input direction
    /// </summary>
    /// <param name="normalized">Whether the returned vector should be normalized</param>
    /// <returns>The movement input direction, clamped between 0 and 1</returns>
    public static Vector2 GetMovementDirection(bool normalized)
    {
        InputAction action = Instance.Player.Movement;
        UpdateControlScheme(action);

        Vector2 actionValue = action.ReadValue<Vector2>();

        // Deadzone for control sticks
        if (currentControlScheme == ControlScheme.Controller) actionValue = DeadzoneRemap(actionValue);

        return normalized ? actionValue.normalized : actionValue;
    }

    /// <summary>
    /// Calculate the direction from the player to the mouse cursor, normalized
    /// </summary>
    /// <param name="playerScreenPosition">The player's position on the screen</param>
    /// <returns>The normalized vector from the player to the mouse cursor</returns>
    public static Vector2 GetAimDirection(Vector2 playerScreenPosition)
    {
        InputAction action = null;
        
        // Getting cursor position polls every frame, overriding the controller scheme
        if (currentControlScheme == ControlScheme.MouseAndKeyboard)
        {
            // Check for controller aim input
            if (DeadzoneRemap(Instance.Player.AimDirection.ReadValue<Vector2>()).sqrMagnitude > 0f)
            {
                // Controller is aiming!
                currentControlScheme = ControlScheme.Controller;
            }
        }

        // Make sure we read from the correct event
        switch (currentControlScheme)
        {
            case ControlScheme.MouseAndKeyboard:
                action = Instance.Player.AimPosition;
                break;

            case ControlScheme.Controller:
                action = Instance.Player.AimDirection;
                break;
        }

        Vector2 actionValue = action.ReadValue<Vector2>();

        if (currentControlScheme == ControlScheme.MouseAndKeyboard)
        {
            // Create player -> mouse cursor vector
            Vector2 playerToCursor = actionValue - playerScreenPosition;
            return playerToCursor.normalized;
        }
        else if (currentControlScheme == ControlScheme.Controller)
        {
            // Deadzone for control sticks
            return DeadzoneRemap(actionValue).normalized;
        }

        // Fallback, should never reach
        return Vector2.zero;
    }

    /// <summary>
    /// Return the position of the mouse cursor, in screen-space (NOTE: only valid with mouse and keyboard)
    /// </summary>
    /// <returns>The position of the mouse cursor in screen-space, null if not using mouse and keyboard</returns>
    public static Vector2? GetAimPosition()
    {
        Vector2? aimPosition = null;

        // Only returns a valid value if the player is using mouse and keyboard
        if (currentControlScheme == ControlScheme.MouseAndKeyboard)
        {
            aimPosition = Instance.Player.AimPosition.ReadValue<Vector2>();
        }

        return aimPosition;
    }

    /// <summary>
    /// Whether the player is holding the primary fire binding down
    /// </summary>
    /// <returns>The button held state</returns>
    public static bool GetPrimaryFire()
    {
        return Instance.Player.PrimaryFire.ReadValue<float>() >= triggerPressThreshold;
    }

    /// <summary>
    /// Whether the player is holding the secondary fire binding down
    /// </summary>
    /// <returns>The button held state</returns>
    public static bool GetSecondaryFire()
    {
        return Instance.Player.SecondaryFire.ReadValue<float>() >= triggerPressThreshold;
    }

    #endregion
}
