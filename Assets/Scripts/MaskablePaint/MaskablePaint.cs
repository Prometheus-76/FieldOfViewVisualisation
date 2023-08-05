using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MaskablePaint : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Texture paintTexture;
    public Texture2D brushTexture;
    public Material brushBlendMaterial;
    public CarvableMesh paintMesh;
    public ComputeShader pixelCounter;

    [Header("Configuration")]
    [Range(0f, 0.99f)]
    public float maskClipThreshold;
    [Min(1)]
    public int maskPixelDensity;
    public MathUtilities.PowerOf2 maskResolutionCap;

    #endregion

    // PROPERTIES
    public float removalAmount { get; private set; } = 0f;

    // PRIVATE
    private int kernalInitialise;
    private int kernalAtomicComparisonAdd;
    private ComputeBuffer computeBuffer = null;
    private AsyncGPUReadbackRequest dataRequest;
    private bool dataHasBeenRequested = false;

    private CustomRenderTexture maskTexture = null;

    public void Initialise()
    {
        // How high should the resolution of our mask be?
        int resolution = maskPixelDensity * Mathf.CeilToInt(paintMesh.halfMeshSize.x + paintMesh.halfMeshSize.y);
        resolution = Mathf.Min(1 << (int)maskResolutionCap, MathUtilities.NextHighestPowerOf2(resolution));

        // Setup the mask RenderTexture
        maskTexture = new CustomRenderTexture(resolution, resolution, RenderTextureFormat.RFloat);
        maskTexture.initializationMode = CustomRenderTextureUpdateMode.OnDemand;
        maskTexture.updateMode = CustomRenderTextureUpdateMode.OnDemand;
        maskTexture.initializationColor = Color.clear;

        maskTexture.Initialize();
        paintMesh.Initialise();
    }

    public void Erase(Vector2 worldPosition, Vector2 worldSize, Texture2D brush)
    {
        Vector2 localPosition = transform.InverseTransformPoint(worldPosition); // World units, relative to mesh origin
        Vector2 normalizedPosition = (localPosition / paintMesh.halfMeshSize); // -1 to 1
        Vector2 uvPosition = (normalizedPosition + Vector2.one) / 2f; // 0 to 1

        if (maskTexture != null)
        {
            // Draw to the mask RenderTexture here...
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

    // Sends a request to the GPU to compare the mask/s to the paint texture
    private void RequestMaskAnalysis()
    {
        // Don't submit another request while one is currently pending
        if (dataHasBeenRequested) return;
        dataHasBeenRequested = true;

        // Get function handles in the compute shader, and create a buffer for the execution batches
        kernalInitialise = pixelCounter.FindKernel("Initialise");
        kernalAtomicComparisonAdd = pixelCounter.FindKernel("AtomicComparisonAdd");
        computeBuffer = new ComputeBuffer(2, sizeof(int));

        // Send data to the compute shader
        pixelCounter.SetTexture(kernalAtomicComparisonAdd, "ImageTexture", paintTexture);
        pixelCounter.SetTexture(kernalAtomicComparisonAdd, "MaskTexture", maskTexture);

        pixelCounter.SetFloat("MaskClipThreshold", maskClipThreshold);

        Vector4 inverseMaskDimensions = new Vector4(1f / maskTexture.width, 1f / maskTexture.height, 0f, 0f);
        pixelCounter.SetVector("InverseMaskDimensions", inverseMaskDimensions);

        pixelCounter.SetBuffer(kernalAtomicComparisonAdd, "ResultBuffer", computeBuffer);
        pixelCounter.SetBuffer(kernalInitialise, "ResultBuffer", computeBuffer);

        // Call all batches of the compute shader
        pixelCounter.Dispatch(kernalInitialise, 1, 1, 1);
        pixelCounter.Dispatch(kernalAtomicComparisonAdd, maskTexture.width / 8, maskTexture.height / 8, 1);

        // Submit a request for the results
        dataRequest = AsyncGPUReadback.Request(computeBuffer);

        // Memory cleanup
        computeBuffer.Release();
        computeBuffer = null;
    }
}
