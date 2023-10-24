using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScreenshakeEvent
{
    private void CalculateZoomOffset() => zoomOffset = isDiscrete ? CalculateDiscreteZoomOffset() : CalculateContinuousZoomOffset();

    private float CalculateDiscreteZoomOffset()
    {
        // Is this noise or an animation?
        if (profileCopy.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Noise)
        {
            return CalculateDiscreteZoomNoiseOffset();
        }
        else if (profileCopy.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Animation)
        {
            return CalculateDiscreteZoomAnimationOffset();
        }

        return 0f;
    }

    private float CalculateDiscreteZoomNoiseOffset()
    {
        // Find intensity multiplier from curve
        float intensityMultiplier;
        if (profileCopy.discreteZoomNoiseUseCustomCurve)
        {
            intensityMultiplier = profileCopy.discreteZoomNoiseCustomCurve.Evaluate(discreteCompletion01);
        }
        else
        {
            // Shortened for readability
            Curves.CurveStyle curveIn = profileCopy.discreteZoomNoiseFixedCurveIn;
            float midpointMin = profileCopy.discreteZoomNoiseFixedCurveMidpointMin;
            float midpointMax = profileCopy.discreteZoomNoiseFixedCurveMidpointMax;
            Curves.CurveStyle curveOut = profileCopy.discreteZoomNoiseFixedCurveOut;

            intensityMultiplier = EvaluateFixedCurve(curveIn, midpointMin, midpointMax, curveOut, discreteCompletion01);
        }

        // Early out, result will be 0
        if (intensityMultiplier <= 0f) return 0f;

        intensityMultiplier *= intensityMultiplier;

        float inMagnitudeMultiplier = profileCopy.discreteZoomNoiseInMagnitude * intensityMultiplier;
        float outMagnitudeMultiplier = profileCopy.discreteZoomNoiseOutMagnitude * intensityMultiplier;

        float magnitudeRange = inMagnitudeMultiplier + outMagnitudeMultiplier;
        float magnitudeOffset = (outMagnitudeMultiplier - inMagnitudeMultiplier) / 2f;

        // Noise sample
        float noiseSample = NoiseRemap(Mathf.PerlinNoise(eventLifetimeTimer * profileCopy.discreteZoomNoiseFrequency, eventSeed));
        return (noiseSample * magnitudeRange) + magnitudeOffset;
    }

    private float CalculateDiscreteZoomAnimationOffset()
    {
        // Find intensity multiplier from curve
        float intensityMultiplier;
        if (profileCopy.discreteZoomAnimationUseCustomCurve)
        {
            intensityMultiplier = profileCopy.discreteZoomAnimationCustomCurve.Evaluate(discreteCompletion01);
        }
        else
        {
            // Shortened for readability
            Curves.CurveStyle curveIn = profileCopy.discreteZoomAnimationFixedCurveIn;
            float midpointMin = profileCopy.discreteZoomAnimationFixedCurveMidpointMin;
            float midpointMax = profileCopy.discreteZoomAnimationFixedCurveMidpointMax;
            Curves.CurveStyle curveOut = profileCopy.discreteZoomAnimationFixedCurveOut;

            intensityMultiplier = EvaluateFixedCurve(curveIn, midpointMin, midpointMax, curveOut, discreteCompletion01);
        }

        // Maximum extent of the animation, scaled by the intensity curve
        float maxMagnitude = profileCopy.discreteZoomAnimationMagnitude * ((profileCopy.discreteZoomAnimationDirection == ScreenshakeSystem.ZoomDirection.ZoomIn) ? -1f : 1f);
        return maxMagnitude * intensityMultiplier;
    }

    private float CalculateContinuousZoomOffset()
    {
        // Just do the noise calculations, make sure to apply live multipliers from the handle

        // Live parameters
        float liveMagnitudeScalar = eventHandle.continuousZoomMagnitudeMultiplier * eventHandle.continuousZoomMagnitudeMultiplier;
        float inMagnitudeMultiplier = profileCopy.continuousZoomNoiseInMagnitude * liveMagnitudeScalar;
        float outMagnitudeMultiplier = profileCopy.continuousZoomNoiseOutMagnitude * liveMagnitudeScalar;

        float magnitudeRange = inMagnitudeMultiplier + outMagnitudeMultiplier;
        float magnitudeOffset = (outMagnitudeMultiplier - inMagnitudeMultiplier) / 2f;

        // Early out, result will be 0
        if (inMagnitudeMultiplier + outMagnitudeMultiplier <= 0f) return 0f;

        // Noise sample
        float noiseSample = NoiseRemap(Mathf.PerlinNoise(continuousZoomTime * profileCopy.continuousZoomNoiseFrequency, eventSeed));
        return (noiseSample * magnitudeRange) + magnitudeOffset;
    }
}
