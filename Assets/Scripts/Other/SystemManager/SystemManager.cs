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

    private bool testShakePlayed = false;

    // Start is called before the first frame update
    void Start()
    {
        if (paintSystem != null) paintSystem.Initialise();
        if (screenshakeSystem != null) screenshakeSystem.Initialise();

        MaskablePaint paint = paintSystem.GetPaint(transform, testPaint, true);
        paint.Splatter();
    }

    private void Update()
    {
        if (testShakePlayed == false && InputManager.GetPhaseButton())
        {
            testShakePlayed = true;
            screenshakeSystem.AddShake(testShake);
        }

        if (testShakePlayed && InputManager.GetPhaseButton() == false) testShakePlayed = false;
    }
}
