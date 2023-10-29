using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public float lookAheadDistance;

    private void CameraUpdate(float deltaTime)
    {
        // Make the camera look ahead of the player's aim direction
        if (InputManager.GetControlScheme() == InputManager.ControlScheme.MouseAndKeyboard)
        {
            cameraController.SetPositionOffset(fireInputPosition.Value * lookAheadDistance);
        }
        else
        {
            cameraController.SetPositionOffset(fireInputDirection * lookAheadDistance);
        }
    }
}
