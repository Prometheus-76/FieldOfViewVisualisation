using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputManager
{
    private const float minDeadzone = 0.01f;
    private const float maxDeadzone = 0.99f;
    private const float triggerPressThreshold = 0.95f;
    private const float triggerReleaseThreshold = 0.7f;

    public enum ControlScheme
    {
        MouseAndKeyboard,
        Controller
    }

    // PRIVATE
    private static InputMaster inputMaster = null;

    private static ControlScheme currentControlScheme = ControlScheme.MouseAndKeyboard;
    private static Gamepad currentGamepad = null;

    private static bool phaseButtonPressed = false;
    private static bool fireButtonPressed = false;
    private static bool swapWeaponsButtonPressed = false;

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

        // Stop rumble if we're switching from controller to another control scheme 
        if (inputAction.activeControl.device is Gamepad == false)
        {
            if (currentControlScheme == ControlScheme.Controller && currentGamepad != null)
            {
                currentGamepad.ResetHaptics();
            }
        }

        switch (inputAction.activeControl.device)
        {
            case Keyboard or Mouse:
                currentControlScheme = ControlScheme.MouseAndKeyboard;
                break;

            case Gamepad:
                Gamepad thisGamepad = inputAction.activeControl.device as Gamepad;

                if (thisGamepad != currentGamepad)
                {
                    // Ensure the previous controller doesn't keep rumbling if we're not using it anymore
                    if (currentGamepad != null) currentGamepad.ResetHaptics();
                    currentGamepad = thisGamepad;
                }

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

    private static bool ButtonPoll(InputAction inputAction, ref bool previousValue)
    {
        float pressValue = inputAction.ReadValue<float>();
        if (pressValue >= triggerPressThreshold)
        {
            UpdateControlScheme(inputAction);
            previousValue = true;
        }
        else if (pressValue <= triggerReleaseThreshold)
        {
            previousValue = false;
        }

        return previousValue;
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
    /// Set the rumble strength of the motors in the current controller
    /// </summary>
    /// <param name="lowFrequencyStrength">The strength of the low frequency motor's rumble</param>
    /// <param name="highFrequencyStrength">The strength of the high frequency motor's rumble</param>
    public static void SetControllerRumble(float lowFrequencyStrength, float highFrequencyStrength)
    {
        if (currentControlScheme == ControlScheme.Controller)
        {
            if (currentGamepad != null)
            {
                lowFrequencyStrength = Mathf.Clamp01(lowFrequencyStrength);
                highFrequencyStrength = Mathf.Clamp01(highFrequencyStrength);

                // Using controller, with the most recently used available
                currentGamepad.SetMotorSpeeds(lowFrequencyStrength, highFrequencyStrength);
            }
        }
    }

    /// <summary>
    /// Stop the current controller from rumbling
    /// </summary>
    public static void ResetControllerRumble()
    {
        // Reset regardless of current control scheme
        if (currentGamepad != null)
        {
            currentGamepad.ResetHaptics();
        }
    }

    /// <summary>
    /// Calculate the current movement input direction
    /// </summary>
    /// <param name="normalized">Whether the returned vector should be normalized</param>
    /// <returns>The movement input direction, clamped between 0 and 1</returns>
    public static Vector2 GetMovementDirection(bool normalized)
    {
        InputAction action = Instance.Gameplay.Movement;
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
            if (DeadzoneRemap(Instance.Gameplay.AimDirection.ReadValue<Vector2>()).sqrMagnitude > 0f)
            {
                // Controller is aiming!
                currentControlScheme = ControlScheme.Controller;
            }
        }

        // Make sure we read from the correct event
        switch (currentControlScheme)
        {
            case ControlScheme.MouseAndKeyboard:
                action = Instance.Gameplay.AimPosition;
                break;

            case ControlScheme.Controller:
                action = Instance.Gameplay.AimDirection;
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
    /// Return the position of the mouse cursor, in the range (-1, 1) on the x and y axes. (NOTE: only valid with mouse and keyboard)
    /// </summary>
    /// <returns>The position of the mouse cursor in the range (-1, 1) on the x and y axes, null if not using mouse and keyboard</returns>
    public static Vector2? GetAimPosition()
    {
        // Only returns a valid value if the player is using mouse and keyboard
        if (currentControlScheme == ControlScheme.MouseAndKeyboard)
        {
            Vector2 aimPositionValue = Instance.Gameplay.AimPosition.ReadValue<Vector2>();
            aimPositionValue.x /= Screen.width;
            aimPositionValue.y /= Screen.height;

            // Remap from range (0, 1) to (-1, 1)
            return (aimPositionValue - (Vector2.one * 0.5f)) * 2f;
        }

        // Not using mouse and keyboard
        return null;
    }

    /// <summary>
    /// Whether the player is holding the phase binding down
    /// </summary>
    /// <returns>The button held state</returns>
    public static bool GetPhaseButton()
    {
        return ButtonPoll(Instance.Gameplay.Phase, ref phaseButtonPressed);
    }

    /// <summary>
    /// Whether the player is holding the fire binding down
    /// </summary>
    /// <returns>The button held state</returns>
    public static bool GetFireButton()
    {
        return ButtonPoll(Instance.Gameplay.Fire, ref fireButtonPressed);
    }

    /// <summary>
    /// Whether the player is holding the swap weapons binding down
    /// </summary>
    /// <returns>The button held state</returns>
    public static bool GetSwapWeaponsButton()
    {
        return ButtonPoll(Instance.Gameplay.SwapWeapons, ref swapWeaponsButtonPressed);
    }

    #endregion
}
