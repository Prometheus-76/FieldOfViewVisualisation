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
    public ScreenshakeSystem screenshakeSystem;

    [Header("Configuration")]
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
    }

    private void Start()
    {
        // Set starting position in scene
        followerPosition = controllerTransform.position;

        // Set remapped zoom value to match initial inspector value
        SetInitialZoom(sceneCamera.orthographicSize);
    }

    private void Update()
    {
        UpdateFollowerPosition(Time.deltaTime);

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

    private void SetInitialZoom(float cameraSize)
    {
        // In camera size, not scaled zoom
        float zoomMidpoint = (zoomRange * zoomDampingMidpoint) + zoomMinimumLimit;
        float zoomMaximumLimit = zoomMinimumLimit + zoomRange;

        float zoomLowerRange = (zoomMidpoint - zoomMinimumLimit);
        float zoomUpperRange = (zoomMaximumLimit - zoomMidpoint);

        // Remap zoom to the predefined limit, with damping
        float dampedZoomValue = zoomMidpoint;
        if (cameraSize < zoomMidpoint)
        {
            // Lower curve
            dampedZoomValue = EvaluateInverseDampingFunction(zoomLowerRange, zoomDampingGradient, cameraSize - zoomMidpoint) + zoomMidpoint;
        }
        else if (cameraSize > zoomMidpoint)
        {
            // Upper curve
            dampedZoomValue = EvaluateInverseDampingFunction(zoomUpperRange, zoomDampingGradient, cameraSize - zoomMidpoint) + zoomMidpoint;
        }

        // Apply scaled value
        SetZoom(dampedZoomValue, true);
    }

    private void UpdateFollowerPosition(float deltaTime)
    {
        // Smoothly follow the camera target
        followerPosition = Vector2.SmoothDamp(followerPosition, targetTransform.position, ref followerVelocity, positionFollowDelay, Mathf.Infinity, deltaTime);

        // Ensure z-position is set
        Vector3 newPosition = followerPosition;
        newPosition.z = depthOffset * -1f;

        controllerTransform.position = newPosition;
    }
    
    public void UpdateBasePosition(float deltaTime)
    {
        // Smoothly approach target base position offset
        currentBasePositionOffset = Vector2.SmoothDamp(currentBasePositionOffset, targetBasePositionOffset, ref basePositionOffsetVelocity, positionOffsetDelay, Mathf.Infinity, deltaTime);
    }

    private void UpdateOffsetPosition()
    {
        // Remap position to the predefined limit, with damping
        Vector2 dampedShakeOffset;
        dampedShakeOffset.x = EvaluateDampingFunction(positionRange.x, positionDampingGradient, screenshakeSystem.smoothPositionOffset.x);
        dampedShakeOffset.y = EvaluateDampingFunction(positionRange.y, positionDampingGradient, screenshakeSystem.smoothPositionOffset.y);

        sceneCameraTransform.localPosition = currentBasePositionOffset + dampedShakeOffset;
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
        float dampedSizeValue = zoomMidpoint;
        if (combinedZoomValue < zoomMidpoint)
        {
            // Lower curve
            dampedSizeValue = EvaluateDampingFunction(zoomLowerRange, zoomDampingGradient, combinedZoomValue - zoomMidpoint) + zoomMidpoint;
        }
        else if (combinedZoomValue > zoomMidpoint)
        {
            // Upper curve
            dampedSizeValue = EvaluateDampingFunction(zoomUpperRange, zoomDampingGradient, combinedZoomValue - zoomMidpoint) + zoomMidpoint;
        }

        sceneCamera.orthographicSize = dampedSizeValue;
    }

    // For converting linear input to output damped within the specified range
    private float EvaluateDampingFunction(float range, float gradient, float input)
    {
        return range * MathUtilities.Tanh((gradient * input) / range);
    }

    // For converting damped input within the specified range to a linear output
    private float EvaluateInverseDampingFunction(float range, float gradient, float input)
    {
        input = Mathf.Clamp(input, -range + 0.1f, range - 0.1f);

        float scaledInput = input / range;
        float rangeScalar = range / (2f * gradient);

        return rangeScalar * Mathf.Log((1f + scaledInput) / (1f - scaledInput));
    }
}
