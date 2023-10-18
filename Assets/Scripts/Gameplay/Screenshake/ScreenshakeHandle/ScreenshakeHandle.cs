using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeHandle
{
    private ScreenshakeEvent screenshakeEvent = null;

    public ScreenshakeHandle(ScreenshakeEvent screenshakeEvent)
    {
        this.screenshakeEvent = screenshakeEvent;
    }

    public void RemoveEvent() => screenshakeEvent.RemoveEvent();
}
