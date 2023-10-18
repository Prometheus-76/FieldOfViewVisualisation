using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    public Transform controllerTransform;
    public Transform playerTransform;
    public RenderingEnvironment renderingEnvironment;

    [Header("Configuration")]
    [Min(0f)]
    public float followDelay;
    [Min(0f)]
    public float depthOffset;

    // PRIVATE
    private Transform renderingEnvironmentTransform;

    private Vector2 followPosition = Vector2.zero;
    private Vector2 followVelocity = Vector2.zero;

    private void Awake()
    {
        // Assign references
        renderingEnvironmentTransform = renderingEnvironment.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        followPosition = controllerTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Smoothly follow the player
        followPosition = Vector2.SmoothDamp(followPosition, playerTransform.position, ref followVelocity, followDelay, Mathf.Infinity, Time.deltaTime);

        // Ensure z-position is set
        Vector3 newPosition = followPosition;
        newPosition.z = depthOffset * -1f;

        controllerTransform.position = newPosition;

        // Keep rendering environment z-position at 0 in world-space
        Vector3 renderingEnvironmentLocalPosition = renderingEnvironmentTransform.localPosition;
        renderingEnvironmentLocalPosition.z = depthOffset;
        renderingEnvironmentTransform.localPosition = renderingEnvironmentLocalPosition;
    }
}
