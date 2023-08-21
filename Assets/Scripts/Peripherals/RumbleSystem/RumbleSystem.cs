using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RumbleSystem : MonoBehaviour
{
    [Min(0.1f)]
    public float falloffScalar;

    private List<RumbleEvent> activeEvents;
    private LinkedList<RumbleEvent> availableEvents;

    public void Initialise()
    {
        activeEvents = new List<RumbleEvent>();
        availableEvents = new LinkedList<RumbleEvent>();
    }

    public void Update()
    {
        float totalLowFrequencyIntensity = 0f;
        float totalHighFrequencyIntensity = 0f;

        for (int i = 0; i < activeEvents.Count; i++)
        {
            activeEvents[i].Simulate(Time.deltaTime);

            totalLowFrequencyIntensity += activeEvents[i].lowFrequencyIntensity;
            totalHighFrequencyIntensity += activeEvents[i].highFrequencyIntensity;

            if (activeEvents[i].hasCompleted)
            {
                // Return this event to the pool on completion
                availableEvents.AddLast(activeEvents[i]);
                activeEvents.RemoveAt(i);
                i -= 1;
            }
        }

        // Calculate asymptotic combined intensity
        float combinedLowFrequencyIntensity = 1f - (1f / ((falloffScalar * totalLowFrequencyIntensity) + 1f));
        float combinedHighFrequencyIntensity = 1f - (1f / ((falloffScalar * totalHighFrequencyIntensity) + 1f));

        // Apply combined values
        InputManager.SetControllerRumble(combinedLowFrequencyIntensity, combinedHighFrequencyIntensity);
    }

    public void AddRumbleEvent(RumbleProfile rumbleProfile, float timeOffset = 0f)
    {
        RumbleEvent newEvent;

        if (availableEvents.Count > 0)
        {
            newEvent = availableEvents.First.Value;
            availableEvents.RemoveFirst();
        }
        else
        {
            newEvent = new RumbleEvent();
        }

        newEvent.Configure(rumbleProfile);
        newEvent.Simulate(timeOffset);
        activeEvents.Add(newEvent);
    }
}
