using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScreenshakeEvent
{
    private void CalculatePositionOffset() => positionOffset = isDiscrete ? CalculateDiscretePositionOffset() : CalculateContinuousPositionOffset();

    private Vector2 CalculateDiscretePositionOffset()
    {
        // Is this noise or an animation?
        if (profileCopy.discretePositionStyle == ScreenshakeSystem.DiscreteType.Noise)
        {

        }
        else if (profileCopy.discretePositionStyle == ScreenshakeSystem.DiscreteType.Animation)
        {

        }

        return Vector2.zero;
    }

    private Vector2 CalculateContinuousPositionOffset()
    {
        // Just do the noise calculations, make sure to apply live multipliers from the handle

        return Vector2.zero;
    }
}
