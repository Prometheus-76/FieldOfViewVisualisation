using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Brush_", menuName = "ScriptableObject/BrushProfile")]
public class BrushProfile : ScriptableObject
{
    [Header("Brush Size")]
    [Min(0.01f), Tooltip("Size of the inner brush radius, which paints at 100% strength.")]
    public float brushInnerRadius = 0.3f;
    [Min(0.01f), Tooltip("How much larger the outer radius is compared to the inner radius. The brush texture is active between them.")]
    public float brushOuterOffset = 0.4f;

    [Header("Brush Detail")]
    [Tooltip("The texture used to offset the circumference of the brush.")]
    public Texture2D brushTexture;
    [Min(0f), Tooltip("Offsets the brightness of the brush texture.")]
    public float brushTextureStrength = 1f;
    [Min(0.01f), Tooltip("Multiplies the UVs of the brush texture, at 1.0 the texture is mapped 1:1 with world units.")]
    public float brushTextureScale = 0.3f;
}
