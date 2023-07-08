using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CarvableMesh
{
    #region Data Structures

    private class EdgePoint : IComparable<EdgePoint>
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

    private bool IsPointFrontFacing(EdgePoint localPoint)
    {
        Vector2 pointToOrigin = Vector2.zero - localPoint.position;

        // Is the previous edge front-facing?
        if (Vector2.Dot(localPoint.previousNormal, pointToOrigin) > 0f) return true;

        // Is the next edge front-facing?
        if (Vector2.Dot(localPoint.nextNormal, pointToOrigin) > 0f) return true;

        // Both edges are back-facing
        return false;
    }

    private bool IsPointOccluded(EdgePoint localPoint, Collider2D expectedCollider)
    {
        // Get points in world-space
        Vector2 worldOrigin = meshTransform.position;
        Vector2 worldPoint = meshTransform.TransformPoint(localPoint.position);

        // Linecast out to the point, if we make it >99.99% of the way there, the point is not occluded
        if (Physics2D.LinecastNonAlloc(worldOrigin, worldPoint, nonAllocHits, carvingLayers) > 0)
        {
            // We hit something unexpected, the object is occluded
            if (nonAllocHits[0].collider != expectedCollider) return true;

            // We must have hit something expected, was it closer than we'd expect?
            return (nonAllocHits[0].fraction <= 0.999f);
        }

        return false;
    }

    private int FindPointEdgeStatus(EdgePoint localPoint)
    {
        Vector2 pointToOrigin = Vector2.zero - localPoint.position;

        // The point is considered "on edge" when it has exactly one front-facing and one back-facing edge
        bool previousBackFacing = (Vector2.Dot(localPoint.previousNormal, pointToOrigin) <= 0f);
        bool nextBackFacing = (Vector2.Dot(localPoint.nextNormal, pointToOrigin) <= 0f);

        if (previousBackFacing && nextBackFacing == false) return -1;
        else if (previousBackFacing == false && nextBackFacing) return 1;
        else return 0;
    }

    private bool AreEdgePointsContinuous(EdgePoint minEdge, EdgePoint maxEdge)
    {
        // If the angle between the surface normals is too large, then we treat them as non-continuous
        if (Vector2.Angle(minEdge.nextNormal, maxEdge.previousNormal) > angleContinuityThreshold) return false;

        // Project points onto surface tangents of each other
        Vector2 projectedAOnTangentB = MathUtilities.FindPointOnLine(minEdge.position, MathUtilities.NormalToTangent(maxEdge.previousNormal), maxEdge.position);
        Vector2 projectedBOnTangentA = MathUtilities.FindPointOnLine(maxEdge.position, MathUtilities.NormalToTangent(minEdge.nextNormal), minEdge.position);

        // Find maximum discrepancy between the point position and where it would need to be for perfect continuity
        float maxContinuityOffset = Mathf.Max((minEdge.position - projectedAOnTangentB).sqrMagnitude, (maxEdge.position - projectedBOnTangentA).sqrMagnitude);

        if (maxContinuityOffset > (projectionOffsetThreshold * projectionOffsetThreshold)) return false;

        // All conditions met, the points are continuous
        return true;
    }
}
