using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtilities
{
    public static bool IsPointInQuad(Vector2 localPoint, Vector2 quadSize)
    {
        return (Mathf.Abs(localPoint.x) <= quadSize.x / 2f && Mathf.Abs(localPoint.y) <= quadSize.y / 2f);
    }

    public static Vector2 ClampPointToQuad(Vector2 localPoint, Vector2 quadSize, bool edgeOnly)
    {
        Vector2 localOutputVector = Vector2.zero;

        // If the point lies outside the bounds of the quad, or we want to project onto it
        if (edgeOnly || IsPointInQuad(localPoint, quadSize) == false)
        {
            // Determine side of intersection / projection
            if (Mathf.Abs(localPoint.x) * quadSize.y <= Mathf.Abs(localPoint.y) * quadSize.x)
            {
                // Intersection with top or bottom side of quad
                localOutputVector.x = (quadSize.y / 2f) * (localPoint.x / Mathf.Abs(localPoint.y));
                localOutputVector.y = Mathf.Sign(localPoint.y) * (quadSize.y / 2f);
            }
            else
            {
                // Intersection with left or right side of quad
                localOutputVector.x = Mathf.Sign(localPoint.x) * (quadSize.x / 2f);
                localOutputVector.y = quadSize.x / 2f * (localPoint.y / Mathf.Abs(localPoint.x));
            }
        }
        else
        {
            // No clamping / projection required, return original vector
            localOutputVector = localPoint;
        }

        return localOutputVector;
    }

    public static Vector2 DetermineQuadrant(Vector2 localPoint, Vector2 quadSize)
    {
        if (Mathf.Abs(localPoint.x) * quadSize.y <= Mathf.Abs(localPoint.y) * quadSize.x)
        {
            // Vertical quadrant
            if (localPoint.y > 0f) return Vector2.up;
            if (localPoint.y < 0f) return Vector2.down;
        }
        else
        {
            // Horizontal quadrant
            if (localPoint.x > 0f) return Vector2.right;
            if (localPoint.x < 0f) return Vector2.left;
        }

        // No quadrant
        return Vector2.zero;
    }

    public static float VectorTo360Angle(float x, float y)
    {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        // Remap from (-180, 180) to (0, 360)
        if (angle < 0f) angle += 360f;

        return angle;
    }

    public static Vector2 AngleToVector(float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        Vector2 direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        return direction;
    }

    public static Vector2 NormalToTangent(Vector2 normal)
    {
        return new Vector2(normal.y, -normal.x);
    }

    public static Vector2 TangentToNormal(Vector2 tangent)
    {
        return new Vector2(-tangent.y, tangent.x);
    }

    public static Vector2 FindPointOnLine(Vector2 point, Vector2 tangent, Vector2 tangentPoint)
    {
        // Distance from the tangentPoint to the projected point on the tangent
        float projectionLength = Vector2.Dot(point - tangentPoint, tangent) / tangent.magnitude;

        // Convert to vector offset from tangent point
        return (projectionLength * tangent.normalized) + tangentPoint;
    }
}
