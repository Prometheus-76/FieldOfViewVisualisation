using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtilities
{
    public const float E = 2.71828182f;

    public static bool IsPointInQuad(Vector2 localPoint, Vector2 halfQuadSize)
    {
        return (Mathf.Abs(localPoint.x) <= halfQuadSize.x && Mathf.Abs(localPoint.y) <= halfQuadSize.y);
    }

    public static Vector2 ClampPointToQuad(Vector2 localPoint, Vector2 halfQuadSize, bool edgeOnly)
    {
        Vector2 localOutputVector = Vector2.zero;

        // If the point lies outside the bounds of the quad, or we want to project onto it
        if (edgeOnly || IsPointInQuad(localPoint, halfQuadSize) == false)
        {
            // Determine side of intersection / projection
            if (Mathf.Abs(localPoint.x) * halfQuadSize.y <= Mathf.Abs(localPoint.y) * halfQuadSize.x)
            {
                // Intersection with top or bottom side of quad
                localOutputVector.x = halfQuadSize.y * (localPoint.x / Mathf.Abs(localPoint.y));
                localOutputVector.y = Mathf.Sign(localPoint.y) * halfQuadSize.y;
            }
            else
            {
                // Intersection with left or right side of quad
                localOutputVector.x = Mathf.Sign(localPoint.x) * halfQuadSize.x;
                localOutputVector.y = halfQuadSize.x * (localPoint.y / Mathf.Abs(localPoint.x));
            }
        }
        else
        {
            // No clamping / projection required, return original vector
            localOutputVector = localPoint;
        }

        return localOutputVector;
    }

    public static Vector2 DetermineQuadrant(Vector2 localPoint, Vector2 halfQuadSize)
    {
        if (Mathf.Abs(localPoint.x) * halfQuadSize.y <= Mathf.Abs(localPoint.y) * halfQuadSize.x)
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

    public static Vector2 Rotate90CW(Vector2 normal)
    {
        return new Vector2(normal.y, -normal.x);
    }

    public static Vector2 Rotate90CCW(Vector2 tangent)
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

    public static int NextHighestPowerOf2(int input)
    {
        // Positive values only
        input = Mathf.Max(input, 1);

        // Effectively makes this >= rather than just >
        input -= 1;

        input |= input >> 1;
        input |= input >> 2;
        input |= input >> 4;
        input |= input >> 8;
        input |= input >> 16; // 32-bit

        input += 1;

        return input;
    }

    public enum PowerOf2
    {
        _1,
        _2,
        _4,
        _8,
        _16,
        _32,
        _64,
        _128,
        _256,
        _512,
        _1024,
        _2048,
        _4096,
        _8192
    }

    public static bool OverlapCircleRect(Vector2 circleCenter, float sqrCircleRadius, Vector2 halfRectSize)
    {
        // Point closest inside bounds of rect
        Vector2 clampedPosition = circleCenter;
        clampedPosition.x = Mathf.Clamp(circleCenter.x, -halfRectSize.x, halfRectSize.x);
        clampedPosition.y = Mathf.Clamp(circleCenter.y, -halfRectSize.y, halfRectSize.y);

        // If distance to this point from the circle's center is less than the radius, then they're overlapping
        float sqrDistanceToClamped = (clampedPosition - circleCenter).sqrMagnitude;

        return sqrDistanceToClamped <= sqrCircleRadius;
    }

    public static bool OverlapCirclePolygon(Vector2 circleCenter, float sqrCircleRadius, Vector2[] localPolygonPointsCCW, bool testOutlineOnly)
    {
        // If the center is within the polygon, the circle is overlapping
        if (testOutlineOnly == false && IsPointWithinPolygon(circleCenter, localPolygonPointsCCW)) return true;

        for (int i = 0; i < localPolygonPointsCCW.Length; i++)
        {
            // Check if a vertex is within the circle, early out
            float sqrDistanceToVertex = (localPolygonPointsCCW[i] - circleCenter).sqrMagnitude;
            if (sqrDistanceToVertex <= sqrCircleRadius) return true;

            int nextIndex = (i + 1);
            if (nextIndex >= localPolygonPointsCCW.Length) nextIndex = 0;

            // Get closest point on line segment
            Tuple<Vector2, bool> closestPointClamped = ClosestPointOnLine(localPolygonPointsCCW[i], localPolygonPointsCCW[nextIndex], circleCenter, true);
            
            // If this point was clamped onto the line then it is on a vertex,
            // So we can continue, because either:
            // - It's in range of the next vertex, and we find out next iteration, or...
            // - It isn't within range of this line segment at all (because we would have dealt with it this iteration)
            if (closestPointClamped.Item2 == true) continue;

            // Check if this mid-segment point is within the circle
            float sqrDistanceToLine = (closestPointClamped.Item1 - circleCenter).sqrMagnitude;
            if (sqrDistanceToLine <= sqrCircleRadius) return true;
        }

        // The circle and polygon are not overlapping
        return false;
    }

    public static Tuple<Vector2, bool> ClosestPointOnLine(Vector2 start, Vector2 end, Vector2 point, bool clampPoint)
    {
        Tuple<Vector2, bool> result;

        Vector2 startToEnd = end - start;
        Vector2 startToPoint = point - start;

        float projection = Vector2.Dot(startToPoint, startToEnd);
        float sqrSegmentLength = startToEnd.sqrMagnitude;
        float interpolant = projection / sqrSegmentLength;

        if (clampPoint && interpolant <= 0f)
        {
            // Clamped to start
            result = new Tuple<Vector2, bool>(start, true);
        }
        else if (clampPoint && interpolant >= 1f)
        {
            // Clamped to end
            result = new Tuple<Vector2, bool>(end, true);
        }
        else
        {
            // Unclamped
            result = new Tuple<Vector2, bool>(start + (startToEnd * interpolant), false);
        }

        return result;
    }

    public static bool IsPointWithinPolygon(Vector2 point, Vector2[] localPolygonPointsCCW)
    {
        uint rayIntersections = 0;

        // Raycast (in the right direction) against each edge from the designated point against all sides of the polygon
        // If the number of intersections is odd, the point is inside the shape, otherwise the point is outside
        for (int i = 0; i < localPolygonPointsCCW.Length; i++)
        {
            int nextIndex = (i + 1);
            if (nextIndex >= localPolygonPointsCCW.Length) nextIndex = 0;

            // Ensure point is to the left of the line (as we are casting right)
            if (point.x <= Mathf.Max(localPolygonPointsCCW[i].x, localPolygonPointsCCW[nextIndex].x))
            {
                Vector2 upperPoint = localPolygonPointsCCW[i].y >= localPolygonPointsCCW[nextIndex].y ? localPolygonPointsCCW[i] : localPolygonPointsCCW[nextIndex];
                Vector2 lowerPoint = localPolygonPointsCCW[i].y < localPolygonPointsCCW[nextIndex].y ? localPolygonPointsCCW[i] : localPolygonPointsCCW[nextIndex];

                // Ensure the point is at the correct height to hit the line
                if (point.y >= Mathf.Min(localPolygonPointsCCW[i].y, localPolygonPointsCCW[nextIndex].y) &&
                    point.y <= Mathf.Max(localPolygonPointsCCW[i].y, localPolygonPointsCCW[nextIndex].y))
                {
                    float yRange = upperPoint.y - lowerPoint.y;
                    
                    // Edge case: Prevents detecting 2 intersections if the tested vertex is within the polygon
                    // and it's y-value is equal to that of a vertex which is raycasted against (and therefore said vertex is also shared with another edge)
                    if (upperPoint.y == point.y) continue;

                    // Line tangent is +/- (1, 0), ie. parallel and aligned with the ray
                    if (yRange == 0f)
                    {
                        rayIntersections += 1;
                        continue;
                    }

                    float interpolantBasedOnY = (point.y - lowerPoint.y) / yRange;
                    float xValue = ((upperPoint.x - lowerPoint.x) * interpolantBasedOnY) + lowerPoint.x;

                    // The ray origin is to the left of the plane
                    if (point.x <= xValue)
                    {
                        rayIntersections += 1;
                        continue;
                    }
                }
            }
        }

        // Check if intersection count is even (if final bit is flipped, number is odd)
        // Doing this saves a modulo operator call
        return ((rayIntersections & 1) == 1);
    }

    public static float Remap(float input, float inMin, float inMax, float outMin, float outMax)
    {
        float inputRange = (inMax - inMin);
        float outputRange = (outMax - outMin);

        float input01 = (input - inMin) / inputRange;
        float output = (input01 * outputRange) + outMin;

        return output;
    }

    public static float Tanh(float input)
    {
        float eulerExponent = Mathf.Pow(E, input);
        float inverseEulerExponent = 1f / eulerExponent;

        return (eulerExponent - inverseEulerExponent) / (eulerExponent + inverseEulerExponent);
    }

    public static float DampValueToRange(float range, float gradient, float input)
    {
        if (range <= 0f || gradient <= 0f || input == 0f) return 0f;

        return range * Tanh((gradient * input) / range);
    }

    public static float UndampValueFromRange(float range, float gradient, float input)
    {
        if (range <= 0f || gradient <= 0f || input == 0f) return 0f;

        const float asymptoteOffset = 0.01f;

        if (asymptoteOffset >= range) return 0f;
        else input = Mathf.Clamp(input, -range + asymptoteOffset, range - asymptoteOffset);

        float scaledInput = input / range;
        float rangeScalar = range / (2f * gradient);

        return rangeScalar * Mathf.Log((1f + scaledInput) / (1f - scaledInput));
    }
}
