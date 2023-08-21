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
        if (x <= 0f) return 0f;
        if (x >= 1f) return 1f;

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
        return Mathf.Clamp01(t);
    }

    private static float Quadratic(float t)
    {
        float t01 = Mathf.Clamp01(t);
        return t01 * t01;
    }

    private static float SquareRoot(float t)
    {
        float t01 = Mathf.Clamp01(t);
        return Mathf.Sqrt(t01);
    }

    private static float Sine(float t)
    {
        float t01 = Mathf.Clamp01(t);
        return 1f - ((Mathf.Cos(Mathf.PI * t01) + 1f) / 2f);
    }

    private static float InverseSine(float t)
    {
        float t01 = Mathf.Clamp01(t);
        return Mathf.Acos(1f - (t01 * 2f)) / Mathf.PI;
    }

    private static float Arc(float t)
    {
        float t01 = Mathf.Clamp01(t);
        return Mathf.Sin(Mathf.Acos(t01 - 1f));
    }

    private static float InverseArc(float t)
    {
        float t01 = Mathf.Clamp01(t);
        return 1f - Mathf.Sin(Mathf.Acos(t01));
    }
}
