using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class PaintManager : MonoBehaviour
{
    #region Inspector

    [Header("Brush config (PLACEHOLDER)")]
    public BrushProfile brushProfile;

    [Header("Variations")]
    public List<Vector2> paintSizes;
    public List<Texture2D> paintTextures;
    public List<Color> paintColours;

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

    [Header("Object Pooling")]
    public GameObject paintPrefab;
    [Range(0, 100)]
    public int initialPaintReserve = 0;
    public bool preventDuplicateReturns = false;

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

    // PRIVATE
    private LinkedList<MaskablePaint> availablePaint;
    private List<MaskablePaint> allPaint;

    private CommandBuffer commandBuffer = null;

    private void Start()
    {
        Initialise();

        MaskablePaint paint = ExtractFromPool(null, paintSizes[0], paintTextures[0], paintColours[0], true);

        paint.transform.position = new Vector2(6, -3);
        paint.transform.rotation = Quaternion.Euler(0f, 0f, -42);

        paint.Splatter();
    }

    private void Update()
    {
        UpdateAllRemovalDeltas();
        
        if (InputManager.GetControlScheme() == InputManager.ControlScheme.MouseAndKeyboard)
        {
            Vector2 cursorWorldPos = Camera.main.ScreenToWorldPoint(InputManager.GetAimPosition().Value);
            EraseFromAll(cursorWorldPos, brushProfile);
        }
    }

    #region Public Methods

    /// <summary>
    /// Setup the paint manager
    /// </summary>
    public void Initialise()
    {
        availablePaint = new LinkedList<MaskablePaint>();
        allPaint = new List<MaskablePaint>();

        commandBuffer = new CommandBuffer();

        // Save material property IDs (better performance)
        paintMaskID = Shader.PropertyToID(paintMaskProperty);
        eraseThresholdID = Shader.PropertyToID(eraseThresholdProperty);
        paintTextureID = Shader.PropertyToID(paintTextureProperty);
        paintColourID = Shader.PropertyToID(paintColourProperty);

        // Create initial pool size
        for (int i = 0; i < initialPaintReserve; i++)
        {
            ReturnToPool(CreatePaint(Vector2.one, null, Color.clear));
        }
    }

    /// <summary>
    /// Erase at a position in the world from all paint objects below the brush
    /// </summary>
    /// <param name="brushPosition">World position of the center of the brush</param>
    /// <param name="brushProfile">The brush style to erase with</param>
    public void EraseFromAll(Vector2 brushPosition, BrushProfile brushProfile)
    {
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
                float sqrCircumcircleRadius = (allPaint[i].GetSize() / 2f).sqrMagnitude;

                // Circumcircle cull pass
                if (sqrDistanceToPaintCenter <= sqrCircumcircleRadius + sqrOuterBrushRadius)
                {
                    Vector2 brushRelativeToPaint = allPaint[i].paintTransform.InverseTransformPoint(brushPosition);

                    // Bounds cull pass
                    if (boundsCulling == false || MathUtilities.OverlapCircleRect(brushRelativeToPaint, sqrOuterBrushRadius, allPaint[i].GetSize() / 2f))
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
    /// <param name="paintSize">The full local size of the paint object</param>
    /// <param name="paintTexture">The paint texture to apply to this object</param>
    /// <param name="paintColour">The colour to set the paint to</param>
    /// <param name="setActive">Whether the paint object should be active on retrieval</param>
    /// <returns>Reference to the paint script on the returned object</returns>
    public MaskablePaint ExtractFromPool(Transform parent, Vector2 paintSize, Texture2D paintTexture, Color paintColour, bool setActive)
    {
        MaskablePaint paintInstance;

        // If there is paint available
        if (availablePaint.Count > 0)
        {
            // Remove an item from the open pool
            paintInstance = availablePaint.First.Value;
            availablePaint.RemoveFirst();

            // Configure and return the instance
            paintInstance.SetSize(paintSize);
            paintInstance.SetTexture(paintTexture);
            paintInstance.SetColour(paintColour);
        }
        else
        {
            // Create a new instance instead
            paintInstance = CreatePaint(paintSize, paintTexture, paintColour);
        }

        paintInstance.transform.parent = parent;
        paintInstance.gameObject.SetActive(setActive);

        return paintInstance;
    }

    /// <summary>
    /// Returns a paint instance to the open pool
    /// </summary>
    /// <param name="paintToRelease">Reference to the script on the paint instance which will be returned</param>
    public void ReturnToPool(MaskablePaint paintToRelease)
    {
        if (preventDuplicateReturns && availablePaint.Contains(paintToRelease)) return;

        // Return it to the pool
        availablePaint.AddLast(paintToRelease);

        // Store as inactive child
        paintToRelease.transform.parent = transform;
        paintToRelease.gameObject.SetActive(false);
    }

    #endregion

    private MaskablePaint CreatePaint(Vector2 paintSize, Texture2D paintTexture, Color paintColour)
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
        paintComponent.Initialise(this, paintSize, paintTexture, paintColour);

        return paintComponent;
    }

    private void UpdateAllRemovalDeltas()
    {
        for (int i = 0; i < allPaint.Count; i++)
        {
            // If active and splattered
            if (allPaint[i].gameObject.activeSelf && allPaint[i].isReset == false)
            {
                // Only proceed if paint was actually removed from this instance
                float paintRemoved = allPaint[i].ComputeRemovalDelta();
                if (paintRemoved <= 0f) continue;

                Color paintColour = allPaint[i].GetColour();

                // Alert listeners that paint was removed
                OnErased.Invoke(paintRemoved, paintColour);
            }
        }
    }
}
