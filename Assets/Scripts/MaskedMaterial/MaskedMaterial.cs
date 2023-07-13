using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Masked Material", menuName = "ScriptableObject/MaskedMaterial")]
public class MaskedMaterial : ScriptableObject
{
    [Header("Material Attachment")]
    public Material material = null;
    public string tilingPropertyReference = "";
    public string offsetPropertyReference = "";

    [Header("Material Configuration")]
    [Min(0.01f)]
    public float scale = 1f;
    public bool scaleWithCameraSize = true;
    [Range(0, 2f)]
    public float parallaxStrength = 0f;
}
