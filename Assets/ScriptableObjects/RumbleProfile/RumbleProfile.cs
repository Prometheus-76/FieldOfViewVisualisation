using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rumble Profile", menuName = "ScriptableObject/RumbleProfile")]
public class RumbleProfile : ScriptableObject
{
    // Low frequency
    public bool lowFrequencyRumble = false;
    public float lowFrequencyStrength = 1f;
    public float lowFrequencyInDuration = 1f;
    public float lowFrequencyHoldDuration = 1f;
    public float lowFrequencyOutDuration = 1f;
    public Curves.CurveStyle lowFrequencyInCurve = Curves.CurveStyle.Linear;
    public Curves.CurveStyle lowFrequencyOutCurve = Curves.CurveStyle.Linear;

    // High frequency
    public bool highFrequencyRumble = false;
    public float highFrequencyStrength = 1f;
    public float highFrequencyInDuration = 1f;
    public float highFrequencyHoldDuration = 1f;
    public float highFrequencyOutDuration = 1f;
    public Curves.CurveStyle highFrequencyInCurve = Curves.CurveStyle.Linear;
    public Curves.CurveStyle highFrequencyOutCurve = Curves.CurveStyle.Linear;

    public float EvaluateLowFrequency(float progress01)
    {
        if (lowFrequencyRumble == false) return 0f;

        float totalDuration = CalculateLowFrequencyDuration();
        float evaluationTime = Mathf.Clamp01(progress01) * totalDuration;

        if (evaluationTime < lowFrequencyInDuration)
        {
            // In-curve
            if (lowFrequencyInDuration <= 0f) return 0f;

            float interpolant = evaluationTime / lowFrequencyInDuration;
            return Curves.Evaluate(lowFrequencyInCurve, interpolant) * lowFrequencyStrength;
        }
        else if (evaluationTime > (lowFrequencyInDuration + lowFrequencyHoldDuration))
        {
            // Out-curve
            if (lowFrequencyOutDuration <= 0f) return 0f;
            
            float interpolant = (evaluationTime - (lowFrequencyInDuration + lowFrequencyHoldDuration)) / lowFrequencyOutDuration;
            return Curves.Evaluate(lowFrequencyOutCurve, 1f - interpolant) * lowFrequencyStrength;
        }

        // Between in and out curves
        return lowFrequencyStrength;
    }

    public float EvaluateHighFrequency(float progress01)
    {
        if (highFrequencyRumble == false) return 0f;

        float totalDuration = CalculateHighFrequencyDuration();
        float evaluationTime = Mathf.Clamp01(progress01) * totalDuration;

        if (evaluationTime < highFrequencyInDuration)
        {
            // In-curve
            if (highFrequencyInDuration <= 0f) return 0f;

            float interpolant = evaluationTime / highFrequencyInDuration;
            return Curves.Evaluate(highFrequencyInCurve, interpolant) * highFrequencyStrength;
        }
        else if (evaluationTime > (highFrequencyInDuration + highFrequencyHoldDuration))
        {
            // Out-curve
            if (highFrequencyOutDuration <= 0f) return 0f;

            float interpolant = (evaluationTime - (highFrequencyInDuration + highFrequencyHoldDuration)) / highFrequencyOutDuration;
            return Curves.Evaluate(highFrequencyOutCurve, 1f - interpolant) * highFrequencyStrength;
        }

        // Between in and out curves
        return highFrequencyStrength;
    }

    public float CalculateLowFrequencyDuration()
    {
        if (lowFrequencyRumble == false) return 0f;

        return lowFrequencyInDuration + lowFrequencyHoldDuration + lowFrequencyOutDuration;
    }

    public float CalculateHighFrequencyDuration()
    {
        if (highFrequencyRumble == false) return 0f;

        return highFrequencyInDuration + highFrequencyHoldDuration + highFrequencyOutDuration;
    }
}
