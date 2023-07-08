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
        averageFpsText.text = "FPS (average): " + (framesSinceLastUpdate / timeSinceUpdate).ToString("F0");
        averageFrameTimeText.text = "FRAME TIME (average): " + ((timeSinceUpdate / framesSinceLastUpdate) * 1000f).ToString("F2") + "ms";

        // Reset data
        framesSinceLastUpdate = 0;
    }
}
