using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeEvent
{
    // PROPERTIES
    public ScreenshakeHandle eventHandle { get; private set; } = null;

    public Vector2 positionOffset { get; private set; } = Vector2.zero;
    public float rotationOffset { get; private set; } = 0f;
    public float zoomOffset { get; private set; } = 0f;

    public bool isComplete { get; private set; } = false;

    // PRIVATE
    private float eventLifetimeTimer = 0f;
    private bool isDiscrete = false;
    private float eventSeed = 0f;

    ScreenshakeProfile profileCopy = null;

    /// <summary>
    /// Basic constructor, sets the event seed and ensures data is set to a reset state
    /// </summary>
    /// <param name="eventIndex">Acts as a unique ID for this event</param>
    public ScreenshakeEvent(int eventIndex)
    {
        // Event seed remains the same regardless of reuse
        // Generate with randomness within distinct steps to reduce seed collisions
        eventSeed = ((eventIndex % 100) * 10f) + Random.Range(0f, 10f);

        ResetEvent();
    }

    #region Public Methods
    
    public void MarkAsComplete() => isComplete = true;

    public void ResetEvent()
    {
        eventHandle = null;

        positionOffset = Vector2.zero;
        rotationOffset = 0f;
        zoomOffset = 0f;

        isComplete = false;

        eventLifetimeTimer = 0f;
    }

    public ScreenshakeHandle ConfigureEventAndHandle(ScreenshakeProfile shakeProfile)
    {
        // Save a copy of the profile data
        shakeProfile.CopyRequiredData(ref profileCopy);

        // Cache profile data
        isDiscrete = (profileCopy.shakeType == ScreenshakeSystem.ShakeType.Discrete);

        // Create and return handle
        eventHandle = new ScreenshakeHandle(this);
        return eventHandle;
    }

    public void UpdateEventLifetime(float deltaTime)
    {
        eventLifetimeTimer += deltaTime;

        // Discrete event has expired, still clamp timer just in case
        if (isDiscrete && eventLifetimeTimer >= profileCopy.shakeDuration)
        {
            eventLifetimeTimer = profileCopy.shakeDuration;
            isComplete = true;
        }
    }

    public void CalculateShakeOffsets()
    {
        // Don't update if completed
        if (isComplete) return;

        // Update all offsets, if enabled
        if (profileCopy.usePosition) CalculatePositionOffset();
        if (profileCopy.useRotation) CalculateRotationOffset();
        if (profileCopy.useZoom) CalculateZoomOffset();
    }

    #endregion

    private void CalculatePositionOffset()
    {

    }

    private void CalculateRotationOffset()
    {
        
    }

    private void CalculateZoomOffset()
    {

    }
}
