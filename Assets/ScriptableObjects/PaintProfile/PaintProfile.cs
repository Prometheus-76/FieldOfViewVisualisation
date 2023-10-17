using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Paint_", menuName = "ScriptableObject/PaintProfile")]
public class PaintProfile : ScriptableObject
{
    [Header("Paint Size")]
    [SerializeField, Min(0.01f)]
    private float basePaintSize;
    [SerializeField, Min(0f)]
    private float paintSizeVariance;

    [Header("Paint Appearance")]
    [SerializeField]
    private List<Texture2D> paintTextures;
    [SerializeField, Space(5f)]
    private List<Color> paintColours;

    #region Public Methods

    /// <summary>
    /// Get a random size for this paint object
    /// </summary>
    /// <returns>The side length of the paint object</returns>
    public float GetRandomSize()
    {
        return Random.Range(basePaintSize, basePaintSize + paintSizeVariance);
    }

    /// <summary>
    /// Get a random texture for this paint object
    /// </summary>
    /// <returns>The texture of the paint object</returns>
    public Texture2D GetRandomTexture()
    {
        // Ensure there are assigned textures to choose from
        if (paintTextures.Count <= 0) return null;

        int randomIndex = Random.Range(0, paintTextures.Count);
        return paintTextures[randomIndex];
    }

    /// <summary>
    /// Get a random colour for this paint object
    /// </summary>
    /// <returns>The colour of the paint object</returns>
    public Color GetRandomColour()
    {
        // Ensure there are assigned colours to choose from, default to white if there are none
        if (paintColours.Count <= 0) return Color.white;

        int randomIndex = Random.Range(0, paintColours.Count);
        return paintColours[randomIndex];
    }

    #endregion
}
