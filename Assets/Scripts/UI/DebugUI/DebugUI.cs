using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class DebugUI : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI averageFpsText;
    public TextMeshProUGUI averageFrameTimeText;

    [Header("Configuration")]
    [Min(0f)]
    public float updateInterval;

    // PRIVATE
    private float updateTimer = 0f;

    private int framesSinceLastUpdate = 0;

    // Update is called once per frame
    void Update()
    {
        // Update every N seconds
        updateTimer += Time.unscaledDeltaTime;
        if(updateTimer >= updateInterval)
        {
            UpdateUI(updateTimer);
            updateTimer = 0f;
        }

        // Track data across frames
        framesSinceLastUpdate += 1;
    }

    void UpdateUI(float timeSinceUpdate)
    {
        float fpsAverage = (framesSinceLastUpdate / timeSinceUpdate);
        averageFpsText.text = "FPS (average): " + fpsAverage.ToString("F0");
        averageFrameTimeText.text = "FRAME TIME (average): " + (1000f / Mathf.Max(fpsAverage, 1)).ToString("F2") + "ms";

        // Reset data
        framesSinceLastUpdate = 0;
    }
}
