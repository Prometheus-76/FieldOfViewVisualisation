using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PaintManager : MonoBehaviour
{
    #region Inspector

    [Header("Brush config (PLACEHOLDER)")]
    public float bInnerRadius;
    public float bOuterRadius;
    public Texture2D bTexture;
    public float bTextureStrength;
    public float bTextureScale;

    [Header("Configuration")]
    public GameObject paintPrefab;

    [Header("Material Properties")]
    public string paintMaskProperty;
    public string eraseThresholdProperty;
    public string paintTextureProperty;
    public string paintColourProperty;

    [Header("Mask Fidelity")]
    [Min(1)]
    public int maskPixelDensity = 32;
    public MathUtilities.PowerOf2 maskResolutionCap = MathUtilities.PowerOf2._1024;

    [Header("Variations")]
    public List<Texture2D> paintTextures;
    public List<Color> paintColours;

    [Header("Object Pooling")]
    [Range(0, 100)]
    public int initialPaintReserve = 0;
    public bool preventDuplicateReturns = false;

    [Header("Shaders")]
    public ComputeShader pixelCounter;
    public Shader surfacePainter;
    public Shader geometryStencil;
    public Shader maskExtension;
    public Shader maskablePaint;

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

    private float totalRemoved = 0f;

    private void Start()
    {
        Initialise();

        MaskablePaint paint = ExtractFromPool();
        paint.gameObject.SetActive(true);
        paint.transform.parent = null;
        paint.transform.position = new Vector2(6, -3);
        paint.transform.rotation = Quaternion.Euler(0f, 0f, -42);
        paint.Splatter();
    }

    private void Update()
    {
        Vector2 cursorWorldPos = Camera.main.ScreenToWorldPoint(InputManager.GetAimPosition().Value);
        EraseFromAll(cursorWorldPos, bInnerRadius, bOuterRadius, bTexture, bTextureStrength, bTextureScale);

        totalRemoved += allPaint[0].ComputeRemovalDelta();
    }

    // Setup the paint manager
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
            ReturnToPool(CreatePaint());
        }
    }

    // Erase at a position in the world from all paint objects below the brush
    public void EraseFromAll(Vector2 brushPosition, float brushInnerRadius, float brushOuterRadius, Texture2D brushTexture, float brushTextureStrength, float brushTextureScale)
    {
        // Broad -> narrow phase to find all applicable paint objects, erase them as we go
        int eraseCount = 0;
        for (int i = 0; i < allPaint.Count; i++)
        {
            // If active and splattered
            if (allPaint[i].gameObject.activeSelf && allPaint[i].isReset == false)
            {
                // If the distance to the bounding box is less than the brush radius...
                if (true)
                {
                    // Check if brush radius overlaps with shape perimeter as well?
                    if (true)
                    {
                        allPaint[i].AddEraseCommand(commandBuffer, brushPosition, brushInnerRadius, brushOuterRadius, brushTexture, brushTextureStrength, brushTextureScale);
                        eraseCount += 1;
                    }
                }
            }
        }

        if (eraseCount > 0)
        {
            // Then execute the commandbuffer and clear it
            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }
    }

    // Get a paint instance from the open pool, creating one if necessary
    public MaskablePaint ExtractFromPool()
    {
        // If there is paint available
        if (availablePaint.Count > 0)
        {
            // Remove and return an item from the open pool
            MaskablePaint paintInstance = availablePaint.First.Value;
            availablePaint.RemoveFirst();

            return paintInstance;
        }

        // Create a new instance and return that instead
        return CreatePaint();
    }

    // Returns a paint instance to the open pool
    public void ReturnToPool(MaskablePaint paintToRelease)
    {
        if (preventDuplicateReturns && availablePaint.Contains(paintToRelease)) return;

        // Return it to the pool
        availablePaint.AddLast(paintToRelease);

        // Store as inactive child
        paintToRelease.transform.parent = transform;
        paintToRelease.gameObject.SetActive(false);
    }

    // Creates a new paint object and return it
    private MaskablePaint CreatePaint()
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
        paintComponent.Initialise(this, paintTextures[0], paintColours[0]);

        return paintComponent;
    }
}
