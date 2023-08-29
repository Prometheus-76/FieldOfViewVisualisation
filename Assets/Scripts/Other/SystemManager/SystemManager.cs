using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    [Header("Systems")]
    public PaintSystem paintSystem;
    public PaintProfile testPaint;

    // Start is called before the first frame update
    void Start()
    {
        if (paintSystem != null) paintSystem.Initialise();

        MaskablePaint paint = paintSystem.GetPaint(transform, testPaint, true);
        paint.Splatter();
    }
}
