using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Screenshake_", menuName = "ScriptableObject/ScreenshakeProfile")]
public class ScreenshakeProfile : ScriptableObject
{
    // BASIC CONFIGURATION
    // ------
    public ScreenshakeSystem.ShakeType shakeType;
    public bool usePosition;
    public bool useRotation;
    public bool useZoom;


    // CONTINUOUS (controlled live by an external script using a handle)
    // ----------

    // Position
    // --Noise
    public Vector2 continuousPositionNoiseMagnitude;
    public Vector2 continuousPositionNoiseFrequency;

    // Rotation
    // --Noise
    public float continuousRotationNoiseMagnitude;
    public float continuousRotationNoiseFrequency;

    // Zoom
    // --Noise
    public float continuousZoomNoiseInMagnitude;
    public float continuousZoomNoiseOutMagnitude;
    public float continuousZoomNoiseFrequency;


    // DISCRETE (controlled internally by the ScreenshakeSystem)
    // --------
    public float shakeDuration;

    // Position
    public ScreenshakeSystem.DiscreteType discretePositionStyle;
    // --Noise
    public Vector2 discretePositionNoiseMagnitude;
    public Vector2 discretePositionNoiseFrequency;
    public AnimationCurve discretePositionNoiseCurve;
    // --Animation
    public float discretePositionAnimationMagnitude;
    public AnimationCurve discretePositionAnimationCurve;
    public Vector2 discretePositionAnimationDirection;

    // Rotation
    // --Noise
    public float discreteRotationNoiseMagnitude;
    public float discreteRotationNoiseFrequency;
    public AnimationCurve discreteRotationNoiseCurve;

    // Zoom
    public ScreenshakeSystem.DiscreteType discreteZoomStyle;
    // --Noise
    public float discreteZoomNoiseInMagnitude;
    public float discreteZoomNoiseOutMagnitude;
    public float discreteZoomNoiseFrequency;
    public AnimationCurve discreteZoomNoiseCurve;
    // --Animation
    public float discreteZoomAnimationMagnitude;
    public AnimationCurve discreteZoomAnimationCurve;
}
