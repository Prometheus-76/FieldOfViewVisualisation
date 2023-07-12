using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public partial class CarvableMesh : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Transform meshTransform = null;
    public MeshFilter meshFilter = null;
    public MeshRenderer meshRenderer = null;

    [Header("Configuration")]
    public LayerMask carvingLayers = 0;
    public Vector2 halfMeshSize = Vector2.zero;

    [Header("Edge Search Fidelity")]
    [Range(1, 32)] 
    public int maxEdgeSearchIterations = 8;
    [Range(0.001f, 5f)]
    public float edgeSearchMinAngleThreshold = 0.1f;

    [Header("Continuity Definition")]
    [Min(0.01f)] 
    public float projectionOffsetThreshold = 0.25f;
    [Range(0.1f,180f)]
    public float angleContinuityThreshold = 5f;

    [Header("Optimisation")]
    public bool cullContinuousVertices = false;
    public bool generateTextureCoordinates = false;

    #endregion

    // PRIVATE
    private bool meshFailed = false;
    private Mesh meshInstance = null;
    private RaycastHit2D[] nonAllocHits;

    // PROPERTIES
    private bool isInitialised
    {
        get
        {
            // All required conditions for correct functionality
            if (meshTransform == null) return false;
            if (meshFilter == null) return false;
            if (meshRenderer == null) return false;
            if (meshInstance == null) return false;
            if (nonAllocHits == null) return false;

            if (meshFilter.mesh != meshInstance) return false;

            // All conditions met
            return true;
        }
    }

    private Vector2 topRight { get { return new Vector2(halfMeshSize.x, halfMeshSize.y); } }
    private Vector2 topLeft { get { return new Vector2(-halfMeshSize.x, halfMeshSize.y); } }
    private Vector2 bottomLeft { get { return new Vector2(-halfMeshSize.x, -halfMeshSize.y); } }
    private Vector2 bottomRight { get { return new Vector2(halfMeshSize.x, -halfMeshSize.y); } }

    #region Public Methods

    public void Initialise()
    {
        if (isInitialised) return;

        // Ensure components are assigned
        if (meshTransform == null) meshTransform = GetComponent<Transform>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

        // Reserve memory for cheaper raycasting
        nonAllocHits = new RaycastHit2D[2];

        // Clear any existing mesh data, or create the mesh instance
        if (meshInstance != null)
        {
            meshInstance.Clear();
        }
        else
        {
            meshInstance = new Mesh();
            meshInstance.name = gameObject.name;
        }

        // Ensure mesh is assigned to filter
        if (meshFilter != null) meshFilter.mesh = meshInstance;

        // If everything worked the way we expected, allow the mesh to operate
        if (isInitialised) meshFailed = false;
    }

    public void UpdateMesh()
    {
        if (meshFailed) return;

        // Attempt to ensure things are initialised
        if (isInitialised == false)
        {
            Initialise();

            // Initialisation failed
            if (isInitialised == false)
            {
                Debug.Log("CarvableMesh initialisation failed: " + "\"" + gameObject.name + "\"");

                meshFailed = true;
                return;
            }
        }

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
        if (Physics2D.OverlapPoint(meshTransform.position, carvingLayers) != null) return;

        // Perform shape projection algorithm to find points which wrap surrounding geometry
        List<EdgePoint> edgePoints = CalculateVisibleShapeVertices();
        CalculateProjectionVertices(edgePoints);
        CalculateIntersectionVertices(edgePoints);

        // Generate mesh from EdgePoint list
        Vector3[] vertices = CreateVertices(edgePoints);
        meshInstance.vertices = vertices;

        int[] triangles = FormTriangles(vertices);
        meshInstance.triangles = triangles;

        if (generateTextureCoordinates)
        {
            Vector2[] UVs = CalculateUVs(vertices);
            meshInstance.uv = UVs;
        }
    }

    #endregion

    private List<EdgePoint> CalculateVisibleShapeVertices()
    {
        List<EdgePoint> edgePoints = new List<EdgePoint>();

        Collider2D[] collidersInRange = Physics2D.OverlapBoxAll(meshTransform.position, halfMeshSize * 2f * meshTransform.lossyScale, meshTransform.eulerAngles.z);

        // Create a clock-wise winding of points that encompass each collider in range
        for (int c = 0; c < collidersInRange.Length; c++)
        {
            List<EdgePoint> colliderVertices = GetColliderVertices(collidersInRange[c]);
            if (colliderVertices == null) continue; // Unsupported collider type

            // For each point in the shape...
            for (int v = 0; v < colliderVertices.Count; v++)
            {
                // Are either of this point's adjacent edges facing the center of the mesh?
                // (Yes it is actually faster to do this before an in-range check)
                if (IsPointFrontFacing(colliderVertices[v]))
                {
                    // Is this front-facing point in range?
                    if (MathUtilities.IsPointInQuad(colliderVertices[v].position, halfMeshSize))
                    {
                        // Is this in-range, front-facing point occluded by other geometry?
                        if (IsPointOccluded(colliderVertices[v], collidersInRange[c]) == false)
                        {
                            // This point is valid!
                            edgePoints.Add(colliderVertices[v]);
                        }
                    }
                }
            }
        }

        // Add mesh corners, if they're not occluded
        EdgePoint topRightPoint = new EdgePoint(topRight, Vector2.left, Vector2.down, MathUtilities.VectorTo360Angle(topRight.x, topRight.y), false);
        if (IsPointOccluded(topRightPoint, null) == false) edgePoints.Add(topRightPoint);

        EdgePoint topLeftPoint = new EdgePoint(topLeft, Vector2.down, Vector2.right, MathUtilities.VectorTo360Angle(topLeft.x, topLeft.y), false);
        if (IsPointOccluded(topLeftPoint, null) == false) edgePoints.Add(topLeftPoint);

        EdgePoint bottomLeftPoint = new EdgePoint(bottomLeft, Vector2.right, Vector2.up, MathUtilities.VectorTo360Angle(bottomLeft.x, bottomLeft.y), false);
        if (IsPointOccluded(bottomLeftPoint, null) == false) edgePoints.Add(bottomLeftPoint);

        EdgePoint bottomRightPoint = new EdgePoint(bottomRight, Vector2.up, Vector2.left, MathUtilities.VectorTo360Angle(bottomRight.x, bottomRight.y), false);
        if (IsPointOccluded(bottomRightPoint, null) == false) edgePoints.Add(bottomRightPoint);

        // QuickSort by angle from 0 to 360, using the CompareTo() method implemented in EdgePoint
        edgePoints.Sort();

        return edgePoints;
    }

    private void CalculateProjectionVertices(List<EdgePoint> edgePoints)
    {
        // Sweep around vertices in ascending angle order
        for (int i = 0; i < edgePoints.Count; i++)
        {
            // Determine if this is an edge, and if so, is it starting or ending a surface?
            int edgePointStatus = FindPointEdgeStatus(edgePoints[i]);

            // For all edge vertices...
            if (edgePointStatus != 0)
            {
                // Cast through them to find a point behind
                EdgePoint edgeDropoffPoint = FindPointBehind(edgePoints[i].position);

                // Does this point go before or after it's corresponding corner vertex in the list?
                bool insertBefore = (edgePointStatus == -1);

                // Find insertion index
                int insertionIndex = insertBefore ? i : (i + 1);

                // Mark EdgePoints as degenerate (meaning they are aligned along the vector to the center)
                edgePoints[i].isDegenerate = true;
                edgeDropoffPoint.isDegenerate = true;

                // Update normals to fake continuity
                if (insertBefore)
                {
                    // This edge is at the start of the object
                    Vector2 edgeNormal = MathUtilities.TangentToNormal(edgePoints[i].position - edgeDropoffPoint.position);
                    edgePoints[i].previousNormal = edgeNormal;
                    edgeDropoffPoint.nextNormal = edgeNormal;
                }
                else
                {
                    // This edge is at the end of the object
                    Vector2 edgeNormal = MathUtilities.TangentToNormal(edgeDropoffPoint.position - edgePoints[i].position);
                    edgePoints[i].nextNormal = edgeNormal;
                    edgeDropoffPoint.previousNormal = edgeNormal;
                }

                // Insert the dropoff point, and ensure we skip it, as we know it isn't a vertex point
                edgePoints.Insert(insertionIndex, edgeDropoffPoint);
                i += 1;
            }
        }
    }

    private void CalculateIntersectionVertices(List<EdgePoint> edgePoints)
    {
        int currentIndex = 0;
        int nextIndex = 1;

        EdgePoint startingElement = edgePoints[0];

        // Sweep around vertices in ascending angle order until startingElement is continuous with the one that comes before it
        while (true)
        {
            nextIndex = (currentIndex + 1) % edgePoints.Count;

            // The bounds of the search, which narrow over time
            EdgePoint minEdge = edgePoints[currentIndex];
            EdgePoint maxEdge = edgePoints[nextIndex];

            // If two consecutive points are not continuous, we need to find an edge between them
            if (AreEdgePointsContinuous(minEdge, maxEdge) == false)
            {
                // Search for a set of vertices that approximate the geometric intersection causing the discontinuity
                Tuple<EdgePoint, EdgePoint> detailPoints = FindEdge(minEdge, maxEdge);

                // Insert minEdge, if it contains new information
                int minEdgeIndex = currentIndex;
                if (detailPoints.Item1 != minEdge && detailPoints.Item1.position != minEdge.position)
                {
                    minEdgeIndex = BinaryListInsertion(edgePoints, detailPoints.Item1);
                }

                // Insert maxEdge, if it contains new information
                int maxEdgeIndex = nextIndex;
                if (detailPoints.Item2 != maxEdge && detailPoints.Item2.position != maxEdge.position)
                {
                    maxEdgeIndex = BinaryListInsertion(edgePoints, detailPoints.Item2); 
                }

                // If the maxEdge point is inserted over the line between 360 and 0 degrees, it should push the minEdge index up by one
                if (minEdgeIndex > maxEdgeIndex) minEdgeIndex += 1;

                // Stitch normals together between inserted points
                detailPoints.Item1.previousNormal = edgePoints[minEdgeIndex - 1 >= 0 ? minEdgeIndex - 1 : edgePoints.Count - 1].nextNormal;

                detailPoints.Item1.nextNormal = MathUtilities.TangentToNormal(detailPoints.Item2.position - detailPoints.Item1.position);
                detailPoints.Item2.previousNormal = detailPoints.Item1.nextNormal;

                // Ensure we skip to checking the max detail point next
                currentIndex = maxEdgeIndex;
            }
            else
            {
                // We've completed a full loop around the point set
                if (edgePoints[nextIndex] == startingElement) break;

                // Check the next one
                currentIndex = nextIndex;
            }
        }
    }

    private EdgePoint RaycastToEdge(Vector2 localPoint)
    {
        // Find the furthest end point of the raycast in local-space
        Vector2 pointOnEdge = MathUtilities.ClampPointToQuad(localPoint, halfMeshSize, true);

        bool didRayHit = Physics2D.LinecastNonAlloc(meshTransform.position, meshTransform.TransformPoint(pointOnEdge), nonAllocHits, carvingLayers) > 0;

        // Info about where the ray landed, whether on geometry or the border of the quad
        Vector2 hitPoint = didRayHit ? meshTransform.InverseTransformPoint(nonAllocHits[0].point) : pointOnEdge;
        Vector2 hitNormal = didRayHit ? meshTransform.InverseTransformDirection(nonAllocHits[0].normal) : MathUtilities.DetermineQuadrant(pointOnEdge, halfMeshSize) * -1f;

        return new EdgePoint(hitPoint, hitNormal, MathUtilities.VectorTo360Angle(pointOnEdge.x, pointOnEdge.y), didRayHit);
    }

    private EdgePoint FindPointBehind(Vector2 localPoint)
    {
        // Find the furthest end point of the raycast
        Vector2 pointOnEdge = MathUtilities.ClampPointToQuad(localPoint, halfMeshSize, true);

        int hits = Physics2D.LinecastNonAlloc(meshTransform.position, meshTransform.TransformPoint(pointOnEdge), nonAllocHits, carvingLayers);

        int behindHitIndex = -1;
        Vector2 worldCastThroughPoint = meshTransform.TransformPoint(localPoint);

        // Check each hit
        for (int i = 0; i < hits; i++)
        {
            // Did we hit an object?
            if (nonAllocHits[i].collider != null)
            {
                // Was it the cast through point?
                if (Vector2.Distance(worldCastThroughPoint, nonAllocHits[i].point) < 0.01f)
                {
                    continue;
                }
            }

            // We hit something behind!
            behindHitIndex = i;
            break;
        }

        bool didRayHit = (behindHitIndex != -1);

        // Info about where the ray landed, whether on geometry or the border of the quad
        Vector2 hitPoint = didRayHit ? meshTransform.InverseTransformPoint(nonAllocHits[behindHitIndex].point) : pointOnEdge;
        Vector2 hitNormal = didRayHit ? meshTransform.InverseTransformDirection(nonAllocHits[behindHitIndex].normal) : MathUtilities.DetermineQuadrant(pointOnEdge, halfMeshSize) * -1f;

        return new EdgePoint(hitPoint, hitNormal, MathUtilities.VectorTo360Angle(pointOnEdge.x, pointOnEdge.y), didRayHit);
    }

    private Tuple<EdgePoint, EdgePoint> FindEdge(EdgePoint startEdge, EdgePoint endEdge)
    {
        // Ensure we make new instances so we don't write directly to the list
        EdgePoint minEdge = new EdgePoint(startEdge.position, startEdge.previousNormal, startEdge.nextNormal, startEdge.angle, startEdge.onSurface);
        EdgePoint maxEdge = new EdgePoint(endEdge.position, endEdge.previousNormal, endEdge.nextNormal, endEdge.angle, endEdge.onSurface);

        bool minUpdated = false;
        bool maxUpdated = false;

        // Complete several iterations to solve the edge constraint
        for (int i = 0; i < maxEdgeSearchIterations; i++)
        {
            // Angular delta between minEdge and maxEdge
            float minToMaxAngle = Mathf.DeltaAngle(minEdge.angle, maxEdge.angle);
            if (minToMaxAngle < 0f) minToMaxAngle += 360f; // Ensure clock-wise distance between angles

            // Clock-wise angular lerp to find midpoint
            float midpointAngle = (minToMaxAngle / 2f) + minEdge.angle;
            Vector2 midpoint = MathUtilities.AngleToVector(midpointAngle);

            // Cast through the midpoint
            EdgePoint midEdgePoint = RaycastToEdge(midpoint);
            midEdgePoint.angle = midpointAngle;

            // Based on continuity with previousEdgePoint, save ray hit data to min or max
            if (AreEdgePointsContinuous(minEdge, midEdgePoint))
            {
                // Raise minEdge to maximum continuous value with previousEdgePoint (ie. close to the edge, but not over)
                minEdge = midEdgePoint;
                minUpdated = true;
            }
            else
            {
                // Lower maxEdge to minimum non-continuous value with previousEdgePoint (ie. just over the edge)
                maxEdge = midEdgePoint;
                maxUpdated = true;
            }

            // This is close enough for us to be satisfied, early out to save on performance
            if (Mathf.Abs(Mathf.DeltaAngle(minEdge.angle, maxEdge.angle)) <= edgeSearchMinAngleThreshold) break;
        }

        // Return both edges in order
        return new Tuple<EdgePoint, EdgePoint>(minUpdated ? minEdge : startEdge, maxUpdated ? maxEdge : endEdge);
    }

    private int BinaryListInsertion(List<EdgePoint> edgePoints, EdgePoint localPoint)
    {
        if (edgePoints.Count == 0)
        {
            edgePoints.Add(localPoint);
            return 0;
        }

        int minIndex = 0;
        int maxIndex = edgePoints.Count - 1;

        // If the point we are adding is a new min or max of the dataset
        if (localPoint.angle >= edgePoints[maxIndex].angle)
        {
            edgePoints.Add(localPoint);
            return edgePoints.Count - 1;
        }
        else if (localPoint.angle < edgePoints[minIndex].angle)
        {
            edgePoints.Insert(0, localPoint);
            return 0;
        }

        // The point we are adding lies somewhere in the middle of the dataset
        while (true)
        {
            // Find half-way between min and max
            int midpointIndex = minIndex + ((maxIndex - minIndex) / 2);
            EdgePoint midpointElement = edgePoints[midpointIndex];

            // Points have collapsed (ie. the search is over)
            if (maxIndex == minIndex + 1)
            {
                edgePoints.Insert(maxIndex, localPoint);
                return maxIndex;
            }

            // Exact match
            if (localPoint.angle == midpointElement.angle)
            {
                edgePoints.Insert(midpointIndex, localPoint);
                return midpointIndex;
            }

            // Raise min index
            if (localPoint.angle > midpointElement.angle)
            {
                minIndex = midpointIndex;
                continue;
            }

            // Lower max index
            if (localPoint.angle < midpointElement.angle)
            {
                maxIndex = midpointIndex;
                continue;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Set gizmo drawing relative to GameObject
        Gizmos.matrix = meshTransform != null ? meshTransform.localToWorldMatrix : transform.localToWorldMatrix;

        // Draw quad edges, clockwise from the top
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}