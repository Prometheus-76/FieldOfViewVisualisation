using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugUI : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI averageFpsText;
    public TextMeshProUGUI averageFrameTimeText;
    public TextMeshProUGUI controlSchemeText;

    [Header("Configuration")]
    [Min(0f)]
    public float updateInterval;

    // PRIVATE
    private float updateTimer = 0f;

    private int framesSinceLastUpdate = 0;

    // Update is called once per frame
    void Update()
    {
        // Update periodic UI every N seconds
        updateTimer += Time.unscaledDeltaTime;
        if(updateTimer >= updateInterval)
        {
            UpdatePeriodicUI(updateTimer);
            updateTimer = 0f;
        }

        UpdateContinuousUI();

        // Track data across frames
        framesSinceLastUpdate += 1;
    }

    void UpdatePeriodicUI(float timeSinceUpdate)
    {
        float fpsAverage = (framesSinceLastUpdate / timeSinceUpdate);
        averageFpsText.text = "FPS (average): " + fpsAverage.ToString("F0");
        averageFrameTimeText.text = "FRAME TIME (average): " + (1000f / Mathf.Max(fpsAverage, 1)).ToString("F2") + "ms";

        // Reset data
        framesSinceLastUpdate = 0;
    }

    void UpdateContinuousUI()
    {
        controlSchemeText.text = "CONTROL SCHEME (last used): " + InputManager.GetControlScheme();
    }
}
