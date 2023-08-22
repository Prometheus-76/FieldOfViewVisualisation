using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RumbleSystem : MonoBehaviour
{
    [Header("Configuration")]
    [Min(0.1f), Tooltip("Affects how rumble is combined, lower values require more rumble to reach the maximum effect")]
    public float falloffStrength;
    [Min(1f), Tooltip("Remaps added rumble such that lower values feel significantly less impactful than higher ones")]
    public float additivePower;

    // PRIVATE
    private float totalLowFrequency = 0f;
    private float totalHighFrequency = 0f;

    private void LateUpdate()
    {
        // Calculate intensity values along an asymptote approaching 1.0
        float combinedLowFrequencyIntensity = 1f - (1f / ((falloffStrength * totalLowFrequency) + 1f));
        float combinedHighFrequencyIntensity = 1f - (1f / ((falloffStrength * totalHighFrequency) + 1f));

        // Apply combined values
        InputManager.SetControllerRumble(combinedLowFrequencyIntensity, combinedHighFrequencyIntensity);

        // Reset rumble values
        totalLowFrequency = 0f;
        totalHighFrequency = 0f;
    }

    /// <summary>
    /// Add some rumble to the total which will be applied at the end of this frame, for best results this function should be called from Update()
    /// </summary>
    /// <param name="lowFrequency">The strength of the low frequency motor rumble</param>
    /// <param name="highFrequency">The strength of the high frequency motor rumble</param>
    public void AddRumbleThisFrame(float lowFrequency, float highFrequency)
    {
        lowFrequency = Mathf.Max(lowFrequency, 0f);
        highFrequency = Mathf.Max(highFrequency, 0f);

        totalLowFrequency += Mathf.Pow(lowFrequency, additivePower);
        totalHighFrequency += Mathf.Pow(highFrequency, additivePower);
    }
}
