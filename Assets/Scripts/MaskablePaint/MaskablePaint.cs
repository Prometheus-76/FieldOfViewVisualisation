using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MaskablePaint : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Texture paintTexture;
    public Material geometryMaskMaterial;
    public CarvableMesh paintMesh;
    public ComputeShader pixelCounter;
    public Shader surfacePainter;

    [Header("Configuration")]
    [Range(0f, 0.99f)]
    public float maskClipThreshold;
    [Min(1)]
    public int maskPixelDensity;
    public MathUtilities.PowerOf2 maskResolutionCap;

    #endregion

    // PROPERTIES
    public float removalAmount { get; private set; } = 0f;

    public bool isInitialised
    {
        get
        {
            if (eraseBuffer == null) return false;
            if (primaryEraseMask == null) return false;
            if (secondaryEraseMask == null) return false;
            if (geometryMask == null) return false;
            if (maskMaterial == null) return false;
            if (hasSceneRenderingBegun == false) return false;
            if (requestedRenderMaskSetup) return false;
            if (paintMesh.isInitialised == false) return false;

            return true;
        }
    }

    // PRIVATE
    private CommandBuffer eraseBuffer = null;

    private AsyncGPUReadbackRequest dataRequest;
    private bool dataHasBeenRequested = false;

    public RenderTexture primaryEraseMask = null;
    public RenderTexture secondaryEraseMask = null;
    public RenderTexture geometryMask = null;
    private Material maskMaterial = null;

    private bool hasSceneRenderingBegun = false;
    private bool requestedRenderMaskSetup = false;

    private void Start()
    {
        Initialise();
        Splatter();
    }

    // Set up the paint object, we should only need to do this once
    public void Initialise()
    {
        removalAmount = 0f;

        // How high should the resolution of our masks be?
        int resolution = maskPixelDensity * Mathf.CeilToInt(paintMesh.halfMeshSize.x + paintMesh.halfMeshSize.y);
        resolution = Mathf.Min(1 << (int)maskResolutionCap, MathUtilities.NextHighestPowerOf2(resolution));

        // Setup the RenderTexture masks
        primaryEraseMask = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        primaryEraseMask.name = "PrimaryEraseMask";
        secondaryEraseMask = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        secondaryEraseMask.name = "SecondaryEraseMask";
        geometryMask = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
        geometryMask.name = "GeometryMask";

        maskMaterial = new Material(surfacePainter);

        eraseBuffer = new CommandBuffer();

        // Initialise the mesh
        paintMesh.Initialise();
    }

    private void Update()
    {
        if (hasSceneRenderingBegun && requestedRenderMaskSetup) SetupRenderingMasks();

        Vector2 erasePosition = Camera.main.ScreenToWorldPoint(InputManager.GetAimPosition().Value);
        SubmitEraseCommand(erasePosition, 0.2f, 0.4f, 0.5f);

        // Keep this at the END of Update()!
        hasSceneRenderingBegun = true;
    }

    // Resets and resimulates the paint splatter
    public void Splatter()
    {
        // Reset splatter amount
        removalAmount = 0f;

        // Create the mesh shape
        paintMesh.UpdateMesh();

        // Configure the rendering masks
        SetupRenderingMasks();
    }

    // Submits paint erase instructions at a given position
    public void SubmitEraseCommand(Vector2 brushPosition, float brushRadius, float brushHardness, float brushStrength)
    {
        // If paint is ready to use
        if (isInitialised)
        {
            // Configure the brush
            maskMaterial.SetVector("_BrushPosition", brushPosition);
            maskMaterial.SetFloat("_BrushRadius", brushRadius);
            maskMaterial.SetFloat("_BrushHardness", brushHardness);
            maskMaterial.SetFloat("_BrushStrength", brushStrength);

            // Give it the previous canvas
            maskMaterial.SetTexture("_MainTex", secondaryEraseMask);

            // Draw (previous canvas + paint this frame) onto the current canvas
            eraseBuffer.SetRenderTarget(primaryEraseMask);
            eraseBuffer.DrawRenderer(paintMesh.meshRenderer, maskMaterial, 0);

            // Make the current canvas the new previous
            eraseBuffer.SetRenderTarget(secondaryEraseMask);
            eraseBuffer.Blit(primaryEraseMask, secondaryEraseMask);

            // NOTE: Not applied until ExecuteEraseCommands() is called!
            ExecuteEraseCommands();
        }
    }

    // Applies all Erase commands and clears the buffer
    public void ExecuteEraseCommands()
    {
        // If paint is ready to use (ensures buffer and required data exists)
        if (isInitialised)
        {
            Graphics.ExecuteCommandBuffer(eraseBuffer);
            eraseBuffer.Clear();
        }
    }

    // Returns the amount of paint which has been removed since last checking
    public float ComputeRemovalDelta()
    {
        RequestMaskAnalysis();

        if (dataHasBeenRequested && dataRequest.done)
        {
            dataHasBeenRequested = false;

            if (dataRequest.hasError == false)
            {
                // How many of the image's non-transparent pixels are covered by the mask? (from 0 - 1)
                float currentPaint = (float)dataRequest.GetData<int>(0)[1] / dataRequest.GetData<int>(0)[0];
                float removalDelta = Mathf.Max(0f, currentPaint - removalAmount);

                removalAmount = currentPaint;

                return removalDelta;
            }
        }

        return 0f;
    }

    // Set up the rendering masks
    private void SetupRenderingMasks()
    {
        // Primary Erase Mask - Stores the erase mask information
        // Secondary Erase Mask - Stores a copy of the erase mask information
        // Geometry Mask - Stores an edge-to-edge render of the mask geometry to determine if a pixel is on the mesh or not

        // Scene rendering must start before we can render the constructed geometry
        if (hasSceneRenderingBegun == false)
        {
            requestedRenderMaskSetup = true;
            return;
        }

        // Create a command buffer to execute GPU tasks
        CommandBuffer setupBuffer = new CommandBuffer();

        // Clear all buffers
        setupBuffer.SetRenderTarget(primaryEraseMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        setupBuffer.SetRenderTarget(secondaryEraseMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        setupBuffer.SetRenderTarget(geometryMask);
        setupBuffer.ClearRenderTarget(true, true, Color.clear);

        // Render the mesh geometry into the green channel
        setupBuffer.SetRenderTarget(geometryMask);
        RenderGeometryMask(setupBuffer);

        // Execute commands and then clear the buffer
        Graphics.ExecuteCommandBuffer(setupBuffer);
        setupBuffer.Clear();

        // Geometry mask has been baked
        requestedRenderMaskSetup = false;
    }

    // Renders the mesh to the green channel of the mask
    private void RenderGeometryMask(CommandBuffer configuredBuffer)
    {
        // How large is the paint mesh in the world?
        Vector2 meshWorldSize = paintMesh.halfMeshSize * paintMesh.transform.lossyScale;

        // How should the mesh and camera be oriented when rendering the mesh to the geometry mask?
        Matrix4x4 lookMatrix = Matrix4x4.LookAt(transform.position - transform.forward, transform.position + transform.forward, paintMesh.transform.up);
        Matrix4x4 scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, 1f, -1f)); // Z scale inverted because Unity reasons
        Matrix4x4 viewMatrix = scaleMatrix * lookMatrix.inverse;
        Matrix4x4 projectionMatrix = Matrix4x4.Ortho(-meshWorldSize.x, meshWorldSize.x, -meshWorldSize.y, meshWorldSize.y, -1f, 1f);
        configuredBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        // Send the draw command
        configuredBuffer.DrawRenderer(paintMesh.meshRenderer, geometryMaskMaterial);
    }

    // Sends a request to the GPU to compare the mask/s to the paint texture
    private void RequestMaskAnalysis()
    {
        // Don't submit another request while one is currently pending
        if (dataHasBeenRequested) return;
        dataHasBeenRequested = true;

        // Get function handles in the compute shader, and create a buffer for the execution batches
        int kernalInitialise = pixelCounter.FindKernel("Initialise");
        int kernalAtomicComparisonAdd = pixelCounter.FindKernel("AtomicComparisonAdd");

        ComputeBuffer computeBuffer = new ComputeBuffer(2, sizeof(int));

        // Send data to the compute shader
        pixelCounter.SetTexture(kernalAtomicComparisonAdd, "ImageTexture", paintTexture);
        pixelCounter.SetTexture(kernalAtomicComparisonAdd, "MaskTexture", primaryEraseMask);

        pixelCounter.SetFloat("MaskClipThreshold", maskClipThreshold);

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
