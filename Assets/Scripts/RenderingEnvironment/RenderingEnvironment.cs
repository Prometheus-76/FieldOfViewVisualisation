using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingEnvironment : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;

    [Header("Rendering Mask")]
    public Transform renderingMaskTransform;
    public CarvableMesh renderingMaskScript;

    [Header("Mask Foreground")]
    public Transform maskForegroundTransform;
    public MeshFilter maskForegroundFilter;
    public MeshRenderer maskForegroundRenderer;
    public MaskedMaterial foregroundMaterial;

    [Header("Mask Background")]
    public Transform maskBackgroundTransform;
    public MeshFilter maskBackgroundFilter;
    public MeshRenderer maskBackgroundRenderer;
    public MaskedMaterial backgroundMaterial;

    // PRIVATE
    private float previousCameraSize;

    private Mesh maskBoundsMesh;
    private Vector3[] maskBoundsVertices;
    private int[] maskBoundsTriangles;
    private Vector2[] maskBoundsUVs;

    private Material foregroundMaterialInstance = null;
    private Material backgroundMaterialInstance = null;

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
        SetForegroundMaterial(foregroundMaterial);
        SetBackgroundMaterial(backgroundMaterial);
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
        if (mainCamera.orthographicSize != previousCameraSize) UpdateBoundsMeshes(halfBoundsSize);

        // Update bounds material tiling
        UpdateForegroundTiling();
        UpdateBackgroundTiling();

        // Regenerate rendering mask
        renderingMaskScript.halfMeshSize = halfBoundsSize;
        renderingMaskScript.UpdateMesh();
    }

    Vector2 CalculateHalfScreenBounds()
    {
        float aspectRatio = (float)Screen.width / Screen.height;

        float halfScreenWidth = aspectRatio * mainCamera.orthographicSize;
        float halfScreenHeight = mainCamera.orthographicSize;

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
        maskForegroundFilter.mesh = maskBoundsMesh;
        maskBackgroundFilter.mesh = maskBoundsMesh;

        // Update camera size to match
        previousCameraSize = mainCamera.orthographicSize;
    }

    void ConstrainEnvironmentTransforms()
    {
        // Lock local transforms of rendering mask and bounds meshes
        renderingMaskTransform.localPosition = Vector3.zero;
        maskForegroundTransform.localPosition = Vector3.zero;
        maskBackgroundTransform.localPosition = Vector3.zero;

        renderingMaskTransform.localRotation = Quaternion.identity;
        maskForegroundTransform.localRotation = Quaternion.identity;
        maskBackgroundTransform.localRotation = Quaternion.identity;

        renderingMaskTransform.localScale = Vector3.one;
        maskForegroundTransform.localScale = Vector3.one;
        maskBackgroundTransform.localScale = Vector3.one;
    }

    public void SetForegroundMaterial(MaskedMaterial newMaterial)
    {
        foregroundMaterial = newMaterial;
        foregroundMaterialInstance = new Material(newMaterial.material);
    }

    public void SetBackgroundMaterial(MaskedMaterial newMaterial)
    {
        backgroundMaterial = newMaterial;
        backgroundMaterialInstance = new Material(newMaterial.material);
    }

    void UpdateForegroundTiling()
    {
        // WIP - SET MATERIAL INSTANCE TILING BASED ON BOUNDS SIZE AND SCALE
        // WIP - SET MATERIAL INSTANCE OFFSET BASED ON PARALLAX STRENGTH AND CAMERA POSITION

        maskForegroundRenderer.material = foregroundMaterialInstance;
    }

    void UpdateBackgroundTiling()
    {
        // WIP - SET MATERIAL INSTANCE TILING BASED ON BOUNDS SIZE
        // WIP - SET MATERIAL INSTANCE OFFSET BASED ON PARALLAX STRENGTH AND CAMERA POSITION

        maskBackgroundRenderer.material = backgroundMaterialInstance;
    }
}
