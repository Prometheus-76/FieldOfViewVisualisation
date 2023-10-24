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
    [Min(0f)]
    public float followDelay;
    [Min(0f)]
    public float depthOffset;

    // PRIVATE
    private Transform cameraTransform;
    private Transform renderingEnvironmentTransform;

    private Vector2 followPosition = Vector2.zero;
    private Vector2 followVelocity = Vector2.zero;

    private void Awake()
    {
        // Assign references
        cameraTransform = sceneCamera.transform;
        renderingEnvironmentTransform = renderingEnvironment.transform;
    }

    private void Start()
    {
        // Set starting position in scene
        followPosition = controllerTransform.position;
    }

    private void Update()
    {
        FollowCameraTarget();

        // Keep rendering environment z-position at 0 in world-space
        Vector3 renderingEnvironmentLocalPosition = renderingEnvironmentTransform.localPosition;
        renderingEnvironmentLocalPosition.z = depthOffset;
        renderingEnvironmentTransform.localPosition = renderingEnvironmentLocalPosition;
    }

    private void FollowCameraTarget()
    {
        // Smoothly follow the camera target
        followPosition = Vector2.SmoothDamp(followPosition, targetTransform.position, ref followVelocity, followDelay, Mathf.Infinity, Time.deltaTime);

        // Ensure z-position is set
        Vector3 newPosition = followPosition;
        newPosition.z = depthOffset * -1f;

        controllerTransform.position = newPosition;
    }
}
