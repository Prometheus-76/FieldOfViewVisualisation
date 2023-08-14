using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Systems")]
    public PaintManager paintManager;

    // Start is called before the first frame update
    void Start()
    {
        if (paintManager != null) paintManager.Initialise();
    }
}
