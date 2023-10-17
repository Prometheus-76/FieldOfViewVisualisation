using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class PaintSystem : MonoBehaviour
{
    #region Inspector

    [Header("Shaders")]
    public ComputeShader pixelCounter;
    public Shader surfacePainter;
    public Shader geometryStencil;
    public Shader maskExtension;
    public Shader maskablePaint;

    [Header("Material Properties")]
    public string paintMaskProperty;
    public string eraseThresholdProperty;
    public string paintTextureProperty;
    public string paintColourProperty;
    public string spawnEffectProperty;

    [Header("Object Pooling")]
    public GameObject paintPrefab;
    public bool preventDuplicateReturns = false;

    [Header("Spawning / Despawning")]
    [Min(0.01f)]
    public float spawnEffectDuration;
    [Min(0f)]
    public float despawnFadeDelay;
    [Min(0.01f)]
    public float despawnFadeDuration;

    [Header("Mask Fidelity")]
    [Min(1)]
    public int maskPixelDensity = 32;
    public MathUtilities.PowerOf2 maskResolutionCap = MathUtilities.PowerOf2._1024;

    [Header("Optimisation")]
    public bool boundsCulling = true;
    public bool perimeterCulling = true;

    [Header("Events"), Space(5f)]
    public UnityEvent<float, Color> OnErased;

    #endregion

    // PROPERTIES
    public const float ERASE_THRESHOLD = 0.99f;

    public int paintMaskID { get; private set; } = -1;
    public int eraseThresholdID { get; private set; } = -1;
    public int paintTextureID { get; private set; } = -1;
    public int paintColourID { get; private set; } = -1;
    public int spawnEffectID { get; private set; } = -1;

    public bool isInitialised { get; private set; } = false;

    // PRIVATE
    private LinkedList<MaskablePaint> availablePaint;
    private List<MaskablePaint> allPaint;

    private CommandBuffer commandBuffer = null;

    #region Public Methods

    /// <summary>
    /// Setup the paint system
    /// </summary>
    public void Initialise()
    {
        if (isInitialised) return;

        availablePaint = new LinkedList<MaskablePaint>();
        allPaint = new List<MaskablePaint>();

        commandBuffer = new CommandBuffer();

        // Save material property IDs (better performance)
        paintMaskID = Shader.PropertyToID(paintMaskProperty);
        eraseThresholdID = Shader.PropertyToID(eraseThresholdProperty);
        paintTextureID = Shader.PropertyToID(paintTextureProperty);
        paintColourID = Shader.PropertyToID(paintColourProperty);
        spawnEffectID = Shader.PropertyToID(spawnEffectProperty);

        isInitialised = true;
    }

    /// <summary>
    /// Erase at a position in the world from all paint objects below the brush
    /// </summary>
    /// <param name="brushPosition">World position of the center of the brush</param>
    /// <param name="brushProfile">The brush style to erase with</param>
    public void EraseFromAll(Vector2 brushPosition, BrushProfile brushProfile)
    {
        if (isInitialised == false) Initialise();

        Vector2 brushRelativeToPaint;
        Vector2 halfPaintSize;

        bool eraseCommandSent = false;

        float sqrOuterBrushRadius = (brushProfile.brushInnerRadius + brushProfile.brushOuterOffset);
        sqrOuterBrushRadius *= sqrOuterBrushRadius;

        // Broad -> narrow phase to find all applicable paint objects, erase them as we go
        for (int i = 0; i < allPaint.Count; i++)
        {
            // If active and splattered
            if (allPaint[i].gameObject.activeSelf && allPaint[i].isReset == false)
            {
                float sqrDistanceToPaintCenter = ((Vector2)allPaint[i].paintTransform.position - brushPosition).sqrMagnitude;
                float sqrCircumcircleRadius = allPaint[i].GetSize() * allPaint[i].GetSize() * 0.5f;

                // Circumcircle cull pass
                if (sqrDistanceToPaintCenter <= sqrCircumcircleRadius + sqrOuterBrushRadius)
                {
                    brushRelativeToPaint = allPaint[i].paintTransform.InverseTransformPoint(brushPosition);

                    halfPaintSize.x = allPaint[i].GetSize() / 2f;
                    halfPaintSize.y = halfPaintSize.x;

                    // Bounds cull pass
                    if (boundsCulling == false || MathUtilities.OverlapCircleRect(brushRelativeToPaint, sqrOuterBrushRadius, halfPaintSize))
                    {
                        // Perimeter cull pass
                        if (perimeterCulling == false || MathUtilities.OverlapCirclePolygon(brushRelativeToPaint, sqrOuterBrushRadius, allPaint[i].GetPerimeter(false), false))
                        {
                            allPaint[i].AddEraseCommand(commandBuffer, brushPosition, brushProfile);
                            eraseCommandSent = true;
                        }                        
                    }
                }
            }
        }

        // If the buffer is populated
        if (eraseCommandSent)
        {
            // Then execute the commandbuffer and clear it
            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }
    }

    /// <summary>
    /// Get a paint instance from the open pool, or create one if necessary
    /// </summary>
    /// <param name="parent">The transform which is set as the parent of the retrieved paint object</param>
    /// <param name="paintProfile">The desired paint style</param>
    /// <param name="setActive">Whether the paint object should be active on retrieval</param>
    /// <returns>Reference to the paint script on the returned object</returns>
    public MaskablePaint GetPaint(Transform parent, PaintProfile paintProfile, bool setActive)
    {
        if (isInitialised == false) Initialise();

        MaskablePaint paintInstance;

        // If there is paint available
        if (availablePaint.Count > 0)
        {
            // Remove an item from the open pool
            paintInstance = availablePaint.First.Value;
            availablePaint.RemoveFirst();

            // Configure and return the instance
            paintInstance.SetSize(paintProfile.GetRandomSize());
            paintInstance.SetTexture(paintProfile.GetRandomTexture());
            paintInstance.SetColour(paintProfile.GetRandomColour());
        }
        else
        {
            // Create a new instance instead
            paintInstance = CreatePaint(paintProfile);
        }

        paintInstance.transform.parent = parent;
        paintInstance.gameObject.SetActive(setActive);

        return paintInstance;
    }

    /// <summary>
    /// Returns a paint instance to the open pool
    /// </summary>
    /// <param name="paintInstance">Reference to the script on the paint instance which will be returned</param>
    public void ReturnPaint(MaskablePaint paintInstance)
    {
        if (isInitialised == false) Initialise();

        if (preventDuplicateReturns && availablePaint.Contains(paintInstance)) return;

        // Return it to the pool
        availablePaint.AddLast(paintInstance);

        // Reset if needed
        if (paintInstance.isReset == false) paintInstance.ResetPaint();

        // Store as inactive child
        paintInstance.transform.parent = transform;
        paintInstance.gameObject.SetActive(false);
    }

    #endregion

    private MaskablePaint CreatePaint(PaintProfile paintProfile)
    {
        // Create the paint object
        GameObject paintObject = Instantiate(paintPrefab);
        MaskablePaint paintComponent = paintObject.GetComponent<MaskablePaint>();

        // Add to complete pool
        allPaint.Add(paintComponent);

        // Store as inactive child
        paintObject.transform.parent = transform;
        paintObject.SetActive(false);

        // Initialise this paint
        paintComponent.Initialise(this, paintProfile);

        return paintComponent;
    }

    private void Update() => UpdateAllRemovalDeltas();

    private void UpdateAllRemovalDeltas()
    {
        if (isInitialised == false) Initialise();

        for (int i = 0; i < allPaint.Count; i++)
        {
            MaskablePaint paintInstance = allPaint[i];

            // If active and splattered
            if (paintInstance.gameObject.activeSelf && paintInstance.isReset == false)
            {
                // If this paint object is now ready to despawn, return it immediately and continue
                if (paintInstance.despawnPercent >= 1f)
                {
                    ReturnPaint(paintInstance);
                    continue;
                }

                // Only proceed if paint was actually removed from this instance
                float paintRemoved = paintInstance.ComputeRemovalDelta();
                if (paintRemoved <= 0f) continue;

                Color paintColour = paintInstance.GetColour();

                // Alert event listeners that paint was removed
                OnErased.Invoke(paintRemoved, paintColour);

                // If all the paint has been removed from this object, we can return it immediately
                if (paintInstance.removalPercent >= 1f) ReturnPaint(paintInstance);
            }
        }
    }
}
