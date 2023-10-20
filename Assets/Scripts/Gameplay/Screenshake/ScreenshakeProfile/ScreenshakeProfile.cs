using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    public bool discretePositionNoiseUseCustomCurve;
    // -->>Custom
    public AnimationCurve discretePositionNoiseCustomCurve;
    // -->>Fixed
    public Curves.CurveStyle discretePositionNoiseFixedCurveIn;
    public float discretePositionNoiseFixedCurveMidpointIn;
    public float discretePositionNoiseFixedCurveMidpointOut;
    public Curves.CurveStyle discretePositionNoiseFixedCurveOut;
    // --Animation
    public float discretePositionAnimationMagnitude;
    public Vector2 discretePositionAnimationDirection;
    public bool discretePositionAnimationUseCustomCurve;
    // -->>Custom
    public AnimationCurve discretePositionAnimationCustomCurve;
    // -->>Fixed
    public Curves.CurveStyle discretePositionAnimationFixedCurveIn;
    public float discretePositionAnimationFixedCurveMidpointIn;
    public float discretePositionAnimationFixedCurveMidpointOut;
    public Curves.CurveStyle discretePositionAnimationFixedCurveOut;

    // Rotation
    // --Noise
    public float discreteRotationNoiseMagnitude;
    public float discreteRotationNoiseFrequency;
    public bool discreteRotationNoiseUseCustomCurve;
    // -->>Custom
    public AnimationCurve discreteRotationNoiseCustomCurve;
    // -->>Fixed
    public Curves.CurveStyle discreteRotationNoiseFixedCurveIn;
    public float discreteRotationNoiseFixedCurveMidpointIn;
    public float discreteRotationNoiseFixedCurveMidpointOut;
    public Curves.CurveStyle discreteRotationNoiseFixedCurveOut;

    // Zoom
    public ScreenshakeSystem.DiscreteType discreteZoomStyle;
    // --Noise
    public float discreteZoomNoiseInMagnitude;
    public float discreteZoomNoiseOutMagnitude;
    public float discreteZoomNoiseFrequency;
    public bool discreteZoomNoiseUseCustomCurve;
    // -->>Custom
    public AnimationCurve discreteZoomNoiseCustomCurve;
    // -->>Fixed
    public Curves.CurveStyle discreteZoomNoiseFixedCurveIn;
    public float discreteZoomNoiseFixedCurveMidpointIn;
    public float discreteZoomNoiseFixedCurveMidpointOut;
    public Curves.CurveStyle discreteZoomNoiseFixedCurveOut;
    // --Animation
    public float discreteZoomAnimationMagnitude;
    public bool discreteZoomAnimationUseCustomCurve;
    // -->>Custom
    public AnimationCurve discreteZoomAnimationCustomCurve;
    // -->>Fixed
    public Curves.CurveStyle discreteZoomAnimationFixedCurveIn;
    public float discreteZoomAnimationFixedCurveMidpointIn;
    public float discreteZoomAnimationFixedCurveMidpointOut;
    public Curves.CurveStyle discreteZoomAnimationFixedCurveOut;

    /// <summary>
    /// Copy the data which is needed from this profile to the provided instance.
    /// NOTE: Only the required data is copied, for example no continuous variables are copied if the profile is set to the discrete type
    /// </summary>
    /// <param name="target">The profile which the data will be copied to</param>
    public void CopyRequiredData(ref ScreenshakeProfile target)
    {
        // Ensure the target is valid
        if (target == null) target = CreateInstance<ScreenshakeProfile>();

        // Basic configuration
        target.shakeType = shakeType;

        target.usePosition = usePosition;
        target.useRotation = useRotation;
        target.useZoom = useZoom;

        if (target.shakeType == ScreenshakeSystem.ShakeType.Continuous)
        {
            if (target.usePosition)
            {
                // Continuous position
                target.continuousPositionNoiseMagnitude = continuousPositionNoiseMagnitude;
                target.continuousPositionNoiseFrequency = continuousPositionNoiseFrequency;
            }

            if (target.useRotation)
            {
                // Continuous rotation
                target.continuousRotationNoiseMagnitude = continuousRotationNoiseMagnitude;
                target.continuousRotationNoiseFrequency = continuousRotationNoiseFrequency;
            }

            if (target.useZoom)
            {
                // Continuous zoom
                target.continuousZoomNoiseInMagnitude = continuousZoomNoiseInMagnitude;
                target.continuousZoomNoiseOutMagnitude = continuousZoomNoiseOutMagnitude;
                target.continuousZoomNoiseFrequency = continuousZoomNoiseFrequency;
            }
        }
        else if (target.shakeType == ScreenshakeSystem.ShakeType.Discrete)
        {
            // Discrete
            target.shakeDuration = shakeDuration;

            if (target.usePosition)
            {
                // Discrete position
                target.discretePositionStyle = discretePositionStyle;

                if (target.discretePositionStyle == ScreenshakeSystem.DiscreteType.Noise)
                {
                    // Discrete position noise
                    target.discretePositionNoiseMagnitude = discretePositionNoiseMagnitude;
                    target.discretePositionNoiseFrequency = discretePositionNoiseFrequency;
                    target.discretePositionNoiseUseCustomCurve = discretePositionNoiseUseCustomCurve;

                    if (target.discretePositionNoiseUseCustomCurve)
                    {
                        // Discrete position noise with custom curve
                        if (target.discretePositionNoiseCustomCurve == null) target.discretePositionNoiseCustomCurve = new AnimationCurve();
                        target.discretePositionNoiseCustomCurve.CopyFrom(discretePositionNoiseCustomCurve);
                    }
                    else
                    {
                        // Discrete position noise with fixed curve
                        target.discretePositionNoiseFixedCurveIn = discretePositionNoiseFixedCurveIn;
                        target.discretePositionNoiseFixedCurveMidpointIn = discretePositionNoiseFixedCurveMidpointIn;
                        target.discretePositionNoiseFixedCurveMidpointOut = discretePositionNoiseFixedCurveMidpointOut;
                        target.discretePositionNoiseFixedCurveOut = discretePositionNoiseFixedCurveOut;
                    }
                }
                else if (target.discretePositionStyle == ScreenshakeSystem.DiscreteType.Animation)
                {
                    // Discrete position animation
                    target.discretePositionAnimationMagnitude = discretePositionAnimationMagnitude;
                    target.discretePositionAnimationUseCustomCurve = discretePositionAnimationUseCustomCurve;

                    if (target.discretePositionAnimationUseCustomCurve)
                    {
                        // Discrete position animation with custom curve
                        if (target.discretePositionAnimationCustomCurve == null) target.discretePositionAnimationCustomCurve = new AnimationCurve();
                        target.discretePositionAnimationCustomCurve.CopyFrom(discretePositionAnimationCustomCurve);
                    }
                    else
                    {
                        // Discrete position animation with fixed curve
                        target.discretePositionAnimationFixedCurveIn = discretePositionAnimationFixedCurveIn;
                        target.discretePositionAnimationFixedCurveMidpointIn = discretePositionAnimationFixedCurveMidpointIn;
                        target.discretePositionAnimationFixedCurveMidpointOut = discretePositionAnimationFixedCurveMidpointOut;
                        target.discretePositionAnimationFixedCurveOut = discretePositionAnimationFixedCurveOut;
                    }

                    target.discretePositionAnimationDirection = discretePositionAnimationDirection;
                }
            }

            if (target.useRotation)
            {
                // Discrete rotation
                target.discreteRotationNoiseMagnitude = discreteRotationNoiseMagnitude;
                target.discreteRotationNoiseFrequency = discreteRotationNoiseFrequency;
                target.discreteRotationNoiseUseCustomCurve = discreteRotationNoiseUseCustomCurve;

                if (target.discreteRotationNoiseUseCustomCurve)
                {
                    // Discrete rotation with custom curve
                    if (target.discreteRotationNoiseCustomCurve == null) target.discreteRotationNoiseCustomCurve = new AnimationCurve();
                    target.discreteRotationNoiseCustomCurve.CopyFrom(discreteRotationNoiseCustomCurve);
                }
                else
                {
                    // Discrete rotation with fixed curve
                    target.discreteRotationNoiseFixedCurveIn = discreteRotationNoiseFixedCurveIn;
                    target.discreteRotationNoiseFixedCurveMidpointIn = discreteRotationNoiseFixedCurveMidpointIn;
                    target.discreteRotationNoiseFixedCurveMidpointOut = discreteRotationNoiseFixedCurveMidpointOut;
                    target.discreteRotationNoiseFixedCurveOut = discreteRotationNoiseFixedCurveOut;
                }
            }

            if (target.useZoom)
            {
                // Discrete zoom
                target.discreteZoomStyle = discreteZoomStyle;

                if (target.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Noise)
                {
                    // Discrete zoom noise
                    target.discreteZoomNoiseInMagnitude = discreteZoomNoiseInMagnitude;
                    target.discreteZoomNoiseOutMagnitude = discreteZoomNoiseOutMagnitude;
                    target.discreteZoomNoiseFrequency = discreteZoomNoiseFrequency;
                    target.discreteZoomNoiseUseCustomCurve = discreteZoomNoiseUseCustomCurve;

                    if (target.discreteZoomNoiseUseCustomCurve)
                    {
                        // Discrete zoom noise with custom curve
                        if (target.discreteZoomNoiseCustomCurve == null) target.discreteZoomNoiseCustomCurve = new AnimationCurve();
                        target.discreteZoomNoiseCustomCurve.CopyFrom(discreteZoomNoiseCustomCurve);
                    }
                    else
                    {
                        // Discrete zoom noise with fixed curve
                        target.discreteZoomNoiseFixedCurveIn = discreteZoomNoiseFixedCurveIn;
                        target.discreteZoomNoiseFixedCurveMidpointIn = discreteZoomNoiseFixedCurveMidpointIn;
                        target.discreteZoomNoiseFixedCurveMidpointOut = discreteZoomNoiseFixedCurveMidpointOut;
                        target.discreteZoomNoiseFixedCurveOut = discreteZoomNoiseFixedCurveOut;
                    }
                }
                else if (target.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Animation)
                {
                    // Discrete zoom animation
                    target.discreteZoomAnimationMagnitude = discreteZoomAnimationMagnitude;
                    target.discreteZoomAnimationUseCustomCurve = discreteZoomNoiseUseCustomCurve;

                    if (target.discreteZoomNoiseUseCustomCurve)
                    {
                        // Discrete zoom animation with custom curve
                        if (target.discreteZoomAnimationCustomCurve == null) target.discreteZoomAnimationCustomCurve = new AnimationCurve();
                        target.discreteZoomAnimationCustomCurve.CopyFrom(discreteZoomAnimationCustomCurve);
                    }
                    else
                    {
                        // Discrete zoom animation with fixed curve
                        target.discreteZoomAnimationFixedCurveIn = discreteZoomAnimationFixedCurveIn;
                        target.discreteZoomAnimationFixedCurveMidpointIn = discreteZoomAnimationFixedCurveMidpointIn;
                        target.discreteZoomAnimationFixedCurveMidpointOut = discreteZoomAnimationFixedCurveMidpointOut;
                        target.discreteZoomAnimationFixedCurveOut = discreteZoomAnimationFixedCurveOut;
                    }
                }
            }
        }
    }
}
