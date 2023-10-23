using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScreenshakeEvent
{
    private void CalculateZoomOffset() => zoomOffset = isDiscrete ? CalculateDiscreteZoomOffset() : CalculateContinuousZoomOffset();

    private float CalculateDiscreteZoomOffset()
    {
        // Is this noise or an animation?
        if (profileCopy.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Noise)
        {

        }
        else if (profileCopy.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Animation)
        {

        }

        return 0f;
    }

    private float CalculateContinuousZoomOffset()
    {
        // Just do the noise calculations, make sure to apply live multipliers from the handle

        return 0f;
    }
}
