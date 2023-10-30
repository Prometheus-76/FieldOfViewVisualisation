using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitstopSystem : MonoBehaviour
{
    [Header("Hitstop")]
    public float minimumHitstopTime;
    public float maximumHitstopTime;
    public float hitstopScalingGradient;

    [Header("Intermittence")]
    public float minimumIntermittenceTime;

    // PRIVATE
    private float currentStopTimer = 0f;
    private float currentIntermissionTimer = 0f;
    private float accumulatedLinearHitstopDuration = 0f;
    private float accumulatedScaledHitstopDuration = 0f;

    private void LateUpdate()
    {
        if (currentStopTimer <= 0f)
        {
            // End the hitstop
            Time.timeScale = 1f;
        }
        else
        {
            // Start/continue hitstop
            Time.timeScale = 0f;
            accumulatedLinearHitstopDuration = 0f;
            accumulatedScaledHitstopDuration = 0f;
        }

        currentStopTimer -= Time.unscaledDeltaTime;
        currentStopTimer = Mathf.Max(0f, currentStopTimer);
    }

    public void AddHitstop(float addedLinearDuration)
    {
        accumulatedLinearHitstopDuration += addedLinearDuration;

        // Find how much time we're adding onto the hitstop this frame
        float addedScaledDuration = EvaluateDampingFunction(maximumHitstopTime, hitstopScalingGradient, accumulatedLinearHitstopDuration) - accumulatedScaledHitstopDuration;

        accumulatedScaledHitstopDuration += addedScaledDuration;
        currentStopTimer += addedScaledDuration;
    }

    // For converting linear input to output damped within the specified range
    private float EvaluateDampingFunction(float range, float gradient, float input)
    {
        return range * MathUtilities.Tanh((gradient * input) / range);
    }
}
