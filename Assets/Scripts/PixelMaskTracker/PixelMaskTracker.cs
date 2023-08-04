using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ImageAnalysisTest : MonoBehaviour
{
    public Texture imageTexture;
    public Texture maskTexture;
    public ComputeShader pixelCounter;

    [Range(0f, 1f)]
    public float maskClipThreshold;


    // PRIVATE
    private int kernalInitialise;
    private int kernalAtomicComparisonAdd;
    private ComputeBuffer computeBuffer;

    private AsyncGPUReadbackRequest request;
    private bool hasRequested = false;

    [SerializeField]
    private float alphaCoverage = 0f;

    // Update is called once per frame
    void Update()
    {
        RequestImageAnalysis();

        if (request.done)
        {
            hasRequested = false;

            if (request.hasError == false)
            {
                // How many of the image's pixels which have a red channel > maskClipThreshold are covered by the mask? (from 0 - 1)
                alphaCoverage = (float)request.GetData<int>(0)[1] / request.GetData<int>(0)[0];
            }
        }
    }

    // Sends a request to the GPU to compare the two textures
    private void RequestImageAnalysis()
    {
        if (hasRequested) return;
        hasRequested = true;

        kernalInitialise = pixelCounter.FindKernel("Initialise");
        kernalAtomicComparisonAdd = pixelCounter.FindKernel("AtomicComparisonAdd");
        computeBuffer = new ComputeBuffer(2, sizeof(int));

        pixelCounter.SetTexture(kernalAtomicComparisonAdd, "ImageTexture", imageTexture);
        pixelCounter.SetTexture(kernalAtomicComparisonAdd, "MaskTexture", maskTexture);

        pixelCounter.SetFloat("MaskClipThreshold", maskClipThreshold);

        Vector4 inverseMaskDimensions = new Vector4(1f / maskTexture.width, 1f / maskTexture.height, 0f, 0f);
        pixelCounter.SetVector("InverseMaskDimensions", inverseMaskDimensions);

        pixelCounter.SetBuffer(kernalAtomicComparisonAdd, "ResultBuffer", computeBuffer);
        pixelCounter.SetBuffer(kernalInitialise, "ResultBuffer", computeBuffer);

        pixelCounter.Dispatch(kernalInitialise, 1, 1, 1);
        pixelCounter.Dispatch(kernalAtomicComparisonAdd, maskTexture.width / 8, maskTexture.height / 8, 1);

        request = AsyncGPUReadback.Request(computeBuffer);

        computeBuffer.Release();
        computeBuffer = null;
    }
}
