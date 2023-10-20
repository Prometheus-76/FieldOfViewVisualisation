using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    [Header("Systems")]
    public PaintSystem paintSystem;
    public ScreenshakeSystem screenshakeSystem;
    public PaintProfile testPaint;
    public ScreenshakeProfile testShake;

    // Start is called before the first frame update
    void Start()
    {
        if (paintSystem != null) paintSystem.Initialise();
        if (screenshakeSystem != null) screenshakeSystem.Initialise();

        MaskablePaint paint = paintSystem.GetPaint(transform, testPaint, true);
        paint.Splatter();

        screenshakeSystem.AddShake(testShake);
    }
}
