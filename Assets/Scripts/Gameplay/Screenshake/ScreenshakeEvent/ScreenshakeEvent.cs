using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScreenshakeEvent
{
    // PROPERTIES
    public ScreenshakeHandle eventHandle { get; private set; } = null;

    public Vector2 positionOffset { get; private set; } = Vector2.zero;
    public float rotationOffset { get; private set; } = 0f;
    public float zoomOffset { get; private set; } = 0f;

    public bool isComplete { get; private set; } = false;

    // PRIVATE
    private float eventLifetimeTimer = 0f;

    private Vector2 continuousPositionTime = Vector2.zero;
    private float continuousRotationTime = 0f;
    private float continuousZoomTime = 0f;

    private bool isDiscrete = false;
    private float discreteCompletion01 = 0f;

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
        // Seed is within range (0, 1000)
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

    public void UpdateEventTimers(float deltaTime)
    {
        eventLifetimeTimer += deltaTime;

        if (isDiscrete)
        {
            if (eventLifetimeTimer >= profileCopy.shakeDuration)
            {
                // Discrete event has expired, still clamp timer just in case
                eventLifetimeTimer = profileCopy.shakeDuration;
                discreteCompletion01 = 1f;

                isComplete = true;
            }
            else
            {
                // Update completion percent
                discreteCompletion01 = Mathf.Clamp01(eventLifetimeTimer / profileCopy.shakeDuration);
            }
        }
        else
        {
            // Progress separate timers
            if (profileCopy.usePosition) continuousPositionTime += deltaTime * eventHandle.continuousPositionFrequencyMultiplier;
            if (profileCopy.useRotation) continuousRotationTime += deltaTime * eventHandle.continuousRotationFrequencyMultiplier;
            if (profileCopy.useZoom) continuousZoomTime += deltaTime * eventHandle.continuousZoomFrequencyMultiplier;
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

    private float EvaluateFixedCurve(Curves.CurveStyle inCurve, float midpointMin, float midpointMax, Curves.CurveStyle outCurve, float t)
    {
        t = Mathf.Clamp01(t);

        if (t < midpointMin)
        {
            // In Curve
            float inT = Mathf.Clamp01(t / midpointMin);
            return Curves.Evaluate(inCurve, inT);
        }
        else if (t > midpointMax)
        {
            // Out Curve
            float outT = Mathf.Clamp01((t - midpointMax) / (1f - midpointMax));
            return Curves.Evaluate(outCurve, 1f - outT);
        }
        else
        {
            // Midpoint
            return 1f;
        }
    }

    private float NoiseRemap(float noiseValue)
    {
        // From (0, 1) to (-1, 1)
        return (noiseValue * 2f) - 1f;
    }
}
