using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeSystem : MonoBehaviour
{
    #region Data Structures

    public enum ShakeType { Discrete, Continuous }
    public enum DiscreteType { Noise, Animation }
    public enum ZoomDirection { ZoomIn, ZoomOut }
    public enum TimescaleMode { Realtime, Scaled }

    #endregion

    [Header("Time Scaling")]
    public TimescaleMode eventTimescaling = TimescaleMode.Realtime;
    public TimescaleMode responseTimescaling = TimescaleMode.Realtime;

    [Header("Shake Smoothing")]
    [Min(0f)]
    public float positionResponseDelay = 0f;
    [Min(0f)]
    public float rotationResponseDelay = 0f;
    [Min(0f)]
    public float zoomResponseDelay = 0f;

    // PROPERTIES
    public bool isInitialised { get; private set; } = false;
    public bool isPaused { get; private set; } = false;

    public Vector2 smoothPositionOffset { get; private set; } = Vector2.zero;
    public float smoothRotationOffset { get; private set; } = 0f;
    public float smoothZoomOffset { get; private set; } = 0f;

    // PRIVATE
    private List<ScreenshakeEvent> activeEvents = null;
    private LinkedList<ScreenshakeEvent> availableEvents = null;

    private Vector2 rawPositionOffset = Vector2.zero;
    private float rawRotationOffset = 0f;
    private float rawZoomOffset = 0f;

    private Vector2 positionVelocity = Vector2.zero;
    private float rotationVelocity = 0f;
    private float zoomVelocity = 0f;

    private int eventsCreatedSoFar = 0;

    #region Public Methods

    /// <summary>
    /// Setup the screenshake system
    /// </summary>
    public void Initialise()
    {
        if (isInitialised) return;
        isInitialised = true;

        activeEvents = new List<ScreenshakeEvent>();
        availableEvents = new LinkedList<ScreenshakeEvent>();
    }

    /// <summary>
    /// Add a new shake event to the camera
    /// </summary>
    /// <param name="shakeProfile">Details of the shake event we want to add</param>
    /// <returns>Handle for the event which is added, can be used to monitor and control the event in real-time</returns>
    public ScreenshakeHandle AddShake(ScreenshakeProfile shakeProfile)
    {
        if (isInitialised == false) Initialise();

        // Get an event from the pool, or create one
        ScreenshakeEvent newEvent = GetConfiguredEvent(shakeProfile);

        // Add it to the list
        activeEvents.Add(newEvent);

        // Return event handle
        return newEvent.eventHandle;
    }

    /// <summary>
    /// Pause or unpause any ongoing screenshake events
    /// </summary>
    /// <param name="state">Whether the pause the ScreenshakeSystem</param>
    public void SetPaused(bool state)
    {
        if (isInitialised == false) Initialise();

        if (isPaused == state) return;
        isPaused = state;
    }  

    #endregion

    private void Update()
    {
        if (isPaused == false)
        {
            // How much time has passed in our desired timescaling modes?
            float eventDeltatime = (eventTimescaling == TimescaleMode.Realtime) ? Time.unscaledDeltaTime : Time.deltaTime;
            float responseDeltatime = (responseTimescaling == TimescaleMode.Realtime) ? Time.unscaledDeltaTime : Time.deltaTime;

            ProcessActiveEvents(eventDeltatime, responseDeltatime);
        }
    }

    private void ProcessActiveEvents(float eventDeltatime, float responseDeltatime)
    {
        // Reset offsets
        rawPositionOffset = Vector2.zero;
        rawRotationOffset = 0f;
        rawZoomOffset = 0f;

        // Process raw offsets from each event
        for (int i = 0; i < activeEvents.Count; i++)
        {
            ScreenshakeEvent thisEvent = activeEvents[i];

            // This event should be removed
            if (thisEvent.isComplete)
            {
                // Disconnect handle from the event
                thisEvent.eventHandle.RemoveEvent();

                // Move event from active list to available pool
                activeEvents.RemoveAt(i);
                availableEvents.AddLast(thisEvent);
                
                // Decrement iterator to compensate
                i -= 1;

                // Don't proceed with this event
                continue;
            }

            // Update shake event offsets
            thisEvent.CalculateShakeOffsets();

            // Accumulate offsets across shake properties
            rawPositionOffset += thisEvent.positionOffset;
            rawRotationOffset += thisEvent.rotationOffset;
            rawZoomOffset += thisEvent.zoomOffset;

            // Progress event timer
            thisEvent.UpdateEventTimers(eventDeltatime);
        }

        // Apply holistic smoothing across all events
        smoothPositionOffset = Vector2.SmoothDamp(smoothPositionOffset, rawPositionOffset, ref positionVelocity, positionResponseDelay, Mathf.Infinity, responseDeltatime);
        smoothRotationOffset = Mathf.SmoothDamp(smoothRotationOffset, rawRotationOffset, ref rotationVelocity, rotationResponseDelay, Mathf.Infinity, responseDeltatime);
        smoothZoomOffset = Mathf.SmoothDamp(smoothZoomOffset, rawZoomOffset, ref zoomVelocity, zoomResponseDelay, Mathf.Infinity, responseDeltatime);
    }

    private ScreenshakeEvent GetConfiguredEvent(ScreenshakeProfile shakeProfile)
    {
        ScreenshakeEvent eventInstance;

        // Should we assign new memory?
        if (availableEvents.Count > 0)
        {
            // Pull from event pool, and reset it
            eventInstance = availableEvents.First.Value;
            availableEvents.RemoveFirst();

            eventInstance.ResetEvent();
        }
        else
        {
            // Assign new memory
            eventInstance = new ScreenshakeEvent(eventsCreatedSoFar);
            eventsCreatedSoFar += 1;
        }

        // Configure new event and return it
        eventInstance.ConfigureEventAndHandle(shakeProfile);
        return eventInstance;
    }
}
