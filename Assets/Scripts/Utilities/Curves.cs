using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Curves
{
    public enum CurveStyle
    {
        Linear,
        Quadratic,
        SquareRoot,
        Sine,
        InverseSine,
        Arc,
        InverseArc
    }

    /// <summary>
    /// Evaluate an easing curve of a given type
    /// </summary>
    /// <param name="style">The style of curve to evaluate</param>
    /// <param name="x">The time parameter (between 0 and 1), as this increases the output will too</param>
    /// <returns>Output of the desired function</returns>
    public static float Evaluate(CurveStyle style, float x)
    {
        x = Mathf.Clamp01(x);

        switch (style)
        {
            case CurveStyle.Linear:
                return Linear(x);

            case CurveStyle.Quadratic:
                return Quadratic(x);

            case CurveStyle.SquareRoot:
                return SquareRoot(x);

            case CurveStyle.Sine:
                return Sine(x);

            case CurveStyle.InverseSine:
                return InverseSine(x);

            case CurveStyle.Arc:
                return Arc(x);

            case CurveStyle.InverseArc:
                return InverseArc(x);
        }

        return 0f;
    }

    private static float Linear(float t)
    {
        return t;
    }

    private static float Quadratic(float t)
    {
        return t * t;
    }

    private static float SquareRoot(float t)
    {
        return Mathf.Sqrt(t);
    }

    private static float Sine(float t)
    {
        return 1f - ((Mathf.Cos(Mathf.PI * t) + 1f) / 2f);
    }

    private static float InverseSine(float t)
    {
        return Mathf.Acos(1f - (t * 2f)) / Mathf.PI;
    }

    private static float Arc(float t)
    {
        return Mathf.Sin(Mathf.Acos(t - 1f));
    }

    private static float InverseArc(float t)
    {
        return 1f - Mathf.Sin(Mathf.Acos(t));
    }
}
