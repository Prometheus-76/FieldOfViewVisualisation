using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarvableMesh : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Transform meshTransform = null;
    public MeshFilter meshFilter = null;
    public MeshRenderer meshRenderer = null;

    [Header("Configuration")]
    public bool autoInitialise = false;
    public LayerMask carvingLayers = 0;
    public Vector2 meshSize = Vector2.zero;

    [Header("Base Fidelity")]
    public bool cullContinuousVertices = false;
    [Min(0.01f)] 
    public float tangentStepDistance = 0.3f;

    [Header("Edge Search Fidelity")]
    [Range(0, 32)] 
    public int edgeSearchIterations = 8;
    [Min(0.01f)] 
    public float projectionOffsetThreshold = 0.25f;
    [Range(0.1f,180f)]
    public float angleContinuityThreshold = 5f;

    #endregion

    // PRIVATE
    private Mesh meshInstance = null;

    class EdgePoint
    {
        public Vector2 point;
        public Vector2 normal;

        public float angle;
        public bool hitObstacle;

        public EdgePoint(Vector2 point, Vector2 normal, float angle = 0f, bool hitObstacle = false)
        {
            this.point = point;
            this.normal = normal;

            this.angle = angle;
            this.hitObstacle = hitObstacle;
        }
    }

    void Start()
    {
        // Ensure components are assigned
        if (meshTransform == null) meshTransform = GetComponent<Transform>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

        if (autoInitialise) UpdateMesh();
    }

    private void LateUpdate()
    {
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        // Require all components to update mesh
        if (meshTransform == null) return;
        if (meshFilter == null) return;
        if (meshRenderer == null) return;

        // Ensure mesh is assigned
        if (meshInstance == null)
        {
            meshInstance = new Mesh();
            meshInstance.name = "ProceduralMesh";

            meshFilter.mesh = meshInstance;
        }

        meshInstance.Clear();

        // Check if the center of the mesh is inside a carvable object, causing all geometry to be occluded
        if (Physics2D.OverlapCircle(meshTransform.position, 0f, carvingLayers) != null) return;

        // Scan around the center, find all discontinuities and attempt to create an accurate set of vertices to fit the surrounding geometry
        List<EdgePoint> edgePoints = ConstructEdgePoints();

        Vector3[] vertices = BuildVertexArray(edgePoints);
        int[] triangles = FormTriangles(vertices);
        Vector2[] UVs = CalculateUVs(vertices);

        meshInstance.vertices = vertices;
        meshInstance.triangles = triangles;
        meshInstance.uv = UVs;
    }

    bool AreEdgePointsContinuous(EdgePoint a, EdgePoint b)
    {
        // One (and only one) of the edges are on the border (ie. the ray missed any obstacles)
        if (a.hitObstacle != b.hitObstacle) return false;

        // If the angle between the surface normals is too large, then we treat them as non-continuous
        if (Vector2.Angle(a.normal, b.normal) > angleContinuityThreshold) return false;

        // Project points onto surface tangents of each other
        Vector2 projectedAOnTangentB = (Vector2)Vector3.ProjectOnPlane(a.point - b.point, b.normal) + b.point;
        Vector2 projectedBOnTangentA = (Vector2)Vector3.ProjectOnPlane(b.point - a.point, a.normal) + a.point;

        // Find maximum discrepancy between the point position and where it would need to be for perfect continuity
        float maxContinuityOffset = Mathf.Max(Vector2.Distance(a.point, projectedAOnTangentB), Vector2.Distance(b.point, projectedBOnTangentA));

        if (maxContinuityOffset > projectionOffsetThreshold) return false;

        // All conditions met, the points are continuous
        return true;
    }

    Vector2 ClampPointToQuad(Vector2 localInputVector, bool edgeOnly)
    {
        Vector2 localOutputVector = Vector2.zero;

        if (edgeOnly || Mathf.Abs(localInputVector.x) > meshSize.x / 2f || Mathf.Abs(localInputVector.y) > meshSize.y / 2f)
        {
            // Edge-fitting required, determine side of intersection
            if (Mathf.Abs(localInputVector.x) * meshSize.y <= Mathf.Abs(localInputVector.y) * meshSize.x)
            {
                // Intersection with top or bottom side of quad
                localOutputVector.x = (meshSize.y / 2f) * (localInputVector.x / Mathf.Abs(localInputVector.y));
                localOutputVector.y = Mathf.Sign(localInputVector.y) * (meshSize.y / 2f);
            }
            else
            {
                // Intersection with left or right side of quad
                localOutputVector.x = Mathf.Sign(localInputVector.x) * (meshSize.x / 2f);
                localOutputVector.y = meshSize.x / 2f * (localInputVector.y / Mathf.Abs(localInputVector.x));
            }
        }
        else
        {
            // No clamping required, return original vector
            localOutputVector = localInputVector;
        }

        return localOutputVector;
    }

    Vector2 DetermineQuadrant(Vector2 localInputVector)
    {
        if (Mathf.Abs(localInputVector.x) * meshSize.y <= Mathf.Abs(localInputVector.y) * meshSize.x)
        {
            if (localInputVector.y > 0f) return Vector2.up;
            if (localInputVector.y < 0f) return Vector2.down;
        }
        else
        {
            if (localInputVector.x > 0f) return Vector2.right;
            if (localInputVector.x < 0f) return Vector2.left;
        }

        return Vector2.zero;
    }

    Vector2 FindStepPoint(EdgePoint previousPoint)
    {
        // Find tangent to previousPoint, but flip it's direction to face left with the normal up
        Vector2 tangent = new Vector2(previousPoint.normal.y, -previousPoint.normal.x);

        // Move along the tangent by the step distance to find the next point to check
        Vector2 stepPoint = previousPoint.point + (tangent.normalized * tangentStepDistance);

        // In case we raycast against a surface with a tangent aligned directly with the center of the mesh (causes infinite loop)
        if (Mathf.Abs(Vector2.Dot(-tangent, stepPoint.normalized)) >= 0.9999f)
        {
            float originalMagnitude = stepPoint.magnitude;

            // Rotate the step point around the mesh center by a tiny amount to offset the tangent issue
            float stepAngle = VectorTo360Angle(stepPoint.x, stepPoint.y);
            stepAngle += 0.1f; // Degrees
            stepAngle *= Mathf.Deg2Rad;

            stepPoint = new Vector2(Mathf.Cos(stepAngle), Mathf.Sin(stepAngle));
            stepPoint *= originalMagnitude;
        }

        return stepPoint;
    }

    float VectorTo360Angle(float x, float y)
    {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        // Remap from (-180, 180) to (0, 360)
        if (angle < 0f) angle += 360f;

        return angle;
    }

    EdgePoint RaycastToEdge(Vector2 castThroughPoint)
    {
        // Find the end point of the raycast
        Vector2 pointOnEdge = ClampPointToQuad(castThroughPoint, true);
        RaycastHit2D hit = Physics2D.Linecast(meshTransform.position, meshTransform.TransformPoint(pointOnEdge), carvingLayers);

        bool didRayHit = (hit.collider != null);

        // Info about where the ray landed, whether on geometry or the border of the quad
        Vector2 hitPoint = didRayHit ? meshTransform.InverseTransformPoint(hit.point) : pointOnEdge;
        Vector2 hitNormal = didRayHit ? meshTransform.InverseTransformDirection(hit.normal) : -DetermineQuadrant(pointOnEdge);

        return new EdgePoint(hitPoint, hitNormal, 0f, didRayHit);
    }

    Tuple<EdgePoint, EdgePoint> FindEdge(EdgePoint previousEdgePoint, EdgePoint projectedEdgePoint)
    {
        EdgePoint minEdge = new EdgePoint(previousEdgePoint.point, previousEdgePoint.normal, previousEdgePoint.angle);
        EdgePoint maxEdge = new EdgePoint(projectedEdgePoint.point, projectedEdgePoint.normal, projectedEdgePoint.angle);

        // Complete several iterations to solve the edge constraint
        for (int i = 0; i < edgeSearchIterations; i++)
        {
            // Fire ray half-way between min and max
            Vector2 midpoint = (minEdge.point + maxEdge.point) / 2f;
            EdgePoint midEdgePoint = RaycastToEdge(midpoint);
            midEdgePoint.angle = VectorTo360Angle(midpoint.x, midpoint.y);

            // Based on continuity with previousEdgePoint, save ray hit data to min or max
            if (AreEdgePointsContinuous(previousEdgePoint, midEdgePoint))
            {
                // Raise minEdge to maximum continuous value with previousEdgePoint (ie. close to the edge, but not over)
                minEdge = midEdgePoint;
            }
            else
            {
                // Lower maxEdge to minimum non-continuous value with previousEdgePoint (ie. just over the edge)
                maxEdge = midEdgePoint;
            }
        }

        // Return both edges in order
        return new Tuple<EdgePoint, EdgePoint>(minEdge, maxEdge);
    }

    List<EdgePoint> ConstructEdgePoints()
    {
        List<EdgePoint> edgePoints = new List<EdgePoint>();

        bool fullLoopCompleted = false;
        bool firstEdgePlaced = false;
        EdgePoint previousEdgePoint = null;

        // Continue placing points until we complete a loop around the exterior, and the first and last points are continuous like all others
        while (fullLoopCompleted == false || AreEdgePointsContinuous(edgePoints[0], previousEdgePoint) == false)
        {
            // Find new point to raycast through based on previous vertex placement (start at 0 degrees if there is no previous point)
            Vector2 newProjectionPoint = firstEdgePlaced ? FindStepPoint(previousEdgePoint) : ClampPointToQuad(Vector2.right, true);
            float newProjectionAngle = firstEdgePlaced ? VectorTo360Angle(newProjectionPoint.x, newProjectionPoint.y) : 0f;

            // Compare angles, if passing through 0 degrees on this step, clamp maximum step at 0 degrees
            if (firstEdgePlaced)
            {
                // Compare previous and projected angles to see if we have completed a full loop of the perimeter
                // When 360 degrees loops back around to 0 degrees, the previous value is greater than the projected one
                if (previousEdgePoint.angle > newProjectionAngle)
                {
                    // This means we are now looking for a point to connect seamlessly to the first edge point
                    fullLoopCompleted = true;

                    // Clamp the new point to not go past 0 degrees
                    newProjectionPoint = ClampPointToQuad(Vector2.right, true);
                    newProjectionAngle = 0f;
                }
            }

            // Linecast through the newProjectedPoint to the edge of the quad
            EdgePoint projectedEdgePoint = RaycastToEdge(newProjectionPoint);
            projectedEdgePoint.angle = newProjectionAngle;

            bool projectionIsContinuous = firstEdgePlaced ? AreEdgePointsContinuous(previousEdgePoint, projectedEdgePoint) : true;

            if (projectionIsContinuous)
            {
                // If continuous, place vertex

                edgePoints.Add(projectedEdgePoint);
            }
            else
            {
                // If not continuous, search for edge

                Tuple<EdgePoint, EdgePoint> edgePair = FindEdge(previousEdgePoint, projectedEdgePoint);

                // Place vertex at min, and then at max
                edgePoints.Add(edgePair.Item1);
                edgePoints.Add(edgePair.Item2);
            }

            // Set the previous edge point
            previousEdgePoint = edgePoints[edgePoints.Count - 1];

            if (edgePoints.Count > 1024)
            {
                Debug.Log("EdgePoint overload! (1024+)");
                break;
            }

            firstEdgePlaced = true;
        }

        return edgePoints;
    }

    Vector3[] BuildVertexArray(List<EdgePoint> edgePoints)
    {
        if (cullContinuousVertices)
        {
            // Skip the center (first) vertex
            for (int i = 1; i < edgePoints.Count; i++)
            {
                // Get the EdgePoint before this one
                EdgePoint previousEdgePoint = edgePoints[(i - 1) < 1 ? edgePoints.Count - 1 : (i - 1)];

                // Get the EdgePoint after this one
                EdgePoint nextEdgePoint = edgePoints[(i + 1) > edgePoints.Count - 1 ? 1 : (i + 1)];

                // If before/after are continuous, cull this vertex
                if (AreEdgePointsContinuous(previousEdgePoint, nextEdgePoint))
                {
                    edgePoints.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        Vector3[] vertices = new Vector3[edgePoints.Count + 1];

        // First point must be in the center to have line of sight to all other points in the mesh
        vertices[0] = Vector2.zero;

        // We only care about the positions of the points now
        for (int i = 0; i < edgePoints.Count; i++)
        {
            vertices[i + 1] = edgePoints[i].point;
        }

        return vertices;
    }

    int[] FormTriangles(Vector3[] vertices)
    {
        int triangleIndexCount = (vertices.Length - 1) * 3;
        int[] triangles = new int[triangleIndexCount];

        // Setup triangle winding order
        for (int i = 0; i < triangleIndexCount - 2; i += 3)
        {
            // First edge vertex (loops back around to share first edge vertex for last triangle)
            triangles[i] = (i < triangleIndexCount - 3) ? (i / 3) + 2 : 1;

            // Second edge vertex
            triangles[i + 1] = (i / 3) + 1;

            // Center
            triangles[i + 2] = 0;
        }

        return triangles;
    }

    Vector2[] CalculateUVs(Vector3[] vertices)
    {
        Vector2[] UVs = new Vector2[vertices.Length];

        // Setup mesh UVs
        for (int i = 0; i < vertices.Length; i++)
        {
            UVs[i].x = Mathf.Clamp01(Mathf.InverseLerp(-meshSize.x / 2f, meshSize.x / 2f, vertices[i].x));
            UVs[i].y = Mathf.Clamp01(Mathf.InverseLerp(-meshSize.y / 2f, meshSize.y / 2f, vertices[i].y));
        }

        return UVs;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 topLeft = new Vector2(-meshSize.x / 2f, meshSize.y / 2f);
        Vector2 topRight = new Vector2(meshSize.x / 2f, meshSize.y / 2f);
        Vector2 bottomRight = new Vector2(meshSize.x / 2f, -meshSize.y / 2f);
        Vector2 bottomLeft = new Vector2(-meshSize.x / 2f, -meshSize.y / 2f);

        // Set gizmo drawing relative to GameObject
        Gizmos.matrix = meshTransform != null ? meshTransform.localToWorldMatrix : transform.localToWorldMatrix;

        // Draw relaxed edges, clockwise from the top
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}