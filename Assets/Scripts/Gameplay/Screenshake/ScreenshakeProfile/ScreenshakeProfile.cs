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

    /// <summary>
    /// Copy the data from this profile to the provided instance
    /// </summary>
    /// <param name="target">The profile which the data will be copied to</param>
    public void CopyData(ref ScreenshakeProfile target)
    {
        // Ensure the target is valid
        if (target == null) target = new ScreenshakeProfile();

        // Copy all variables to the new copy
        target.shakeType = shakeType;

        target.usePosition = usePosition;
        target.useRotation = useRotation;
        target.useZoom = useZoom;

        target.continuousPositionNoiseMagnitude = continuousPositionNoiseMagnitude;
        target.continuousPositionNoiseFrequency = continuousPositionNoiseFrequency;

        target.continuousRotationNoiseMagnitude = continuousRotationNoiseMagnitude;
        target.continuousRotationNoiseFrequency = continuousRotationNoiseFrequency;

        target.continuousZoomNoiseInMagnitude = continuousZoomNoiseInMagnitude;
        target.continuousZoomNoiseOutMagnitude = continuousZoomNoiseOutMagnitude;
        target.continuousZoomNoiseFrequency = continuousZoomNoiseFrequency;

        target.shakeDuration = shakeDuration;

        target.discretePositionStyle = discretePositionStyle;

        target.discretePositionNoiseMagnitude = discretePositionNoiseMagnitude;
        target.discretePositionNoiseFrequency = discretePositionNoiseFrequency;
        target.discretePositionNoiseCurve.CopyFrom(discretePositionNoiseCurve);

        target.discretePositionAnimationMagnitude = discretePositionAnimationMagnitude;
        target.discretePositionAnimationCurve.CopyFrom(discretePositionAnimationCurve);
        target.discretePositionAnimationDirection = discretePositionAnimationDirection;

        target.discreteRotationNoiseMagnitude = discreteRotationNoiseMagnitude;
        target.discreteRotationNoiseFrequency = discreteRotationNoiseFrequency;
        target.discreteRotationNoiseCurve.CopyFrom(discreteRotationNoiseCurve);

        target.discreteZoomStyle = discreteZoomStyle;

        target.discreteZoomNoiseInMagnitude = discreteZoomNoiseInMagnitude;
        target.discreteZoomNoiseOutMagnitude = discreteZoomNoiseOutMagnitude;
        target.discreteZoomNoiseFrequency = discreteZoomNoiseFrequency;
        target.discreteZoomNoiseCurve.CopyFrom(discreteZoomNoiseCurve);

        target.discreteZoomAnimationMagnitude = discreteZoomAnimationMagnitude;
        target.discreteZoomAnimationCurve.CopyFrom(discreteZoomAnimationCurve);
    }
}
