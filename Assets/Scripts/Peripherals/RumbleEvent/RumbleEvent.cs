using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RumbleEvent
{
    // PRIVATE
    private RumbleProfile rumbleProfile;

    private float elapsedTime = 0f;
    private float lowFrequencyDuration = 0f;
    private float highFrequencyDuration = 0f;

    public bool hasCompleted { get; private set; }
    public float lowFrequencyIntensity { get; private set; }
    public float highFrequencyIntensity { get; private set; }

    public void Configure(RumbleProfile newProfile)
    {
        rumbleProfile = newProfile;

        elapsedTime = 0f;
        lowFrequencyDuration = rumbleProfile.CalculateLowFrequencyDuration();
        highFrequencyDuration = rumbleProfile.CalculateHighFrequencyDuration();

        lowFrequencyIntensity = 0f;
        highFrequencyIntensity = 0f;
        hasCompleted = false;
    }

    public void Simulate(float deltaTime)
    {
        elapsedTime += deltaTime;

        // Process low frequency rumble
        if (rumbleProfile.lowFrequencyRumble)
        {
            float lowFrequencyProgress01 = Mathf.Clamp01(elapsedTime / lowFrequencyDuration);

            if (elapsedTime <= lowFrequencyDuration) lowFrequencyIntensity = rumbleProfile.EvaluateLowFrequency(lowFrequencyProgress01);
            else lowFrequencyDuration = 0f;
        }

        // Process high frequency rumble
        if (rumbleProfile.highFrequencyRumble)
        {
            float highFrequencyProgress01 = Mathf.Clamp01(elapsedTime / highFrequencyDuration);

            if (elapsedTime <= highFrequencyDuration) highFrequencyIntensity = rumbleProfile.EvaluateHighFrequency(highFrequencyProgress01);
            else highFrequencyDuration = 0f;
        }

        // Check if the rumble event has passed the maximum duration
        if (elapsedTime > Mathf.Max(lowFrequencyDuration, highFrequencyDuration)) hasCompleted = true;
    }
}
