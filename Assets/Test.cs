using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public CarvableMesh carvableMesh;

    // Start is called before the first frame update
    void Start()
    {
        carvableMesh.Initialise();
        carvableMesh.UpdateMesh();
    }
}
