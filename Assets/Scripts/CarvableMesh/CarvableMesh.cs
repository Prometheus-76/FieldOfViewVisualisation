using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;

public class CarvableMesh : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Transform meshTransform = null;
    public MeshFilter meshFilter = null;
    public MeshRenderer meshRenderer = null;

    [Header("Configuration")]
    public LayerMask carvingLayers = 0;
    public Vector2 meshSize = Vector2.zero;

    [Header("Edge Search Fidelity")]
    [Range(0, 32)] 
    public int maxEdgeSearchIterations = 8;
    [Range(0.001f, 10f)]
    public float edgeSearchMinAngleThreshold = 0.1f;

    [Header("Continuity Definition")]
    [Min(0.01f)] 
    public float projectionOffsetThreshold = 0.25f;
    [Range(0.1f,180f)]
    public float angleContinuityThreshold = 5f;

    [Header("Geometry")]
    [Range(0f, 1f)]
    public float occlusionFractionThreshold = 0.995f;

    [Header("Optimisation")]
    public bool cullBackFacingVertices = false;
    public bool cullOccludedVertices = false;
    public bool cullContinuousVertices = false;

    #endregion

    #region Members

    // PRIVATE
    private bool initialisationFailed = false;
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

    private Vector2 topRight { get { return new Vector2(meshSize.x / 2f, meshSize.y / 2f); } }

    private Vector2 topLeft { get { return new Vector2(meshSize.x / -2f, meshSize.y / 2f); } }

    private Vector2 bottomLeft { get { return new Vector2(meshSize.x / -2f, meshSize.y / -2f); } }

    private Vector2 bottomRight { get { return new Vector2(meshSize.x / 2f, meshSize.y / -2f); } }

    // DATA STRUCTURES
    private class EdgePoint:IComparable<EdgePoint>
    {
        public Vector2 position;

        public Vector2 previousNormal;
        public Vector2 nextNormal;

        public float angle;

        public bool onSurface;
        public bool isDegenerate = false;

        // Edge - (Position, Normal, Angle, Surface)
        public EdgePoint(Vector2 position, Vector2 normal, float angle, bool onSurface)
        {
            this.position = position;

            // Between 2 vertices, therefore both normals are identical
            previousNormal = normal;
            nextNormal = normal;

            this.angle = angle;

            this.onSurface = onSurface;
        }

        // Vertex - (Position, Previous Normal, Next Normal, Angle, Surface)
        public EdgePoint(Vector2 position, Vector2 previousNormal, Vector2 nextNormal, float angle, bool onSurface)
        {
            this.position = position;

            // On a vertex, therefore the edges to either side might have differing normals
            this.previousNormal = previousNormal;
            this.nextNormal = nextNormal;

            this.angle = angle;

            this.onSurface = onSurface;
        }

        // Allows list sorting
        public int CompareTo(EdgePoint other)
        {
            // Should this element go before or after (ascending order)
            if (angle < other.angle) return -1;
            if (angle > other.angle) return 1;

            // It doesn't matter!
            return 0;
        }
    }

    #endregion

    private void Start()
    {
        //UpdateMesh();
    }

    private void LateUpdate()
    {
        UpdateMesh();
    }

    public void Initialise()
    {
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
            meshInstance.name = "ProceduralMesh";
        }

        // Ensure mesh is assigned to filter
        if (meshFilter != null) meshFilter.mesh = meshInstance;

        // If everything worked the way we expected, allow the mesh to operate
        if (isInitialised) initialisationFailed = false;
    }

    public void UpdateMesh()
    {
        if (initialisationFailed) return;

        // Attempt to ensure things are initialised
        if (isInitialised == false)
        {
            Initialise();

            // Initialisation failed
            if (isInitialised == false)
            {
                Debug.Log("CarvableMesh initialisation failed: " + "\"" + gameObject.name + "\"");

                initialisationFailed = true;
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
        List<EdgePoint> edgePoints = ConstructMeshPoints();

        Vector3[] vertices = BuildVertexArray(edgePoints);
        int[] triangles = FormTriangles(vertices);
        Vector2[] UVs = CalculateUVs(vertices);

        meshInstance.vertices = vertices;
        meshInstance.triangles = triangles;
        meshInstance.uv = UVs;
    }

    bool IsPointInRange(Vector2 localPoint)
    {
        return (Mathf.Abs(localPoint.x) <= meshSize.x / 2f && Mathf.Abs(localPoint.y) <= meshSize.y / 2f);
    }

    Vector2 ClampPointToQuad(Vector2 localPoint, bool edgeOnly)
    {
        Vector2 localOutputVector = Vector2.zero;

        if (edgeOnly || IsPointInRange(localPoint) == false)
        {
            // Edge-fitting required, determine side of intersection
            if (Mathf.Abs(localPoint.x) * meshSize.y <= Mathf.Abs(localPoint.y) * meshSize.x)
            {
                // Intersection with top or bottom side of quad
                localOutputVector.x = (meshSize.y / 2f) * (localPoint.x / Mathf.Abs(localPoint.y));
                localOutputVector.y = Mathf.Sign(localPoint.y) * (meshSize.y / 2f);
            }
            else
            {
                // Intersection with left or right side of quad
                localOutputVector.x = Mathf.Sign(localPoint.x) * (meshSize.x / 2f);
                localOutputVector.y = meshSize.x / 2f * (localPoint.y / Mathf.Abs(localPoint.x));
            }
        }
        else
        {
            // No clamping required, return original vector
            localOutputVector = localPoint;
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

    float VectorTo360Angle(float x, float y)
    {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        // Remap from (-180, 180) to (0, 360)
        if (angle < 0f) angle += 360f;

        return angle;
    }

    EdgePoint RaycastToEdge(Vector2 localCastThroughPoint)
    {
        // Find the furthest end point of the raycast
        Vector2 pointOnEdge = ClampPointToQuad(localCastThroughPoint, true);
        Physics2D.LinecastNonAlloc(meshTransform.position, meshTransform.TransformPoint(pointOnEdge), nonAllocHits, carvingLayers);

        bool didRayHit = Physics2D.LinecastNonAlloc(meshTransform.position, meshTransform.TransformPoint(pointOnEdge), nonAllocHits, carvingLayers) > 0;

        // Info about where the ray landed, whether on geometry or the border of the quad
        Vector2 hitPoint = didRayHit ? meshTransform.InverseTransformPoint(nonAllocHits[0].point) : pointOnEdge;
        Vector2 hitNormal = didRayHit ? meshTransform.InverseTransformDirection(nonAllocHits[0].normal) : -DetermineQuadrant(pointOnEdge);

        return new EdgePoint(hitPoint, hitNormal, VectorTo360Angle(pointOnEdge.x, pointOnEdge.y), didRayHit);
    }

    EdgePoint FindPointBehind(Vector2 localCastThroughPoint)
    {
        // Find the furthest end point of the raycast
        Vector2 pointOnEdge = ClampPointToQuad(localCastThroughPoint, true);
        int hits = Physics2D.LinecastNonAlloc(meshTransform.position, meshTransform.TransformPoint(pointOnEdge), nonAllocHits, carvingLayers);
        int behindHitIndex = -1;
        Vector2 worldCastThroughPoint = meshTransform.TransformPoint(localCastThroughPoint);

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
        Vector2 hitNormal = didRayHit ? meshTransform.InverseTransformDirection(nonAllocHits[behindHitIndex].normal) : -DetermineQuadrant(pointOnEdge);

        return new EdgePoint(hitPoint, hitNormal, VectorTo360Angle(pointOnEdge.x, pointOnEdge.y), didRayHit);
    }

    Tuple<EdgePoint, EdgePoint> FindEdge(EdgePoint startEdge, EdgePoint endEdge)
    {
        EdgePoint minEdge = new EdgePoint(startEdge.position, startEdge.nextNormal, startEdge.angle, startEdge.onSurface);
        EdgePoint maxEdge = new EdgePoint(endEdge.position, endEdge.previousNormal, endEdge.angle, endEdge.onSurface);

        // Complete several iterations to solve the edge constraint
        for (int i = 0; i < maxEdgeSearchIterations; i++)
        {
            // Fire ray half-way between min and max
            Vector2 midpoint = (minEdge.position + maxEdge.position) / 2f;
            EdgePoint midEdgePoint = RaycastToEdge(midpoint);
            midEdgePoint.angle = VectorTo360Angle(midpoint.x, midpoint.y);

            // Based on continuity with previousEdgePoint, save ray hit data to min or max
            if (AreEdgePointsContinuous(minEdge, midEdgePoint))
            {
                // Raise minEdge to maximum continuous value with previousEdgePoint (ie. close to the edge, but not over)
                minEdge = midEdgePoint;
            }
            else
            {
                // Lower maxEdge to minimum non-continuous value with previousEdgePoint (ie. just over the edge)
                maxEdge = midEdgePoint;
            }

            // This is close enough for us to be satisfied, early out to save on performance
            if (Mathf.Abs(Mathf.DeltaAngle(minEdge.angle, maxEdge.angle)) <= edgeSearchMinAngleThreshold) break;
        }

        // Return both edges in order
        return new Tuple<EdgePoint, EdgePoint>(minEdge, maxEdge);
    }

    bool AreEdgePointsContinuous(EdgePoint minEdge, EdgePoint maxEdge)
    {
        // If the angle between the surface normals is too large, then we treat them as non-continuous
        if (Vector2.Angle(minEdge.nextNormal, maxEdge.previousNormal) > angleContinuityThreshold) return false;

        // Project points onto surface tangents of each other
        Vector2 projectedAOnTangentB = (Vector2)Vector3.ProjectOnPlane(minEdge.position - maxEdge.position, maxEdge.previousNormal) + maxEdge.position;
        Vector2 projectedBOnTangentA = (Vector2)Vector3.ProjectOnPlane(maxEdge.position - minEdge.position, minEdge.nextNormal) + minEdge.position;

        // Find maximum discrepancy between the point position and where it would need to be for perfect continuity
        float maxContinuityOffset = Mathf.Max(Vector2.Distance(minEdge.position, projectedAOnTangentB), Vector2.Distance(maxEdge.position, projectedBOnTangentA));

        if (maxContinuityOffset > projectionOffsetThreshold) return false;

        // All conditions met, the points are continuous
        return true;
    }

    int BinarySortedListInsertion(List<EdgePoint> edgePoints, EdgePoint point)
    {
        if (edgePoints.Count == 0)
        {
            edgePoints.Add(point);
            return 0;
        }

        int minIndex = 0;
        int maxIndex = edgePoints.Count - 1;

        // If the point we are adding is a new min or max of the dataset
        if (point.angle >= edgePoints[maxIndex].angle)
        {
            edgePoints.Add(point);
            return edgePoints.Count - 1;
        }
        else if (point.angle < edgePoints[minIndex].angle)
        {
            edgePoints.Insert(0, point);
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
                edgePoints.Insert(maxIndex, point);
                return maxIndex;
            }

            // Exact match, we can just add the point here
            if (point.angle == midpointElement.angle)
            {
                edgePoints.Insert(midpointIndex, point);
                return midpointIndex;
            }

            // Raise min index
            if (point.angle > midpointElement.angle)
            {
                minIndex = midpointIndex;
                continue;
            }

            // Lower max index
            if (point.angle < midpointElement.angle)
            {
                maxIndex = midpointIndex;
                continue;
            }
        }
    }

    List<EdgePoint> GetColliderVertices(Collider2D collider)
    {
        List<Vector2> vertices;

        switch (collider)
        {
            case BoxCollider2D:
                vertices = GetBoxVertices(collider as BoxCollider2D);
                break;
            default:
                // This collider type is not supported!
                return null;
        }

        List<EdgePoint> colliderEdgePoints = new List<EdgePoint>();

        // For each vertex in collider-space
        for (int i = 0; i < vertices.Count; i++)
        {
            // Find point data
            int nextIndex = (i + 1) % vertices.Count;
            int previousIndex = (i - 1 >= 0) ? (i - 1) : (vertices.Count - 1);
            Vector2 previousNormal = TangentToNormal((vertices[i] - vertices[previousIndex]));
            Vector2 nextNormal = TangentToNormal((vertices[nextIndex] - vertices[i]));
            float angleToPoint = VectorTo360Angle(vertices[i].x, vertices[i].y);

            // Create EdgePoint for it
            EdgePoint edgePoint = new EdgePoint(vertices[i], previousNormal, nextNormal, angleToPoint, true);
            colliderEdgePoints.Add(edgePoint);
        }

        return colliderEdgePoints;
    }

    List<Vector2> GetBoxVertices(BoxCollider2D boxCollider)
    {
        List<Vector2> boxVertices = new List<Vector2>(4);

        Vector2 halfTotalSize = boxCollider.size / 2f;
        Transform colliderTransform = boxCollider.transform;

        // Find points relative to the collider
        boxVertices.Add(new Vector2(-halfTotalSize.x, halfTotalSize.y));
        boxVertices.Add(new Vector2(halfTotalSize.x, halfTotalSize.y));
        boxVertices.Add(new Vector2(halfTotalSize.x, -halfTotalSize.y));
        boxVertices.Add(new Vector2(-halfTotalSize.x, -halfTotalSize.y));

        // Convert points relative to mesh
        for (int i = 0; i < boxVertices.Count; i++)
        {
            // To world-space
            boxVertices[i] = colliderTransform.TransformPoint(boxVertices[i]);

            // To mesh-space
            boxVertices[i] = meshTransform.InverseTransformPoint(boxVertices[i]);
        }

        return boxVertices;
    }

    bool IsPointFrontFacing(EdgePoint localPoint)
    {
        Vector2 pointToOrigin = Vector2.zero - localPoint.position;

        // Is the previous edge front-facing?
        if (Vector2.Dot(localPoint.previousNormal, pointToOrigin) > 0f) return true;

        // Is the next edge front-facing?
        if (Vector2.Dot(localPoint.nextNormal, pointToOrigin) > 0f) return true;

        // Both edges are back-facing
        return false;
    }

    bool IsPointOccluded(EdgePoint localPoint)
    {
        // Get points in world-space
        Vector2 worldOrigin = meshTransform.position;
        Vector2 worldPoint = meshTransform.TransformPoint(localPoint.position);

        // Linecast out to the point, if we make it >99.99% of the way there, the point is not occluded
        if (Physics2D.LinecastNonAlloc(worldOrigin, worldPoint, nonAllocHits, carvingLayers) > 0)
        {
            // The point is occluded if we hit something before the fraction threshold
            return (nonAllocHits[0].fraction < occlusionFractionThreshold);
        }

        return false;
    }

    int FindPointEdgeStatus(EdgePoint localPoint)
    {
        Vector2 pointToOrigin = Vector2.zero - localPoint.position;

        // The point is considered "on edge" when it has exactly one front-facing and one back-facing edge
        bool previousBackFacing = (Vector2.Dot(localPoint.previousNormal, pointToOrigin) <= 0f);
        bool nextBackFacing = (Vector2.Dot(localPoint.nextNormal, pointToOrigin) <= 0f);

        if (previousBackFacing && nextBackFacing == false) return -1;
        else if (previousBackFacing == false && nextBackFacing) return 1;
        else return 0;
    }

    Vector2 NormalToTangent(Vector2 normal)
    {
        return new Vector2(normal.y, -normal.x);
    }

    Vector2 TangentToNormal(Vector2 tangent)
    {
        return new Vector2(-tangent.y, tangent.x);
    }

    List<EdgePoint> ConstructMeshPoints()
    {
        List<EdgePoint> edgePoints = CalculateShapeProjectionVertices();

        FindProjectedEdgeVertices(edgePoints);

        FindGeometryIntersectionVertices(edgePoints);

        return edgePoints;
    }

    List<EdgePoint> CalculateShapeProjectionVertices()
    {
        List<EdgePoint> edgePoints = new List<EdgePoint>();

        Collider2D[] collidersInRange = Physics2D.OverlapBoxAll(meshTransform.position, meshSize * meshTransform.lossyScale, meshTransform.eulerAngles.z);

        // Create a clock-wise winding of points that encompass each collider in range
        for (int c = 0; c < collidersInRange.Length; c++)
        {
            List<EdgePoint> colliderVertices = GetColliderVertices(collidersInRange[c]);

            // For each point in the shape...
            for (int v = 0; v < colliderVertices.Count; v++)
            {
                // Is this point in range?
                if (IsPointInRange(colliderVertices[v].position))
                {
                    // Are either of this point's adjacent edges facing the center of the mesh?
                    if (cullBackFacingVertices == false || IsPointFrontFacing(colliderVertices[v]))
                    {
                        // Is this in-range, front-facing point occluded by other geometry?
                        if (cullOccludedVertices == false || IsPointOccluded(colliderVertices[v]) == false)
                        {
                            // This point is valid!
                            edgePoints.Add(colliderVertices[v]);
                        }
                    }
                }
            }
        }

        // Add mesh corners, if they're not occluded
        EdgePoint topRightPoint = new EdgePoint(topRight, Vector2.left, Vector2.down, VectorTo360Angle(topRight.x, topRight.y), false);
        if (IsPointOccluded(topRightPoint) == false) edgePoints.Add(topRightPoint);

        EdgePoint topLeftPoint = new EdgePoint(topLeft, Vector2.down, Vector2.right, VectorTo360Angle(topLeft.x, topLeft.y), false);
        if (IsPointOccluded(topLeftPoint) == false) edgePoints.Add(topLeftPoint);

        EdgePoint bottomLeftPoint = new EdgePoint(bottomLeft, Vector2.right, Vector2.up, VectorTo360Angle(bottomLeft.x, bottomLeft.y), false);
        if (IsPointOccluded(bottomLeftPoint) == false) edgePoints.Add(bottomLeftPoint);

        EdgePoint bottomRightPoint = new EdgePoint(bottomRight, Vector2.up, Vector2.left, VectorTo360Angle(bottomRight.x, bottomRight.y), false);
        if (IsPointOccluded(bottomRightPoint) == false) edgePoints.Add(bottomRightPoint);

        // Sort by angle value now, using the CompareTo() method implemented in EdgePoint
        edgePoints.Sort();

        return edgePoints;
    }

    void FindProjectedEdgeVertices(List<EdgePoint> edgePoints)
    {
        // Sweep around vertices in ascending angle order
        for (int i = 0; i < edgePoints.Count; i++)
        {
            // For all edge vertices...
            int edgePointStatus = FindPointEdgeStatus(edgePoints[i]);
            if (edgePointStatus != 0)
            {
                // Cast through them to find a point behind
                EdgePoint edgeDropoffPoint = FindPointBehind(edgePoints[i].position);

                // Does this point go before or after in the list?
                bool insertAfter = (edgePointStatus == 1);

                // Find insertion index
                int insertionIndex = insertAfter ? (i + 1) : i;

                // Mark EdgePoints as degenerate
                edgePoints[i].isDegenerate = true;
                edgeDropoffPoint.isDegenerate = true;

                // Update normals to fake continuity
                if (insertAfter)
                {
                    // This edge is at the end of the object
                    Vector2 edgeNormal = TangentToNormal(edgeDropoffPoint.position - edgePoints[i].position);
                    edgePoints[i].nextNormal = edgeNormal;
                    edgeDropoffPoint.previousNormal = edgeNormal;
                }
                else
                {
                    // This edge is at the start of the object
                    Vector2 edgeNormal = TangentToNormal(edgePoints[i].position - edgeDropoffPoint.position);
                    edgePoints[i].previousNormal = edgeNormal;
                    edgeDropoffPoint.nextNormal = edgeNormal;
                }

                // Insert the dropoff point, and ensure we skip it, as we know it isn't a vertex point
                edgePoints.Insert(insertionIndex, edgeDropoffPoint);
                i += 1;
            }
        }
    }

    void FindGeometryIntersectionVertices(List<EdgePoint> edgePoints)
    {
        // Sweep around vertices in ascending angle order
        for (int i = 0; i < edgePoints.Count; i++)
        {
            int nextIndex = ((i + 1) % edgePoints.Count);

            // If two consecutive points are not continuous, we need to find an edge between them
            if (AreEdgePointsContinuous(edgePoints[i], edgePoints[nextIndex]) == false)
            {
                // Search for a set of vertices that approximate the geometric intersection causing the discontinuity
                Tuple<EdgePoint, EdgePoint> detailPoints = FindEdge(edgePoints[i], edgePoints[nextIndex]);

                int insertionIndex1 = -1;
                int insertionIndex2 = -1;
                if (detailPoints.Item1 != edgePoints[i]) insertionIndex1 = BinarySortedListInsertion(edgePoints, detailPoints.Item1); // The minEdge is a different point to what it started as, insert into list
                if (detailPoints.Item2 != edgePoints[nextIndex]) insertionIndex2 = BinarySortedListInsertion(edgePoints, detailPoints.Item2); // The maxEdge is a different point to what it started as, insert into list
                if (insertionIndex2 <= insertionIndex1) insertionIndex1 += 1;

                // Update normals and stuff
                edgePoints[insertionIndex1].previousNormal = edgePoints[insertionIndex1 - 1 >= 0 ? insertionIndex1 - 1 : edgePoints.Count - 1].nextNormal;
                edgePoints[insertionIndex1].nextNormal = edgePoints[insertionIndex2].previousNormal;

                // Ensure we skip to checking the max detail point next
                i = (Mathf.Max(i, insertionIndex1, insertionIndex2 - 1));
            }
        }
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
            vertices[i + 1] = edgePoints[i].position;
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
            int firstIndex = (i < triangleIndexCount - 3) ? (i / 3) + 2 : 1;
            int secondIndex = (i / 3) + 1;

            // First edge vertex (loops back around to share first edge vertex for last triangle)
            triangles[i] = firstIndex;

            // Second edge vertex
            triangles[i + 1] = secondIndex;

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