using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScreenshakeEvent
{
    private void CalculatePositionOffset() => positionOffset = isDiscrete ? CalculateDiscretePositionOffset() : CalculateContinuousPositionOffset();

    private Vector2 CalculateDiscretePositionOffset()
    {
        // Is this noise or an animation?
        if (profileCopy.discretePositionStyle == ScreenshakeSystem.DiscreteType.Noise)
        {
            return CalculateDiscretePositionNoiseOffset();
        }
        else if (profileCopy.discretePositionStyle == ScreenshakeSystem.DiscreteType.Animation)
        {
            return CalculateDiscretePositionAnimationOffset();
        }

        return Vector2.zero;
    }

    private Vector2 CalculateDiscretePositionNoiseOffset()
    {
        // Find intensity multiplier from curve
        float intensityMultiplier;
        if (profileCopy.discretePositionNoiseUseCustomCurve)
        {
            intensityMultiplier = profileCopy.discretePositionNoiseCustomCurve.Evaluate(discreteCompletion01);
        }
        else
        {
            // Shortened for readability
            Curves.CurveStyle curveIn = profileCopy.discretePositionNoiseFixedCurveIn;
            float midpointMin = profileCopy.discretePositionNoiseFixedCurveMidpointMin;
            float midpointMax = profileCopy.discretePositionNoiseFixedCurveMidpointMax;
            Curves.CurveStyle curveOut = profileCopy.discretePositionNoiseFixedCurveOut;

            intensityMultiplier = EvaluateFixedCurve(curveIn, midpointMin, midpointMax, curveOut, discreteCompletion01);
        }

        // Early out, result will be (0, 0)
        if (intensityMultiplier <= 0f) return Vector2.zero;
        intensityMultiplier *= Mathf.Abs(intensityMultiplier);

        // Noise samples
        Vector2 noiseSamples;
        noiseSamples.x = NoiseRemap(Mathf.PerlinNoise(-eventSeed, continuousPositionTime.x * profileCopy.continuousPositionNoiseFrequency.x));
        noiseSamples.y = NoiseRemap(Mathf.PerlinNoise(continuousPositionTime.y * profileCopy.continuousPositionNoiseFrequency.y, -eventSeed));
        return noiseSamples * intensityMultiplier;
    }

    private Vector2 CalculateDiscretePositionAnimationOffset()
    {
        // Find intensity multiplier from curve
        float intensityMultiplier;
        if (profileCopy.discretePositionAnimationUseCustomCurve)
        {
            intensityMultiplier = profileCopy.discretePositionAnimationCustomCurve.Evaluate(discreteCompletion01);
        }
        else
        {
            // Shortened for readability
            Curves.CurveStyle curveIn = profileCopy.discretePositionAnimationFixedCurveIn;
            float midpointMin = profileCopy.discretePositionAnimationFixedCurveMidpointMin;
            float midpointMax = profileCopy.discretePositionAnimationFixedCurveMidpointMax;
            Curves.CurveStyle curveOut = profileCopy.discretePositionAnimationFixedCurveOut;

            intensityMultiplier = EvaluateFixedCurve(curveIn, midpointMin, midpointMax, curveOut, discreteCompletion01);
        }

        // Early out, result will be (0, 0)
        if (intensityMultiplier <= 0f) return Vector2.zero;
        intensityMultiplier *= Mathf.Abs(intensityMultiplier);

        // Maximum extent of the animation, scaled by the intensity curve
        Vector2 maxMagnitude = profileCopy.discretePositionAnimationMagnitude * profileCopy.discretePositionAnimationDirection;
        return maxMagnitude * intensityMultiplier;
    }

    private Vector2 CalculateContinuousPositionOffset()
    {
        // Just do the noise calculations, make sure to apply live multipliers from the handle

        // Live parameters
        Vector2 liveMagnitudeScalar = eventHandle.continuousPositionMagnitudeMultiplier * eventHandle.continuousPositionMagnitudeMultiplier;
        Vector2 magnitudeMultiplier = (profileCopy.continuousPositionNoiseMagnitude * liveMagnitudeScalar);

        // Early out, result will be 0
        if (magnitudeMultiplier.sqrMagnitude <= 0f) return Vector2.zero;

        // Noise samples
        Vector2 noiseSamples;
        noiseSamples.x = NoiseRemap(Mathf.PerlinNoise(-eventSeed, continuousPositionTime.x * profileCopy.continuousPositionNoiseFrequency.x));
        noiseSamples.y = NoiseRemap(Mathf.PerlinNoise(continuousPositionTime.y * profileCopy.continuousPositionNoiseFrequency.y, -eventSeed));
        return noiseSamples * magnitudeMultiplier;
    }
}
