using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CarvableMesh
{
    #region Private Methods

    // Need to add more shapes
    private List<EdgePoint> GetColliderVertices(Collider2D collider)
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

        // For each vertex in local-space
        for (int i = 0; i < vertices.Count; i++)
        {
            // Find point data
            int nextIndex = (i + 1) % vertices.Count;
            int previousIndex = (i - 1 >= 0) ? (i - 1) : (vertices.Count - 1);
            Vector2 previousNormal = MathUtilities.TangentToNormal((vertices[i] - vertices[previousIndex]));
            Vector2 nextNormal = MathUtilities.TangentToNormal(vertices[nextIndex] - vertices[i]);
            float angleToPoint = MathUtilities.VectorTo360Angle(vertices[i].x, vertices[i].y);

            // Create EdgePoint for it
            EdgePoint edgePoint = new EdgePoint(vertices[i], previousNormal, nextNormal, angleToPoint, true);
            colliderEdgePoints.Add(edgePoint);
        }

        return colliderEdgePoints;
    }

    private List<Vector2> GetBoxVertices(BoxCollider2D boxCollider)
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

    #endregion
}
