using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InspectorUtilities
{
    /// <summary>
    /// Draws a header, identically to the standard inspector header attribute
    /// </summary>
    /// <param name="headerText">The text you want to display in the header</param>
    public static void AddHeader(string headerText)
    {
        // Store the enabled state prior to placing this header
        bool isEnabled = GUI.enabled;

        // Ensure the header appears enabled
        GUI.enabled = true;

        // Default attribute headers add some space above themselves
        EditorGUILayout.Space(10f);

        // Ensure the text is bold!
        EditorGUILayout.LabelField(headerText, EditorStyles.boldLabel);

        // Reset the enabled state
        GUI.enabled = isEnabled;
    }

    public static void MinMaxLabelledSlider(GUIContent attributes, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        // Start drawing horizontally
        EditorGUILayout.BeginHorizontal();

        // Draw the label for this property
        EditorGUILayout.PrefixLabel(attributes);

        // First input box
        EditorGUIUtility.fieldWidth = 40f;
        minValue = Mathf.Min(EditorGUILayout.FloatField(float.Parse(minValue.ToString("F2")), GUILayout.MaxWidth(40f)), maxValue);

        // Draw slider
        EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);

        // Second input box
        maxValue = Mathf.Max(EditorGUILayout.FloatField(float.Parse(maxValue.ToString("F2")), GUILayout.MaxWidth(40f)), minValue);

        // Stop drawing horizontally
        EditorGUILayout.EndHorizontal();
    }

    public static void MinMaxLabelledIntSlider(GUIContent attributes, ref int minValue, ref int maxValue, int minLimit, int maxLimit)
    {
        // Start drawing horizontally
        EditorGUILayout.BeginHorizontal();

        // Draw the label for this property
        EditorGUILayout.PrefixLabel(attributes);

        // First input box
        EditorGUIUtility.fieldWidth = 40f;
        minValue = Mathf.Clamp(EditorGUILayout.IntField(minValue, GUILayout.MaxWidth(40f)), minLimit, maxValue);

        // Draw slider using floats and convert back to ints
        float minValueFloat = minValue;
        float maxValueFloat = maxValue;
        EditorGUILayout.MinMaxSlider(ref minValueFloat, ref maxValueFloat, minLimit, maxLimit);
        minValue = Mathf.RoundToInt(minValueFloat);
        maxValue = Mathf.RoundToInt(maxValueFloat);

        // Second input box
        maxValue = Mathf.Clamp(EditorGUILayout.IntField(maxValue, GUILayout.MaxWidth(40f)), minValue, maxLimit);

        // Stop drawing horizontally
        EditorGUILayout.EndHorizontal();
    }

    public static void DrawDivider(float halfHeight)
    {
        EditorGUILayout.Space(halfHeight);

        Rect drawingRect = EditorGUILayout.BeginHorizontal();
        drawingRect.x -= 25f;
        drawingRect.width += 50f;
        drawingRect.height = 1f;
        EditorGUI.DrawRect(drawingRect, Color.grey);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(halfHeight + drawingRect.height);
    }
}
