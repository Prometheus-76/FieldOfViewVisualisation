using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitstopSystem : MonoBehaviour
{
    [Header("Hitstop")]
    [Min(1)]
    public int minimumHitstopMilliseconds;
    [Min(1)]
    public int hitstopRangeMilliseconds;
    [Range(0.1f, 1f)]
    public float hitstopScalingGradient;

    [Header("Intermission")]
    [Min(0)]
    public int minimumIntermissionMilliseconds;

    // PROPERTIES
    public bool isHitstopHappening { get; private set; } = false;
    public bool isIntermissionHappening { get; private set; } = false;

    // PRIVATE
    private float previousHitstopTimer = 0f;
    private float currentHitstopTimer = 0f;
    private float previousIntermissionTimer = 0f;
    private float currentIntermissionTimer = 0f;
    private float currentLinearHitstop = 0f;
    private float currentScaledHitstop = 0f;

    private void LateUpdate()
    {
        // Update the intermission period and then the hitstop effect, important it happens in this order each frame since:
        // - The intermission period can lead directly into another hitstop effect, but it isn't guaranteed
        // - However the hitstop effect will always lead directly into an intermission period of at least one frame
        if (isIntermissionHappening && isHitstopHappening == false) UpdateIntermission(Time.unscaledDeltaTime);
        if (isIntermissionHappening == false) UpdateHitstop(Time.unscaledDeltaTime);
    }

    #region Public Methods

    /// <summary>
    /// Apply some hitstop at the end of the next frame, which is not drawn during a hitstop intermission
    /// </summary>
    /// <param name="addedMilliseconds">The amount of hitstop to be added, in whole milliseconds</param>
    public void AddHitstop(int addedMilliseconds)
    {
        currentLinearHitstop += (addedMilliseconds / 1000f);

        // Find how much time we're adding onto the hitstop this frame
        float maximumHitstopSeconds = ((minimumHitstopMilliseconds + hitstopRangeMilliseconds) / 1000f);
        float combinedScaledDuration = MathUtilities.DampValueToRange(maximumHitstopSeconds, hitstopScalingGradient, currentLinearHitstop);
        float addedScaledDuration = combinedScaledDuration - currentScaledHitstop;

        currentScaledHitstop = combinedScaledDuration;
        currentHitstopTimer += addedScaledDuration;
    }

    #endregion

    private void UpdateHitstop(float unscaledDeltaTime)
    {
        // Is the hitstop effect starting or ending at the end of this frame?
        if (isHitstopHappening == false && currentHitstopTimer > 0f && previousHitstopTimer <= 0f) OnHitstopStart();
        if (isHitstopHappening && currentHitstopTimer <= 0f && previousHitstopTimer > 0f) OnHitstopEnd();

        previousHitstopTimer = currentHitstopTimer;

        // Update hitstop timer
        currentHitstopTimer -= unscaledDeltaTime;
        if (isHitstopHappening == false)
        {
            currentHitstopTimer = Mathf.Max(0f, currentHitstopTimer);
        }
    }

    private void UpdateIntermission(float unscaledDeltaTime)
    {
        previousIntermissionTimer = currentIntermissionTimer;

        // Update intermission timer
        currentIntermissionTimer -= unscaledDeltaTime;
        currentIntermissionTimer = Mathf.Max(0f, currentIntermissionTimer);

        bool intermissionLastsOneFrame = (minimumIntermissionMilliseconds <= 0);
        bool intermissionEndedThisFrame = (currentIntermissionTimer <= 0f && previousIntermissionTimer > 0f);

        // Is the intermission period ending at the end of this frame?
        if (isIntermissionHappening && intermissionLastsOneFrame || intermissionEndedThisFrame) OnIntermissionEnd();
    }

    private void OnHitstopStart()
    {
        isHitstopHappening = true;

        // Ensure hitstop is applied for at least the minimum duration
        currentLinearHitstop = Mathf.Max((minimumHitstopMilliseconds / 1000f), currentLinearHitstop);

        float maximumHitstopSeconds = ((minimumHitstopMilliseconds + hitstopRangeMilliseconds) / 1000f);
        currentScaledHitstop = MathUtilities.DampValueToRange(maximumHitstopSeconds, hitstopScalingGradient, currentLinearHitstop);

        currentHitstopTimer = currentScaledHitstop;

        // Start hitstop effect
        Time.timeScale = 0f;
    }

    private void OnHitstopEnd()
    {
        isHitstopHappening = false;

        // Reset trackers for previous hitstop effect duration
        currentLinearHitstop = 0f;
        currentScaledHitstop = 0f;

        // End hitstop effect
        Time.timeScale = 1f;

        // Intermission period starts on the same frame that the hitstop ends
        OnIntermissionStart();
    }

    private void OnIntermissionStart()
    {
        isIntermissionHappening = true;

        // Start intermission timer
        currentIntermissionTimer = (minimumIntermissionMilliseconds / 1000f);
        previousIntermissionTimer = 0f;
    }

    private void OnIntermissionEnd()
    {
        isIntermissionHappening = false;

        // Hitstop has not been running this frame, ensure timer is reset
        previousHitstopTimer = 0f;
    }
}