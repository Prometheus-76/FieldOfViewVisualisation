using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingEnvironment : MonoBehaviour
{
    [Header("Configuration")]
    public Camera sceneCamera;
    public Transform targetTransform;

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
    private Mesh maskBoundsMesh;
    private Vector3[] maskBoundsVertices;
    private int[] maskBoundsTriangles;
    private Vector2[] maskBoundsUVs;

    private Material renderingMaskMaterialInstance = null;
    private Material backgroundMaterialInstance = null;
    private Material foregroundMaterialInstance = null;

    private Transform environmentTransform = null;
    private Transform cameraTransform = null;

    // Start is called before the first frame update
    private void Start()
    {
        // Get references
        environmentTransform = transform;
        cameraTransform = sceneCamera.transform;

        // Lock transforms
        ConstrainEnvironmentTransforms();

        // Configure bounds
        InitialiseBoundsMeshes();

        // Configure mask
        renderingMaskScript.Initialise();
        renderingMaskScript.halfMeshSize = CalculateHalfEnvironmentBounds(sceneCamera.orthographicSize, cameraTransform.eulerAngles.z, cameraTransform.position - targetTransform.position);
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

        // Find bounding box for camera projection
        Vector2 halfEnvironmentBounds = CalculateHalfEnvironmentBounds(sceneCamera.orthographicSize, cameraTransform.eulerAngles.z, cameraTransform.position - targetTransform.position);

        // Regenerate rendering mask
        renderingMaskScript.halfMeshSize = halfEnvironmentBounds;
        renderingMaskScript.GenerateMesh();

        // Update bounds meshes
        UpdateBoundsMeshes(halfEnvironmentBounds);
    }

    private void SetBackgroundMaterial(Material newMaterial)
    {
        backgroundMaterial = newMaterial;
        backgroundMaterialInstance = new Material(newMaterial);

        backgroundMaterialInstance.name = newMaterial.name + " (Instance)";
        maskBackgroundRenderer.material = backgroundMaterialInstance;
    }

    private void SetForegroundMaterial(Material newMaterial)
    {
        foregroundMaterial = newMaterial;
        foregroundMaterialInstance = new Material(newMaterial);

        foregroundMaterialInstance.name = newMaterial.name + " (Instance)";
        maskForegroundRenderer.material = foregroundMaterialInstance;
    }

    private Vector2 CalculateHalfEnvironmentBounds(float orthographicSize, float zRotationEuler, Vector2 positionOffset)
    {
        // Calculate the size of the camera window in world-space
        float aspectRatio = (float)Screen.width / Screen.height;

        Vector2 halfCameraDimensions;
        halfCameraDimensions.x = aspectRatio * orthographicSize;
        halfCameraDimensions.y = orthographicSize;

        // Create bounding box for the camera, including its rotation
        float zRotationRadians = zRotationEuler * Mathf.Deg2Rad;
        float horizontalScalar = Mathf.Abs(Mathf.Cos(zRotationRadians));
        float verticalScalar = Mathf.Abs(Mathf.Sin(zRotationRadians));

        Vector2 halfCameraBoundsDimensions;
        halfCameraBoundsDimensions.x = (halfCameraDimensions.x * horizontalScalar) + (halfCameraDimensions.y * verticalScalar);
        halfCameraBoundsDimensions.y = (halfCameraDimensions.x * verticalScalar) + (halfCameraDimensions.y * horizontalScalar);

        // Return bounding box for the camera like above, except the origin of the box must be at (0, 0)
        positionOffset.x = Mathf.Abs(positionOffset.x);
        positionOffset.y = Mathf.Abs(positionOffset.y);
        return halfCameraBoundsDimensions + positionOffset;
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

    private void UpdateBoundsMeshes(Vector2 halfBoundsSize)
    {
        // Calculate vertex positions
        Vector2 topLeft = new Vector2(-halfBoundsSize.x, halfBoundsSize.y);
        Vector2 topRight = new Vector2(halfBoundsSize.x, halfBoundsSize.y);
        Vector2 bottomRight = new Vector2(halfBoundsSize.x, -halfBoundsSize.y);
        Vector2 bottomLeft = new Vector2(-halfBoundsSize.x, -halfBoundsSize.y);

        // Set vertex positions
        maskBoundsVertices[0] = topLeft;
        maskBoundsVertices[1] = topRight;
        maskBoundsVertices[2] = bottomRight;
        maskBoundsVertices[3] = bottomLeft;

        maskBoundsMesh.vertices = maskBoundsVertices;

        // Apply updated mesh to filters
        maskBackgroundFilter.mesh = maskBoundsMesh;
        maskForegroundFilter.mesh = maskBoundsMesh;
    }

    private void ConstrainEnvironmentTransforms()
    {
        // Lock world transform to player position in the world
        environmentTransform.position = targetTransform.position;

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