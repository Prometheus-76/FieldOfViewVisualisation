using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Masked Material", menuName = "ScriptableObject/MaskedMaterial")]
public class MaskedMaterial : ScriptableObject
{
    [Header("Configuration")]
    public Material material = null;
    [Min(0.01f)]
    public float scale = 1f;
    [Range(-2f, 2f)]
    public float parallaxStrength = 0f;
}
