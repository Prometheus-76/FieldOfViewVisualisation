using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScreenshakeEvent
{
    private void CalculateRotationOffset() => rotationOffset = isDiscrete ? CalculateDiscreteRotationOffset() : CalculateContinuousRotationOffset();

    private float CalculateDiscreteRotationOffset()
    {
        // Rotation always uses noise, we can just do the calculations here

        // Find intensity multiplier from curve
        float intensityMultiplier;
        if (profileCopy.discreteRotationNoiseUseCustomCurve)
        {
            intensityMultiplier = profileCopy.discreteRotationNoiseCustomCurve.Evaluate(discreteCompletion01);
        }
        else
        {
            // Shortened for readability
            Curves.CurveStyle curveIn = profileCopy.discreteRotationNoiseFixedCurveIn;
            float midpointMin = profileCopy.discreteRotationNoiseFixedCurveMidpointMin;
            float midpointMax = profileCopy.discreteRotationNoiseFixedCurveMidpointMax;
            Curves.CurveStyle curveOut = profileCopy.discreteRotationNoiseFixedCurveOut;

            intensityMultiplier = EvaluateFixedCurve(curveIn, midpointMin, midpointMax, curveOut, discreteCompletion01);
        }

        // Early out, result will be 0
        if (intensityMultiplier <= 0f) return 0f;

        // Noise sample
        return NoiseRemap(Mathf.PerlinNoise(eventSeed, eventLifetimeTimer * profileCopy.discreteRotationNoiseFrequency)) * profileCopy.discreteRotationNoiseMagnitude * intensityMultiplier;
    }

    private float CalculateContinuousRotationOffset()
    {
        // Just do the noise calculations, make sure to apply live multipliers from the handle

        // Live parameters
        float magnitudeMultiplier = (profileCopy.continuousRotationNoiseMagnitude * eventHandle.continuousRotationMagnitudeMultiplier);

        // Early out, result will be 0
        if (magnitudeMultiplier <= 0f) return 0f;

        // Noise sample
        return NoiseRemap(Mathf.PerlinNoise(eventSeed, continuousRotationTime * profileCopy.continuousRotationNoiseFrequency)) * magnitudeMultiplier;
    }
}
