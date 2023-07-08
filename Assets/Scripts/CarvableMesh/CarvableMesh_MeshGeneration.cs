using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CarvableMesh
{
    #region Private Methods 

    private Vector3[] CreateVertices(List<EdgePoint> edgePoints)
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

    private int[] FormTriangles(Vector3[] vertices)
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

    private Vector2[] CalculateUVs(Vector3[] vertices)
    {
        Vector2[] UVs = new Vector2[vertices.Length];

        Vector2 inverseMeshSize = new Vector2(1f / meshSize.x, 1f / meshSize.y);
        Vector2 halfMeshSize = meshSize / 2f;

        // Setup mesh UVs
        for (int i = 0; i < vertices.Length; i++)
        {
            UVs[i].x = Mathf.Clamp01((vertices[i].x + halfMeshSize.x) * inverseMeshSize.x);
            UVs[i].y = Mathf.Clamp01((vertices[i].y + halfMeshSize.y) * inverseMeshSize.y);
        }

        return UVs;
    }

    #endregion
}
