using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeHandle
{
    // PROPERTIES
    public bool isEventAttached { get { return screenshakeEvent == null; } }

    // ---------
    // Position
    // ---------
    private Vector2 _continuousPositionMagnitudeMultiplier = Vector2.one;
    public Vector2 continuousPositionMagnitudeMultiplier
    {
        get => _continuousPositionMagnitudeMultiplier; 
        set
        {
            _continuousPositionMagnitudeMultiplier.x = Mathf.Max(0f, value.x);
            _continuousPositionMagnitudeMultiplier.y = Mathf.Max(0f, value.y);
        }
    }

    private Vector2 _continuousPositionFrequencyMultiplier = Vector2.one;
    public Vector2 continuousPositionFrequencyMultiplier
    {
        get => _continuousPositionFrequencyMultiplier; 
        set
        {
            _continuousPositionFrequencyMultiplier.x = Mathf.Max(0f, value.x);
            _continuousPositionFrequencyMultiplier.y = Mathf.Max(0f, value.y);
        }
    }

    // ---------
    // Rotation
    // ---------
    private float _continuousRotationMagnitudeMultiplier = 1f;
    public float continuousRotationMagnitudeMultiplier 
    { 
        get => _continuousRotationMagnitudeMultiplier; 
        set
        {
            _continuousRotationMagnitudeMultiplier = Mathf.Max(0f, value);
        }
    }

    private float _continuousRotationFrequencyMultiplier = 1f;
    public float continuousRotationFrequencyMultiplier
    {
        get => _continuousRotationFrequencyMultiplier;
        set
        {
            _continuousRotationFrequencyMultiplier = Mathf.Max(0f, value);
        }
    }

    // -----
    // Zoom
    // -----
    private float _continuousZoomInMagnitudeMultiplier = 1f;
    public float continuousZoomInMagnitudeMultiplier
    {
        get => _continuousZoomInMagnitudeMultiplier;
        set
        {
            _continuousZoomInMagnitudeMultiplier = Mathf.Max(0f, value);
        }
    }

    private float _continuousZoomOutMagnitudeMultiplier = 1f;
    public float continuousZoomOutMagnitudeMultiplier
    {
        get => _continuousZoomOutMagnitudeMultiplier;
        set
        {
            _continuousZoomOutMagnitudeMultiplier = Mathf.Max(0f, value);
        }
    }

    private float _continuousZoomFrequencyMultiplier = 1f;
    public float continuousZoomFrequencyMultiplier
    {
        get => _continuousZoomFrequencyMultiplier;
        set
        {
            _continuousZoomFrequencyMultiplier = Mathf.Max(0f, value);
        }
    }

    // PRIVATE
    private ScreenshakeEvent screenshakeEvent = null;

    public ScreenshakeHandle(ScreenshakeEvent instantiatingEvent)
    {
        screenshakeEvent = instantiatingEvent;
    }

    public void RemoveEvent()
    {
        if (screenshakeEvent != null) screenshakeEvent.MarkAsComplete();
        screenshakeEvent = null;
    }
}