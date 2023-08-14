using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MaskablePaint : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Transform paintTransform;
    [SerializeField]
    private CarvableMesh carvableMesh;
    public MeshRenderer paintMeshRenderer;

    #endregion

    // PROPERTIES
    public float removalPercent { get; private set; } = 0f;
    public bool removalInProgress { get; private set; } = false;
    public bool maskModifiedSinceLastCheck { get; private set; } = false;
    public bool isReset { get; private set; } = true;

    public bool isInitialised
    {
        get
        {
            if (primaryEraseMask == null) return false;
            if (secondaryEraseMask == null) return false;
            if (maskPaintingMaterial == null) return false;
            if (maskStencilMaterial == null) return false;
            if (maskExtensionMaterial == null) return false;
            if (paintMaterialInstance == null) return false;
            if (carvableMesh.isInitialised == false) return false;

            return true;
        }
    }

    // PRIVATE
    private int previousErasedPixels = 0;
    private AsyncGPUReadbackRequest dataRequest;

    private RenderTexture primaryEraseMask = null;
    private RenderTexture secondaryEraseMask = null;
    private RenderTexture outputEraseMask = null;
    private RenderTexture geometryMask = null;
    private Material maskPaintingMaterial = null;
    private Material maskStencilMaterial = null;
    private Material maskExtensionMaterial = null;

    private ComputeShader pixelCounter = null;
    private Material paintMaterialInstance = null;

    private float paintSize = 0f;
    private Texture paintTexture = null;
    private Color paintColour = Color.clear;

    private int maskPixelDensity = -1;
    private int maskResolutionCap = -1;

    private int paintMaskID = -1;
    private int eraseThresholdID = -1;
    private int paintTextureID = -1;
    private int paintColourID = -1;

    #region Public Methods

    /// <summary>
    /// Set up the paint object, we should only need to do this once when it's created
    /// </summary>
    /// <param name="paintManager">The paint manager instance which this paint is being initialised from</param>
    /// <param name="paintProfile">The style of paint this object should be initialised to</param>
    public void Initialise(PaintManager paintManager, PaintProfile paintProfile)
    {
        // Ensure erase data is marked as reset
        previousErasedPixels = 0;
        removalPercent = 0f;
        isReset = true;

        // Set compute shader
        pixelCounter = paintManager.pixelCounter;

        // Save values from PaintManager
        maskPixelDensity = paintManager.maskPixelDensity;
        maskResolutionCap = 1 << (int)paintManager.maskResolutionCap;

        // Get the shader variable hooks
        paintMaskID = paintManager.paintMaskID;
        eraseThresholdID = paintManager.eraseThresholdID;
        paintTextureID = paintManager.paintTextureID;
        paintColourID = paintManager.paintColourID;

        // Initialise the RenderTexture masks
        primaryEraseMask = new RenderTexture(0, 0, 0, RenderTextureFormat.RFloat);
        primaryEraseMask.filterMode = FilterMode.Bilinear;
        primaryEraseMask.name = "PrimaryEraseMask";

        secondaryEraseMask = new RenderTexture(0, 0, 0, RenderTextureFormat.RFloat);
        secondaryEraseMask.filterMode = FilterMode.Bilinear;
        secondaryEraseMask.name = "SecondaryEraseMask";

        outputEraseMask = new RenderTexture(0, 0, 0, RenderTextureFormat.RFloat);
        outputEraseMask.filterMode = FilterMode.Bilinear;
        outputEraseMask.name = "OutputEraseMask";

        geometryMask = new RenderTexture(0, 0, 0, RenderTextureFormat.RFloat);
        geometryMask.filterMode = FilterMode.Point;
        geometryMask.name = "GeometryMask";

        // Set paint properties
        SetSize(paintProfile.GetRandomSize());
        SetTexture(paintProfile.GetRandomTexture());
        SetColour(paintProfile.GetRandomColour());
        ClearMasks();

        // Create material instances
        maskPaintingMaterial = new Material(paintManager.surfacePainter);
        maskStencilMaterial = new Material(paintManager.geometryStencil);
        maskExtensionMaterial = new Material(paintManager.maskExtension);

        paintMaterialInstance = new Material(paintManager.maskablePaint);

        // Attach material properties
        paintMaterialInstance.SetTexture(paintMaskID, outputEraseMask);
        paintMaterialInstance.SetFloat(eraseThresholdID, PaintManager.ERASE_THRESHOLD);

        // Apply material
        paintMeshRenderer.material = paintMaterialInstance;

        // Initialise the carvable mesh
        carvableMesh.Initialise();
    }

    /// <summary>
    /// Simulates the paint splatter, creating the mesh shape and baking it to the geometry mask
    /// </summary>
    public void Splatter()
    {
        if (isReset == false) ResetPaint();

        // Regenerate the mesh shape
        carvableMesh.GenerateMesh();

        // Bake the geometry mask
        BakeGeometryMask();

        isReset = false;
    }

    /// <summary>
    /// Reset the paint splatter, and all associated data for it
    /// </summary>
    public void ResetPaint()
    {
        if (isInitialised == false) return;

        // Reset previous erase data
        previousErasedPixels = 0;
        removalPercent = 0f;
        ClearMasks();

        // Ignore pending GPU jobs
        removalInProgress = false;

        isReset = true;
    }

    /// <summary>
    /// Sets a new material for this paint object (and resets it)
    /// </summary>
    /// <param name="newTexture">The new texture to assign</param>
    public void SetTexture(Texture2D newTexture)
    {
        // Only update if necessary
        if (newTexture == paintTexture) return;
        paintTexture = newTexture;

        // Reset if required
        if (isReset == false) ResetPaint();

        paintMaterialInstance.SetTexture(paintTextureID, newTexture);
    }

    /// <summary>
    /// Set the colour of the material of this paint object
    /// </summary>
    /// <param name="newColour">The new colour to set for this paint object</param>
    public void SetColour(Color newColour)
    {
        // Only update if necessary
        if (paintColour == newColour) return;
        paintColour = newColour;

        // If there is a material instantiated already, assign the colour
        if (paintMaterialInstance != null)
        {
            paintMaterialInstance.SetColor(paintColourID, paintColour);
        }
    }

    /// <summary>
    /// Get the current paint colour
    /// </summary>
    /// <returns>The paint colour currently set, including the alpha</returns>
    public Color GetColour()
    {
        return paintColour;
    }

    /// <summary>
    /// Set the alpha of the material of this paint object
    /// </summary>
    /// <param name="newAlpha">The new alpha of the paint colour</param>
    public void SetAlpha(float newAlpha)
    {
        // Only update if necessary
        if (paintColour.a == newAlpha) return;

        // We can't write to paintColour yet, otherwise it will appear as though the colour does not need changing
        Color updatedColour = paintColour;
        updatedColour.a = newAlpha;

        SetColour(updatedColour);
    }

    /// <summary>
    /// Set the size of the mesh for this paint object, resets it, and updates the render textures
    /// </summary>
    /// <param name="newSize">The full local size of each side of the paint object</param>
    public void SetSize(float newSize)
    {
        // Ensure we only update if necessary
        if (newSize == paintSize) return;
        paintSize = newSize;

        // Reset if required
        if (isReset == false) ResetPaint();

        // Apply to mesh size
        carvableMesh.halfMeshSize.x = paintSize / 2f;
        carvableMesh.halfMeshSize.y = carvableMesh.halfMeshSize.x;

        // How high should the resolution of our masks be?
        int sideResolution = Mathf.CeilToInt(paintSize * maskPixelDensity);
        sideResolution = Mathf.Min(maskResolutionCap, MathUtilities.NextHighestPowerOf2(sideResolution));

        // Set the resolution of the masks
        primaryEraseMask.Release();
        primaryEraseMask.height = sideResolution;
        primaryEraseMask.width = sideResolution;

        secondaryEraseMask.Release();
        secondaryEraseMask.height = sideResolution;
        secondaryEraseMask.width = sideResolution;

        outputEraseMask.Release();
        outputEraseMask.height = sideResolution;
        outputEraseMask.width = sideResolution;

        geometryMask.Release();
        geometryMask.height = sideResolution;
        geometryMask.width = sideResolution;
    }

    /// <summary>
    /// Get the current paint size
    /// </summary>
    /// <returns>The full paint size</returns>
    public float GetSize()
    {
        return paintSize;
    }

    /// <summary>
    /// Return the perimeter of the mesh object
    /// </summary>
    /// <param name="returnCopy">Whether the array should be returned as a copy or a reference (unsafe but faster)</param>
    /// <returns>Array of vectors representing the mesh vertices in local space</returns>
    public Vector2[] GetPerimeter(bool returnCopy)
    {
        return carvableMesh.GetPerimeter(returnCopy);
    }

    /// <summary>
    /// Submits paint erase instructions at a given position
    /// </summary>
    /// <param name="commandBuffer">The command buffer to which the erase command will be added</param>
    /// <param name="brushPosition">The world position of the center of the brush</param>
    /// <param name="brushProfile">The style of brush to erase with</param>
    public void AddEraseCommand(CommandBuffer commandBuffer, Vector2 brushPosition, BrushProfile brushProfile)
    {
        // If paint is ready to use
        if (isInitialised)
        {
            // Configure the brush
            maskPaintingMaterial.SetVector("_BrushPosition", brushPosition);
            maskPaintingMaterial.SetFloat("_BrushInnerRadius", brushProfile.brushInnerRadius);
            maskPaintingMaterial.SetFloat("_BrushOuterRadius", brushProfile.brushInnerRadius + brushProfile.brushOuterOffset);
            maskPaintingMaterial.SetTexture("_BrushTexture", brushProfile.brushTexture);
            maskPaintingMaterial.SetFloat("_BrushTextureStrength", brushProfile.brushTextureStrength);
            maskPaintingMaterial.SetFloat("_BrushTextureScale", brushProfile.brushTextureScale);

            maskPaintingMaterial.SetTexture("_MainTex", primaryEraseMask);

            // Combine the primary mask and the brush onto the secondary mask
            commandBuffer.SetRenderTarget(secondaryEraseMask);
            commandBuffer.DrawRenderer(carvableMesh.meshRenderer, maskPaintingMaterial, 0);

            // Copy the combined brush and mask back onto the primary mask
            commandBuffer.SetRenderTarget(primaryEraseMask);
            commandBuffer.Blit(secondaryEraseMask, primaryEraseMask);

            // Extend the mask by one pixel to account for the rasterizer cutting it off around the edges of the carved mesh
            commandBuffer.SetRenderTarget(outputEraseMask);
            maskExtensionMaterial.SetTexture("_MainTex", primaryEraseMask);
            commandBuffer.Blit(secondaryEraseMask, outputEraseMask, maskExtensionMaterial);

            maskModifiedSinceLastCheck = true;
        }
    }

    /// <summary>
    /// Returns the amount of paint which has been removed since last checking
    /// </summary>
    /// <returns>The amount of paint which has been removed (in world-space area)</returns>
    public float ComputeRemovalDelta()
    {
        // Don't check if no changes have been made to the mask
        if (maskModifiedSinceLastCheck == false) return 0f;

        RequestMaskAnalysis();

        // When the current request is complete
        if (dataRequest.done)
        {
            removalInProgress = false;

            if (dataRequest.hasError == false)
            {
                // How many of the image's non-transparent pixels are covered by the mask? (from 0 - 1)
                int totalErasablePixels = dataRequest.GetData<int>(0)[0];
                int currentErasedPixels = dataRequest.GetData<int>(0)[1];
                
                // How many of the removable pixels have been removed?
                removalPercent = Mathf.Clamp01((float)currentErasedPixels / totalErasablePixels);

                // How many pixels have been removed since last checking?
                int removalDelta = Mathf.Max(0, currentErasedPixels - previousErasedPixels);
                previousErasedPixels = currentErasedPixels;

                // Convert from pixel count -> world space area
                float texelArea = outputEraseMask.texelSize.x * outputEraseMask.texelSize.y; // texelSize = (1 / textureResolution)

                Vector2 meshDimensions = carvableMesh.halfMeshSize * 2f;
                float meshArea = meshDimensions.x * meshDimensions.y;

                maskModifiedSinceLastCheck = false;
                return (removalDelta * texelArea * meshArea);
            }
        }

        return 0f;
    }

    #endregion

    private void RequestMaskAnalysis()
    {
        // Don't submit another request while one is currently pending
        if (removalInProgress) return;
        removalInProgress = true;

        ComputeBuffer computeBuffer = new ComputeBuffer(2, sizeof(int));
        
        // Get function handles in the compute shader, and create a buffer for the execution batches
        int kernalInitialise = pixelCounter.FindKernel("Initialise");
        int kernalComputePixelCounts = pixelCounter.FindKernel("ComputePixelCounts");

        // Send data to the compute shader
        pixelCounter.SetTexture(kernalComputePixelCounts, "PaintTexture", paintTexture);
        pixelCounter.SetTexture(kernalComputePixelCounts, "PaintMaskTexture", outputEraseMask);
        pixelCounter.SetTexture(kernalComputePixelCounts, "GeometryMaskTexture", geometryMask);

        pixelCounter.SetFloat("MaskClipThreshold", PaintManager.ERASE_THRESHOLD);

        Vector4 inverseMaskDimensions = new Vector4(1f / primaryEraseMask.width, 1f / primaryEraseMask.height, 0f, 0f);
        pixelCounter.SetVector("InverseMaskDimensions", inverseMaskDimensions);

        pixelCounter.SetBuffer(kernalComputePixelCounts, "ResultBuffer", computeBuffer);
        pixelCounter.SetBuffer(kernalInitialise, "ResultBuffer", computeBuffer);

        // Call all batches of the compute shader
        pixelCounter.Dispatch(kernalInitialise, 1, 1, 1);
        pixelCounter.Dispatch(kernalComputePixelCounts, primaryEraseMask.width / 8, primaryEraseMask.height / 8, 1);

        // Submit a request for the results
        dataRequest = AsyncGPUReadback.Request(computeBuffer);

        // Memory cleanup
        computeBuffer.Release();
    }

    private void BakeGeometryMask()
    {
        // Create a command buffer to execute GPU tasks
        CommandBuffer bakeBuffer = new CommandBuffer();

        // Draw (previous canvas + paint this frame) onto the current canvas
        bakeBuffer.SetRenderTarget(geometryMask);
        bakeBuffer.DrawRenderer(carvableMesh.meshRenderer, maskStencilMaterial, 0);

        // Execute commands and then clear the buffer
        Graphics.ExecuteCommandBuffer(bakeBuffer);
        bakeBuffer.Clear();
    }

    private void ClearMasks()
    {
        // Create a command buffer to execute GPU tasks
        CommandBuffer setupBuffer = new CommandBuffer();

        // Clear all buffers
        setupBuffer.SetRenderTarget(primaryEraseMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        setupBuffer.SetRenderTarget(secondaryEraseMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        setupBuffer.SetRenderTarget(outputEraseMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        setupBuffer.SetRenderTarget(geometryMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        // Execute commands and then clear the buffer
        Graphics.ExecuteCommandBuffer(setupBuffer);
        setupBuffer.Clear();
    }
}
