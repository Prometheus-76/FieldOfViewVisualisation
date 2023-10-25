using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    public Transform controllerTransform;
    public Transform targetTransform;
    public Camera sceneCamera;
    public RenderingEnvironment renderingEnvironment;
    public ScreenshakeSystem screenshakeSystem;

    [Header("Configuration")]
    public LayerMask blockingLayers;
    [Min(0f)]
    public float depthOffset;

    [Header("Position")]
    [Min(0f)]
    public float positionFollowDelay;
    [Min(0f)]
    public float positionOffsetDelay;
    [Min(0f)]
    public Vector2 positionRange;
    [Range(0.1f, 1f)]
    public float positionDampingGradient;

    [Header("Rotation")]
    [Range(0f, 180f)]
    public float rotationRange;
    [Range(0.1f, 1f)]
    public float rotationDampingGradient;

    [Header("Zoom")]
    [Min(0f)]
    public float baseZoomDelay;
    [Min(0.1f)]
    public float zoomMinimumLimit;
    [Min(0f)]
    public float zoomRange;
    [Range(0f, 1f)]
    public float zoomDampingMidpoint;
    [Range(0.1f, 1f)]
    public float zoomDampingGradient;

    // PRIVATE
    private Transform sceneCameraTransform;
    private Transform renderingEnvironmentTransform;

    private Vector2 followerPosition = Vector2.zero;
    private Vector2 followerVelocity = Vector2.zero;

    private Vector2 currentBasePositionOffset = Vector2.zero;
    private Vector2 targetBasePositionOffset = Vector2.zero;
    private Vector2 basePositionOffsetVelocity = Vector2.zero;

    private float currentBaseZoom = 0f;
    private float targetBaseZoom = 0f;
    private float baseZoomVelocity = 0f;

    private void Awake()
    {
        // Assign references
        sceneCameraTransform = sceneCamera.transform;
        renderingEnvironmentTransform = renderingEnvironment.transform;
    }

    private void Start()
    {
        // Set starting position in scene
        followerPosition = controllerTransform.position;

        SetZoom(sceneCamera.orthographicSize, true);
    }

    private void Update()
    {
        UpdateFollowerPosition(Time.deltaTime);
        EnforceDepthOffset();

        UpdateBasePosition(Time.deltaTime);
        UpdateOffsetPosition();

        UpdateOffsetRotation();

        UpdateBaseZoom(Time.deltaTime);
        UpdateOffsetZoom();
    }

    #region Public Methods

    public void SetPositionOffset(Vector2 newPositionOffset, bool instantly = false)
    {
        targetBasePositionOffset = newPositionOffset;

        if (instantly)
        {
            currentBasePositionOffset = newPositionOffset;
            basePositionOffsetVelocity = Vector2.zero;
        }
    }

    public void SetZoom(float newZoom, bool instantly = false)
    {
        targetBaseZoom = newZoom;

        if (instantly)
        {
            currentBaseZoom = newZoom;
            baseZoomVelocity = 0f;
        }
    }

    #endregion

    private void UpdateFollowerPosition(float deltaTime)
    {
        // Smoothly follow the camera target
        followerPosition = Vector2.SmoothDamp(followerPosition, targetTransform.position, ref followerVelocity, positionFollowDelay, Mathf.Infinity, deltaTime);

        // Ensure the follower position isn't inside a wall
        RaycastHit2D blockingHit = Physics2D.Linecast(targetTransform.position, followerPosition, blockingLayers);
        followerPosition = blockingHit.collider != null ? blockingHit.point : followerPosition;

        // Ensure z-position is set
        Vector3 newPosition = followerPosition;
        newPosition.z = depthOffset * -1f;

        controllerTransform.position = newPosition;
    }

    private void EnforceDepthOffset()
    {
        // Keep rendering environment z-position at 0 in world-space
        Vector3 renderingEnvironmentLocalPosition = renderingEnvironmentTransform.localPosition;
        renderingEnvironmentLocalPosition.z = depthOffset;
        renderingEnvironmentTransform.localPosition = renderingEnvironmentLocalPosition;
    }
    
    public void UpdateBasePosition(float deltaTime)
    {
        // Smoothly approach target base position offset
        currentBasePositionOffset = Vector2.SmoothDamp(currentBasePositionOffset, targetBasePositionOffset, ref basePositionOffsetVelocity, positionOffsetDelay, Mathf.Infinity, deltaTime);
    }

    private void UpdateOffsetPosition()
    {
        Vector2 combinedPositionOffset = currentBasePositionOffset + screenshakeSystem.smoothPositionOffset;

        // Remap position to the predefined limit, with damping
        Vector2 dampedPositionOffset;
        dampedPositionOffset.x = EvaluateDampingFunction(positionRange.x, positionDampingGradient, combinedPositionOffset.x);
        dampedPositionOffset.y = EvaluateDampingFunction(positionRange.y, positionDampingGradient, combinedPositionOffset.y);

        sceneCameraTransform.localPosition = dampedPositionOffset;
    }

    private void UpdateOffsetRotation()
    {
        // Remap rotation to the predefined limit, with damping
        float dampedRotationOffset = EvaluateDampingFunction(rotationRange, rotationDampingGradient, screenshakeSystem.smoothRotationOffset);

        sceneCameraTransform.localEulerAngles = Vector3.forward * dampedRotationOffset;
    }

    private void UpdateBaseZoom(float deltaTime)
    {
        // Smoothly approach target zoom value
        currentBaseZoom = Mathf.SmoothDamp(currentBaseZoom, targetBaseZoom, ref baseZoomVelocity, baseZoomDelay, Mathf.Infinity, deltaTime);
    }

    private void UpdateOffsetZoom()
    {
        float combinedZoomValue = currentBaseZoom + screenshakeSystem.smoothZoomOffset;

        // In camera size, not scaled zoom
        float zoomMidpoint = (zoomRange * zoomDampingMidpoint) + zoomMinimumLimit;
        float zoomMaximumLimit = zoomMinimumLimit + zoomRange;

        float zoomLowerRange = (zoomMidpoint - zoomMinimumLimit);
        float zoomUpperRange = (zoomMaximumLimit - zoomMidpoint);

        // Remap zoom to the predefined limit, with damping
        float dampedZoomValue = zoomMidpoint;
        if (combinedZoomValue < zoomMidpoint)
        {
            // Lower curve
            dampedZoomValue = EvaluateDampingFunction(zoomLowerRange, zoomDampingGradient, combinedZoomValue - zoomMidpoint) + zoomMidpoint;
        }
        else if (combinedZoomValue > zoomMidpoint)
        {
            // Upper curve
            dampedZoomValue = EvaluateDampingFunction(zoomUpperRange, zoomDampingGradient, combinedZoomValue - zoomMidpoint) + zoomMidpoint;
        }

        sceneCamera.orthographicSize = dampedZoomValue;
    }

    private float EvaluateDampingFunction(float range, float gradient, float input)
    {
        return range * MathUtilities.Tanh((gradient * input) / range);
    }
}
