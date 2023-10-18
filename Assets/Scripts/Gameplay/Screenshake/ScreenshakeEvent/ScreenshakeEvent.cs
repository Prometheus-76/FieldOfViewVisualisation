using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeEvent
{
    public ScreenshakeSystem shakeSystem;
    public ScreenshakeHandle eventHandle;

    // PROPERTIES
    public Vector3 positionOffset { get; private set; } = Vector3.zero;
    public Quaternion rotationOffset { get; private set; } = Quaternion.identity;
    public float zoomOffset { get; private set; } = 0f;

    public bool isComplete { get; private set; } = false;

    // PRIVATE
    private float eventLifetimeTimer = 0f;
    private bool isContinuous = false;

    ScreenshakeProfile profileCopy = null;

    public ScreenshakeEvent(ScreenshakeSystem shakeSystem, ScreenshakeProfile shakeProfile)
    {
        this.shakeSystem = shakeSystem;

        // Save a copy of the profile data at the time the event was created
        shakeProfile.CopyData(ref profileCopy);

        positionOffset = Vector3.zero;
        rotationOffset = Quaternion.identity;
        zoomOffset = 0f;

        isComplete = false;

        eventLifetimeTimer = 0f;
        isContinuous = (profileCopy.shakeType == ScreenshakeSystem.ShakeType.Continuous);

        // Create and assign the handle (this should be done LAST so that the event is fully setup when passed in)
        eventHandle = new ScreenshakeHandle(this);
    }

    public void UpdateEvent(float deltaTime)
    {
        eventLifetimeTimer += deltaTime;

        // Discrete events should be clamped to not exceed their maximum duration
        if (isContinuous == false)
        {
            eventLifetimeTimer = Mathf.Clamp(eventLifetimeTimer, 0f, profileCopy.shakeDuration);
        }

        CalculatePositionOffset();

        CalculateRotationOffset();

        CalculateZoomOffset();
    }

    private void CalculatePositionOffset()
    {

    }

    private void CalculateRotationOffset()
    {

    }

    private void CalculateZoomOffset()
    {

    }

    public void RemoveEvent() => shakeSystem.RemoveEvent(this);
}
