using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "Screenshake_", menuName = "ScriptableObject/ScreenshakeProfile")]
public class ScreenshakeProfile : ScriptableObject
{
    // BASIC CONFIGURATION
    // ------
    public ScreenshakeSystem.ShakeType shakeType = ScreenshakeSystem.ShakeType.Discrete;

    public bool usePosition = true;
    public bool useRotation = true;
    public bool useZoom = true;


    // DISCRETE (controlled internally by the ScreenshakeSystem)
    // --------
    public float shakeDuration = 0.5f;

    // Position
    public ScreenshakeSystem.DiscreteType discretePositionStyle = ScreenshakeSystem.DiscreteType.Noise;
    // --Noise
    public Vector2 discretePositionNoiseMagnitude = Vector2.one;
    public Vector2 discretePositionNoiseFrequency = Vector2.one;
    public bool discretePositionNoiseUseCustomCurve = false;
    // -->>Custom
    public AnimationCurve discretePositionNoiseCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    // -->>Fixed
    public Curves.CurveStyle discretePositionNoiseFixedCurveIn = Curves.CurveStyle.Linear;
    public float discretePositionNoiseFixedCurveMidpointMin = 0.45f;
    public float discretePositionNoiseFixedCurveMidpointMax = 0.55f;
    public Curves.CurveStyle discretePositionNoiseFixedCurveOut = Curves.CurveStyle.Linear;
    // --Animation
    public float discretePositionAnimationMagnitude = 0.5f;
    public Vector2 discretePositionAnimationDirection = Vector2.up;
    public bool discretePositionAnimationUseCustomCurve = false;
    // -->>Custom
    public AnimationCurve discretePositionAnimationCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    // -->>Fixed
    public Curves.CurveStyle discretePositionAnimationFixedCurveIn = Curves.CurveStyle.Linear;
    public float discretePositionAnimationFixedCurveMidpointMin = 0.45f;
    public float discretePositionAnimationFixedCurveMidpointMax = 0.55f;
    public Curves.CurveStyle discretePositionAnimationFixedCurveOut = Curves.CurveStyle.Linear;

    // Rotation
    // --Noise
    public float discreteRotationNoiseMagnitude = 5f;
    public float discreteRotationNoiseFrequency = 2f;
    public bool discreteRotationNoiseUseCustomCurve = false;
    // -->>Custom
    public AnimationCurve discreteRotationNoiseCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    // -->>Fixed
    public Curves.CurveStyle discreteRotationNoiseFixedCurveIn = Curves.CurveStyle.Linear;
    public float discreteRotationNoiseFixedCurveMidpointMin = 0.45f;
    public float discreteRotationNoiseFixedCurveMidpointMax = 0.55f;
    public Curves.CurveStyle discreteRotationNoiseFixedCurveOut = Curves.CurveStyle.Linear;

    // Zoom
    public ScreenshakeSystem.DiscreteType discreteZoomStyle = ScreenshakeSystem.DiscreteType.Noise;
    // --Noise
    public float discreteZoomNoiseInMagnitude = 0.5f;
    public float discreteZoomNoiseOutMagnitude = 0.5f;
    public float discreteZoomNoiseFrequency = 2f;
    public bool discreteZoomNoiseUseCustomCurve = false;
    // -->>Custom
    public AnimationCurve discreteZoomNoiseCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    // -->>Fixed
    public Curves.CurveStyle discreteZoomNoiseFixedCurveIn = Curves.CurveStyle.Linear;
    public float discreteZoomNoiseFixedCurveMidpointMin = 0.45f;
    public float discreteZoomNoiseFixedCurveMidpointMax = 0.55f;
    public Curves.CurveStyle discreteZoomNoiseFixedCurveOut = Curves.CurveStyle.Linear;
    // --Animation
    public float discreteZoomAnimationMagnitude = 1f;
    public ScreenshakeSystem.ZoomDirection discreteZoomAnimationDirection = ScreenshakeSystem.ZoomDirection.ZoomIn;
    public bool discreteZoomAnimationUseCustomCurve = false;
    // -->>Custom
    public AnimationCurve discreteZoomAnimationCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    // -->>Fixed
    public Curves.CurveStyle discreteZoomAnimationFixedCurveIn = Curves.CurveStyle.Linear;
    public float discreteZoomAnimationFixedCurveMidpointMin = 0.45f;
    public float discreteZoomAnimationFixedCurveMidpointMax = 0.55f;
    public Curves.CurveStyle discreteZoomAnimationFixedCurveOut = Curves.CurveStyle.Linear;

    
    // CONTINUOUS (controlled live by an external script using a handle)
    // ----------

    // Position
    // --Noise
    public Vector2 continuousPositionNoiseMagnitude = Vector2.one;
    public Vector2 continuousPositionNoiseFrequency = Vector2.one;

    // Rotation
    // --Noise
    public float continuousRotationNoiseMagnitude = 5f;
    public float continuousRotationNoiseFrequency = 2f;

    // Zoom
    // --Noise
    public float continuousZoomNoiseInMagnitude = 0.5f;
    public float continuousZoomNoiseOutMagnitude = 0.5f;
    public float continuousZoomNoiseFrequency = 2f;

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

        if (target.shakeType == ScreenshakeSystem.ShakeType.Discrete)
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
                        target.discretePositionNoiseFixedCurveMidpointMin = discretePositionNoiseFixedCurveMidpointMin;
                        target.discretePositionNoiseFixedCurveMidpointMax = discretePositionNoiseFixedCurveMidpointMax;
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
                        target.discretePositionAnimationFixedCurveMidpointMin = discretePositionAnimationFixedCurveMidpointMin;
                        target.discretePositionAnimationFixedCurveMidpointMax = discretePositionAnimationFixedCurveMidpointMax;
                        target.discretePositionAnimationFixedCurveOut = discretePositionAnimationFixedCurveOut;
                    }

                    if (discretePositionAnimationDirection.sqrMagnitude > 0f)
                    {
                        target.discretePositionAnimationDirection = discretePositionAnimationDirection.normalized;
                    }
                    else
                    {
                        // Randomise direction when supplied vector is (0, 0)
                        float angleRadians = Random.Range(0f, Mathf.PI * 2f);
                        target.discretePositionAnimationDirection.x = Mathf.Cos(angleRadians);
                        target.discretePositionAnimationDirection.y = Mathf.Sin(angleRadians);
                    }
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
                    target.discreteRotationNoiseFixedCurveMidpointMin = discreteRotationNoiseFixedCurveMidpointMin;
                    target.discreteRotationNoiseFixedCurveMidpointMax = discreteRotationNoiseFixedCurveMidpointMax;
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
                        target.discreteZoomNoiseFixedCurveMidpointMin = discreteZoomNoiseFixedCurveMidpointMin;
                        target.discreteZoomNoiseFixedCurveMidpointMax = discreteZoomNoiseFixedCurveMidpointMax;
                        target.discreteZoomNoiseFixedCurveOut = discreteZoomNoiseFixedCurveOut;
                    }
                }
                else if (target.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Animation)
                {
                    // Discrete zoom animation
                    target.discreteZoomAnimationMagnitude = discreteZoomAnimationMagnitude;
                    target.discreteZoomAnimationDirection = discreteZoomAnimationDirection;
                    target.discreteZoomAnimationUseCustomCurve = discreteZoomAnimationUseCustomCurve;

                    if (target.discreteZoomAnimationUseCustomCurve)
                    {
                        // Discrete zoom animation with custom curve
                        if (target.discreteZoomAnimationCustomCurve == null) target.discreteZoomAnimationCustomCurve = new AnimationCurve();
                        target.discreteZoomAnimationCustomCurve.CopyFrom(discreteZoomAnimationCustomCurve);
                    }
                    else
                    {
                        // Discrete zoom animation with fixed curve
                        target.discreteZoomAnimationFixedCurveIn = discreteZoomAnimationFixedCurveIn;
                        target.discreteZoomAnimationFixedCurveMidpointMin = discreteZoomAnimationFixedCurveMidpointMin;
                        target.discreteZoomAnimationFixedCurveMidpointMax = discreteZoomAnimationFixedCurveMidpointMax;
                        target.discreteZoomAnimationFixedCurveOut = discreteZoomAnimationFixedCurveOut;
                    }
                }
            }
        }
        else if (target.shakeType == ScreenshakeSystem.ShakeType.Continuous)
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
    }
}