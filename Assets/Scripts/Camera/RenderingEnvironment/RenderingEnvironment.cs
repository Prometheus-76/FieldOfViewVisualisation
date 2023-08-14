using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingEnvironment : MonoBehaviour
{
    [Header("Camera")]
    public Camera sceneCamera;

    [Header("Rendering Mask")]
    public Shader renderingMaskShader;
    public CarvableMesh renderingMaskScript;
    public Transform renderingMaskTransform;
    public MeshRenderer renderingMaskRenderer;

    [Header("Mask Background")]
    public Material backgroundMaterial;
    public Transform maskBackgroundTransform;
    public MeshFilter maskBackgroundFilter;
    public MeshRenderer maskBackgroundRenderer;

    [Header("Mask Foreground")]
    public Material foregroundMaterial;
    public Transform maskForegroundTransform;
    public MeshFilter maskForegroundFilter;
    public MeshRenderer maskForegroundRenderer;

    // PRIVATE
    private float previousCameraSize;

    private Mesh maskBoundsMesh;
    private Vector3[] maskBoundsVertices;
    private int[] maskBoundsTriangles;
    private Vector2[] maskBoundsUVs;

    private Material renderingMaskMaterialInstance = null;
    private Material backgroundMaterialInstance = null;
    private Material foregroundMaterialInstance = null;

    // Start is called before the first frame update
    private void Start()
    {
        // Lock transforms
        ConstrainEnvironmentTransforms();

        // Configure bounds
        InitialiseBoundsMeshes();

        // Configure mask
        renderingMaskScript.Initialise();
        renderingMaskScript.halfMeshSize = CalculateHalfScreenBounds();
        renderingMaskMaterialInstance = new Material(renderingMaskShader);
        renderingMaskRenderer.material = renderingMaskMaterialInstance;

        // Update mask materials
        SetBackgroundMaterial(backgroundMaterial);
        SetForegroundMaterial(foregroundMaterial);
    }

    // LateUpdate is called at the end of every frame
    private void LateUpdate()
    {
        // Keep transforms locked into place
        ConstrainEnvironmentTransforms();

        // Find screen size
        Vector2 halfBoundsSize = CalculateHalfScreenBounds();
        renderingMaskScript.halfMeshSize = halfBoundsSize;

        // Update bounds meshes when the camera changes size
        if (sceneCamera.orthographicSize != previousCameraSize) UpdateBoundsMeshes(halfBoundsSize);

        // Regenerate rendering mask
        renderingMaskScript.halfMeshSize = halfBoundsSize;
        renderingMaskScript.GenerateMesh();
    }

    public void SetBackgroundMaterial(Material newMaterial)
    {
        backgroundMaterial = newMaterial;
        backgroundMaterialInstance = new Material(newMaterial);

        backgroundMaterialInstance.name = newMaterial.name + " (Instance)";
        maskBackgroundRenderer.material = backgroundMaterialInstance;
    }

    public void SetForegroundMaterial(Material newMaterial)
    {
        foregroundMaterial = newMaterial;
        foregroundMaterialInstance = new Material(newMaterial);

        foregroundMaterialInstance.name = newMaterial.name + " (Instance)";
        maskForegroundRenderer.material = foregroundMaterialInstance;
    }

    private Vector2 CalculateHalfScreenBounds()
    {
        float aspectRatio = (float)Screen.width / Screen.height;

        float halfScreenWidth = aspectRatio * sceneCamera.orthographicSize;
        float halfScreenHeight = sceneCamera.orthographicSize;

        return new Vector2(halfScreenWidth, halfScreenHeight);
    }

    private void InitialiseBoundsMeshes()
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

    private void UpdateBoundsMeshes(Vector2 boundsSize)
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

    private void ConstrainEnvironmentTransforms()
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
}
