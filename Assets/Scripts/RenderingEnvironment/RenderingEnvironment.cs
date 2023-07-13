using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingEnvironment : MonoBehaviour
{
    [Header("Camera")]
    public Camera sceneCamera;
    public Transform sceneCameraTransform;

    [Header("Rendering Mask")]
    public CarvableMesh renderingMaskScript;
    public Transform renderingMaskTransform;

    [Header("Mask Background")]
    public MaskedMaterial backgroundMaterial;
    public Transform maskBackgroundTransform;
    public MeshFilter maskBackgroundFilter;
    public MeshRenderer maskBackgroundRenderer;

    [Header("Mask Foreground")]
    public MaskedMaterial foregroundMaterial;
    public Transform maskForegroundTransform;
    public MeshFilter maskForegroundFilter;
    public MeshRenderer maskForegroundRenderer;

    [Header("Other Configuration")]
    [Range(0, 5000)]
    public int backgroundRenderQueue = 0;
    [Range(0, 5000)]
    public int foregroundRenderQueue = 2000;

    // PRIVATE
    private float previousCameraSize;

    private Mesh maskBoundsMesh;
    private Vector3[] maskBoundsVertices;
    private int[] maskBoundsTriangles;
    private Vector2[] maskBoundsUVs;

    private Material backgroundMaterialInstance = null;
    private Material foregroundMaterialInstance = null;

    // Start is called before the first frame update
    void Start()
    {
        // Lock transforms
        ConstrainEnvironmentTransforms();

        // Configure bounds
        InitialiseBoundsMeshes();

        // Configure mask
        renderingMaskScript.Initialise();
        renderingMaskScript.halfMeshSize = CalculateHalfScreenBounds();

        // Update mask materials
        SetBackgroundMaterial(backgroundMaterial);
        SetForegroundMaterial(foregroundMaterial);
    }

    // LateUpdate is called at the end of every frame
    void LateUpdate()
    {
        // Keep transforms locked into place
        ConstrainEnvironmentTransforms();

        // Find screen size
        Vector2 halfBoundsSize = CalculateHalfScreenBounds();
        renderingMaskScript.halfMeshSize = halfBoundsSize;

        // Update bounds meshes when the camera changes size
        if (sceneCamera.orthographicSize != previousCameraSize) UpdateBoundsMeshes(halfBoundsSize);

        // Update bounds materials
        UpdateBackground();
        UpdateForeground();

        // Regenerate rendering mask
        renderingMaskScript.halfMeshSize = halfBoundsSize;
        renderingMaskScript.UpdateMesh();
    }

    Vector2 CalculateHalfScreenBounds()
    {
        float aspectRatio = (float)Screen.width / Screen.height;

        float halfScreenWidth = aspectRatio * sceneCamera.orthographicSize;
        float halfScreenHeight = sceneCamera.orthographicSize;

        return new Vector2(halfScreenWidth, halfScreenHeight);
    }

    void InitialiseBoundsMeshes()
    {
        maskBoundsMesh = new Mesh();
        maskBoundsMesh.name = "MaskBounds";

        maskBoundsVertices = new Vector3[4];
        maskBoundsTriangles = new int[6];
        maskBoundsUVs = new Vector2[4];

        // Placeholder vertices
        for (int i = 0; i < maskBoundsVertices.Length; i++)
        {
            maskBoundsVertices[i] = Vector3.zero;
        }

        // Set triangle winding order
        maskBoundsTriangles[0] = 0;
        maskBoundsTriangles[1] = 1;
        maskBoundsTriangles[2] = 2;

        maskBoundsTriangles[3] = 2;
        maskBoundsTriangles[4] = 3;
        maskBoundsTriangles[5] = 0;

        // Set UVs
        maskBoundsUVs[0] = Vector2.up;
        maskBoundsUVs[1] = Vector2.one;
        maskBoundsUVs[2] = Vector2.right;
        maskBoundsUVs[3] = Vector2.zero;

        // Set mesh data
        maskBoundsMesh.vertices = maskBoundsVertices;
        maskBoundsMesh.triangles = maskBoundsTriangles;
        maskBoundsMesh.uv = maskBoundsUVs;
    }

    void UpdateBoundsMeshes(Vector2 boundsSize)
    {
        // Calculate vertex positions
        Vector2 topLeft = new Vector2(-boundsSize.x, boundsSize.y);
        Vector2 topRight = new Vector2(boundsSize.x, boundsSize.y);
        Vector2 bottomRight = new Vector2(boundsSize.x, -boundsSize.y);
        Vector2 bottomLeft = new Vector2(-boundsSize.x, -boundsSize.y);

        // Set vertex positions
        maskBoundsVertices[0] = topLeft;
        maskBoundsVertices[1] = topRight;
        maskBoundsVertices[2] = bottomRight;
        maskBoundsVertices[3] = bottomLeft;

        maskBoundsMesh.vertices = maskBoundsVertices;

        // Apply updated mesh to filters
        maskBackgroundFilter.mesh = maskBoundsMesh;
        maskForegroundFilter.mesh = maskBoundsMesh;

        // Update camera size to match
        previousCameraSize = sceneCamera.orthographicSize;
    }

    void ConstrainEnvironmentTransforms()
    {
        // Lock local transforms of rendering mask and bounds meshes
        renderingMaskTransform.localPosition = Vector3.zero;
        maskBackgroundTransform.localPosition = Vector3.zero;
        maskForegroundTransform.localPosition = Vector3.zero;

        renderingMaskTransform.localRotation = Quaternion.identity;
        maskBackgroundTransform.localRotation = Quaternion.identity;
        maskForegroundTransform.localRotation = Quaternion.identity;

        renderingMaskTransform.localScale = Vector3.one;
        maskBackgroundTransform.localScale = Vector3.one;
        maskForegroundTransform.localScale = Vector3.one;
    }

    public void SetBackgroundMaterial(MaskedMaterial newMaterial)
    {
        backgroundMaterial = newMaterial;
        backgroundMaterialInstance = new Material(newMaterial.material);

        backgroundMaterialInstance.renderQueue = backgroundRenderQueue;
        backgroundMaterialInstance.name = newMaterial.material.name + " (Instance)";
    }

    public void SetForegroundMaterial(MaskedMaterial newMaterial)
    {
        foregroundMaterial = newMaterial;
        foregroundMaterialInstance = new Material(newMaterial.material);

        foregroundMaterialInstance.renderQueue = foregroundRenderQueue;
        foregroundMaterialInstance.name = newMaterial.material.name + " (Instance)";
    }

    void UpdateBackground()
    {
        // Set tiling based on scale value and camera size
        if (backgroundMaterialInstance.HasProperty(backgroundMaterial.tilingPropertyReference))
        {
            Vector2 baseTiling = backgroundMaterial.scaleWithCameraSize ? (CalculateHalfScreenBounds() * 2f) : Vector2.one;
            Vector2 scaledTiling = baseTiling * backgroundMaterial.scale;

            backgroundMaterialInstance.SetVector(backgroundMaterial.tilingPropertyReference, scaledTiling);
        }

        // Set offset based on parallax value and camera position
        if (backgroundMaterialInstance.HasProperty(backgroundMaterial.offsetPropertyReference))
        {
            Vector2 parallaxOffset = sceneCameraTransform.position * backgroundMaterial.parallaxStrength * 0.5f;

            backgroundMaterialInstance.SetVector(backgroundMaterial.offsetPropertyReference, parallaxOffset);
        }

        // Ensure render queue is updated
        if (backgroundMaterialInstance.renderQueue != backgroundRenderQueue)
        {
            backgroundMaterialInstance.renderQueue = backgroundRenderQueue;
        }

        // Assign updated material
        maskBackgroundRenderer.material = backgroundMaterialInstance;
    }

    void UpdateForeground()
    {
        // Set tiling based on scale value and camera size
        if (foregroundMaterialInstance.HasProperty(foregroundMaterial.tilingPropertyReference))
        {
            Vector2 baseTiling = foregroundMaterial.scaleWithCameraSize ? (CalculateHalfScreenBounds() * 2f) : Vector2.one;
            Vector2 scaledTiling = baseTiling * foregroundMaterial.scale;

            foregroundMaterialInstance.SetVector(foregroundMaterial.tilingPropertyReference, scaledTiling);
        }

        // Set offset based on parallax value and camera position
        if (foregroundMaterialInstance.HasProperty(foregroundMaterial.offsetPropertyReference))
        {
            Vector2 parallaxOffset = sceneCameraTransform.position * foregroundMaterial.parallaxStrength * 0.5f;

            foregroundMaterialInstance.SetVector(foregroundMaterial.offsetPropertyReference, parallaxOffset);
        }

        // Ensure render queue is updated
        if (foregroundMaterialInstance.renderQueue != foregroundRenderQueue)
        {
            foregroundMaterialInstance.renderQueue = foregroundRenderQueue;
        }

        // Assign updated material
        maskForegroundRenderer.material = foregroundMaterialInstance;
    }
}
