using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeSystem : MonoBehaviour
{
    public enum ShakeType { Discrete, Continuous }
    public enum DiscreteType { Noise, Animation }

    // PROPERTIES
    public bool isInitialised { get; private set; } = false;
    public bool isPaused { get; private set; } = false;

    // PRIVATE
    private List<ScreenshakeEvent> activeEvents = null;

    #region Public Methods

    /// <summary>
    /// Setup the screenshake system
    /// </summary>
    public void Initialise()
    {
        if (isInitialised) return;
        isInitialised = true;

        activeEvents = new List<ScreenshakeEvent>();
    }

    /// <summary>
    /// Add a new shake event to the camera
    /// </summary>
    /// <param name="shakeProfile">Details of the shake event we want to add</param>
    /// <returns>Handle for the event which is added, can be used to monitor and control the event in real-time</returns>
    public ScreenshakeHandle AddShake(ScreenshakeProfile shakeProfile)
    {
        if (isInitialised == false) Initialise();

        // Create event (TODO: Event pooling? Handle pooling would be far too dangerous because they are held by reference externally)
        ScreenshakeEvent newEvent = new ScreenshakeEvent(this, shakeProfile);

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

    public void RemoveEvent(ScreenshakeEvent shakeEvent)
    {
        if (isInitialised == false) Initialise();

        // Remove or mark the object for removal here
    }

    #endregion

    private void Update()
    {
        if (isPaused == false)
        {
            ProcessActiveEvents(Time.deltaTime);
        }
    }

    private void ProcessActiveEvents(float deltaTime)
    {
        for (int i = 0; i < activeEvents.Count; i++)
        {
            activeEvents[i].UpdateEvent(deltaTime);

            Vector3 positionOffset = activeEvents[i].positionOffset;
            Quaternion rotationOffset = activeEvents[i].rotationOffset;
            float zoomOffset = activeEvents[i].zoomOffset;
        }
    }
}
