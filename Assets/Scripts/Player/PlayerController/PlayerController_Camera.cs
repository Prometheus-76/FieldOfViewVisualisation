using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [Min(0f)]
    public float lookAheadDistance;

    private void CameraUpdate()
    {
        // Make the camera look ahead of the player's aim direction
        if (InputManager.GetControlScheme() == InputManager.ControlScheme.MouseAndKeyboard)
        {
            // Vector from player to mouse cursor
            Vector2 playerScreenPosition = CalculatePlayerPositionOnScreen();
            cameraController.SetPositionOffset((fireInputPosition.Value - playerScreenPosition) * lookAheadDistance);
        }
        else
        {
            cameraController.SetPositionOffset(fireInputDirection * lookAheadDistance);
        }
    }

    private Vector2 CalculatePlayerPositionOnScreen()
    {
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(playerTransform.position);
        screenPosition.x /= Screen.width;
        screenPosition.y /= Screen.height;

        // Remap from range (0, 1) to (-1, 1)
        return (screenPosition - (Vector2.one * 0.5f)) * 2f;
    }
}
