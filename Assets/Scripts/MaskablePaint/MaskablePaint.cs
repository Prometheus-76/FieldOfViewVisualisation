using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MaskablePaint : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public CarvableMesh carvableMesh;
    public MeshRenderer paintMeshRenderer;

    [Header("Shaders")]
    public ComputeShader pixelCounter;
    public Shader surfacePainter;
    public Shader geometryStencil;

    [Header("Configuration")]
    [Range(0f, 0.99f)]
    public float maskClipMinThreshold = 0.99f;
    [Min(1)]
    public int maskPixelDensity = 32;
    public MathUtilities.PowerOf2 maskResolutionCap = MathUtilities.PowerOf2._1024;

    #endregion

    // PROPERTIES
    public float removalPercent { get; private set; } = 0f;
    public bool removalInProgress { get; private set; } = false;
    public bool isReset { get; private set; } = true;

    public bool isInitialised
    {
        get
        {
            if (primaryEraseMask == null) return false;
            if (secondaryEraseMask == null) return false;
            if (maskPaintingMaterial == null) return false;
            if (maskStencilMaterial == null) return false;
            if (paintMaterialInstance == null) return false;
            if (carvableMesh.isInitialised == false) return false;

            return true;
        }
    }

    // PRIVATE
    private float previousErasedPixels = 0;
    private AsyncGPUReadbackRequest dataRequest;

    private RenderTexture primaryEraseMask = null;
    private RenderTexture secondaryEraseMask = null;
    public RenderTexture geometryMask = null;
    private Material maskPaintingMaterial = null;
    private Material maskStencilMaterial = null;

    private Material paintMaterialInstance = null;

    private Color paintMaterialColour = Color.clear;
    private Texture paintMaterialTexture = null;
    private int paintColourID = -1;
    private int paintTextureID = -1;

    // Set up the paint object, we should only need to do this once
    public void Initialise(Material newMaterial, string colourPropertyName, string texturePropertyName)
    {
        removalPercent = 0f;

        // How high should the resolution of our masks be?
        int resolution = maskPixelDensity * Mathf.CeilToInt(carvableMesh.halfMeshSize.x + carvableMesh.halfMeshSize.y);
        resolution = Mathf.Min(1 << (int)maskResolutionCap, MathUtilities.NextHighestPowerOf2(resolution));

        // Setup the RenderTexture masks
        primaryEraseMask = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        primaryEraseMask.name = "PrimaryEraseMask";
        secondaryEraseMask = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        secondaryEraseMask.name = "SecondaryEraseMask";
        geometryMask = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        geometryMask.name = "GeometryMask";
        ClearMasks();

        maskPaintingMaterial = new Material(surfacePainter);
        maskStencilMaterial = new Material(geometryStencil);
        SetMaterial(newMaterial, colourPropertyName, texturePropertyName);

        // Initialise the carvable mesh
        carvableMesh.Initialise();

        isReset = true;
    }

    // Simulates the paint splatter
    public void Splatter()
    {
        if (isReset == false) ResetPaint();

        // Regenerate the mesh shape
        carvableMesh.UpdateMesh();

        // Bake the geometry mask
        BakeGeometryMask();

        isReset = false;
    }

    // Reset the paint splatter
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

    // Submits paint erase instructions at a given position
    public void AddEraseCommand(CommandBuffer commandBuffer, Vector2 brushPosition, float brushRadius, float brushHardness, float brushStrength)
    {
        // If paint is ready to use
        if (isInitialised)
        {
            // Configure the brush
            maskPaintingMaterial.SetVector("_BrushPosition", brushPosition);
            maskPaintingMaterial.SetFloat("_BrushRadius", brushRadius);
            maskPaintingMaterial.SetFloat("_BrushHardness", brushHardness);
            maskPaintingMaterial.SetFloat("_BrushStrength", brushStrength);

            // Give it the previous canvas
            maskPaintingMaterial.SetTexture("_MainTex", secondaryEraseMask);

            // Draw (previous canvas + paint this frame) onto the current canvas
            commandBuffer.SetRenderTarget(primaryEraseMask);
            commandBuffer.DrawRenderer(carvableMesh.meshRenderer, maskPaintingMaterial, 0);

            // Make the current canvas the new previous
            commandBuffer.SetRenderTarget(secondaryEraseMask);
            commandBuffer.Blit(primaryEraseMask, secondaryEraseMask);
        }
    }

    // Returns the amount of paint which has been removed since last checking
    public float ComputeRemovalDelta()
    {
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
                float removalDelta = Mathf.Max(0, currentErasedPixels - previousErasedPixels);
                previousErasedPixels = currentErasedPixels;

                return removalDelta;
            }
        }

        return 0f;
    }

    // Sets a new material for this paint object (and resets it)
    public void SetMaterial(Material newMaterial, string texturePropertyName, string colourPropertyName)
    {
        ResetPaint();

        // Get the shader variable hooks
        paintTextureID = Shader.PropertyToID(texturePropertyName);
        paintColourID = Shader.PropertyToID(colourPropertyName);

        // Create a new instance of this material
        paintMaterialInstance = Instantiate(newMaterial);
        paintMaterialTexture = newMaterial.GetTexture(paintTextureID);

        if (paintMaterialColour != Color.clear)
        {
            SetColour(paintMaterialColour);
        }

        // Apply new material
        paintMeshRenderer.material = paintMaterialInstance;
    }

    // Set the colour of the material of this paint object
    public void SetColour(Color newColour)
    {
        paintMaterialColour = newColour;

        // If there is a material instantiated already, assign the colour
        if (paintMaterialInstance != null)
        {
            paintMaterialInstance.SetColor(paintColourID, paintMaterialColour);
        }
    }

    // Bake the shape of the mesh into the geometry mask texture
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

    // Clear the rendering masks
    private void ClearMasks()
    {
        // Create a command buffer to execute GPU tasks
        CommandBuffer setupBuffer = new CommandBuffer();

        // Clear all buffers
        setupBuffer.SetRenderTarget(primaryEraseMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        setupBuffer.SetRenderTarget(secondaryEraseMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        setupBuffer.SetRenderTarget(geometryMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        // Execute commands and then clear the buffer
        Graphics.ExecuteCommandBuffer(setupBuffer);
        setupBuffer.Clear();
    }

    // Sends a request to the GPU to compare the mask/s to the paint texture
    private void RequestMaskAnalysis()
    {
        // Don't submit another request while one is currently pending
        if (removalInProgress) return;
        removalInProgress = true;

        ComputeBuffer computeBuffer = new ComputeBuffer(2, sizeof(int));
        
        // Get function handles in the compute shader, and create a buffer for the execution batches
        int kernalInitialise = pixelCounter.FindKernel("Initialise");
        int kernalAtomicComparisonAdd = pixelCounter.FindKernel("AtomicComparisonAdd");

        // Send data to the compute shader
        pixelCounter.SetTexture(kernalAtomicComparisonAdd, "ImageTexture", paintMaterialTexture);
        pixelCounter.SetTexture(kernalAtomicComparisonAdd, "PaintMaskTexture", primaryEraseMask);

        pixelCounter.SetFloat("MaskClipThreshold", maskClipMinThreshold);

        Vector4 inverseMaskDimensions = new Vector4(1f / primaryEraseMask.width, 1f / primaryEraseMask.height, 0f, 0f);
        pixelCounter.SetVector("InverseMaskDimensions", inverseMaskDimensions);

        pixelCounter.SetBuffer(kernalAtomicComparisonAdd, "ResultBuffer", computeBuffer);
        pixelCounter.SetBuffer(kernalInitialise, "ResultBuffer", computeBuffer);

        // Call all batches of the compute shader
        pixelCounter.Dispatch(kernalInitialise, 1, 1, 1);
        pixelCounter.Dispatch(kernalAtomicComparisonAdd, primaryEraseMask.width / 8, primaryEraseMask.height / 8, 1);

        // Submit a request for the results
        dataRequest = AsyncGPUReadback.Request(computeBuffer);

        // Memory cleanup
        computeBuffer.Release();
    }
}
