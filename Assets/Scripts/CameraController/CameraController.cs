using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    public CarvableMesh visionMesh;

    void Start()
    {
        visionMesh.Initialise();
    }

    void LateUpdate()
    {
        visionMesh.UpdateMesh();
    }
}
