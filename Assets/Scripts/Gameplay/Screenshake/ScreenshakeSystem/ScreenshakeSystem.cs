using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeSystem : MonoBehaviour
{
    public enum ShakeType { Continuous, Discrete }
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
    /// <param name="newShake">Details of the shake event we want to add</param>
    /// <returns>Handle for the event which is added, can be used to monitor and control the event in real-time</returns>
    public ScreenshakeHandle AddShake(ScreenshakeProfile newShake)
    {
        if (isInitialised == false) Initialise();

        // Create event
        ScreenshakeEvent newEvent = null;

        // Add it to the list
        activeEvents.Add(newEvent);

        // Return event handle
        return newEvent.eventHandle;
    }

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
            ProcessActiveEvents();
        }
    }

    private void ProcessActiveEvents()
    {

    }
}
