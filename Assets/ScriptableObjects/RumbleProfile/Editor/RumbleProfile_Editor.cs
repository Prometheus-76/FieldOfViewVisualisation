using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RumbleProfile))]
public class RumbleProfile_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        RumbleProfile rumbleProfile = target as RumbleProfile;
        GUIContent attributes;

        InspectorUtilities.AddHeader("Low Frequency");

        attributes = new GUIContent("Low Frequency Rumble", "Whether to use the low frequency rumble");
        rumbleProfile.lowFrequencyRumble = EditorGUILayout.Toggle(attributes, rumbleProfile.lowFrequencyRumble);

        // Show other low frequency rumble settings
        if (rumbleProfile.lowFrequencyRumble)
        {
            EditorGUILayout.Space(10f);
            
            attributes = new GUIContent("Strength", "How strong the low frequency rumble strength is");
            rumbleProfile.lowFrequencyStrength = EditorGUILayout.Slider(attributes, rumbleProfile.lowFrequencyStrength, 0f, 1f);

            EditorGUILayout.Space(10f);

            attributes = new GUIContent("In Duration", "The time it takes to transition to full rumble strength (in seconds)");
            rumbleProfile.lowFrequencyInDuration = Mathf.Max(0f, EditorGUILayout.FloatField(attributes, rumbleProfile.lowFrequencyInDuration));

            attributes = new GUIContent("Hold Duration", "The time the rumble remains at full strength (in seconds)");
            rumbleProfile.lowFrequencyHoldDuration = Mathf.Max(0f, EditorGUILayout.FloatField(attributes, rumbleProfile.lowFrequencyHoldDuration));

            attributes = new GUIContent("Out Duration", "The time it takes to transition from full rumble strength (in seconds)");
            rumbleProfile.lowFrequencyOutDuration = Mathf.Max(0f, EditorGUILayout.FloatField(attributes, rumbleProfile.lowFrequencyOutDuration));

            if (rumbleProfile.lowFrequencyInDuration > 0f || rumbleProfile.lowFrequencyOutDuration > 0f) EditorGUILayout.Space(10f);

            if (rumbleProfile.lowFrequencyInDuration > 0f)
            {
                attributes = new GUIContent("In Curve", "The ramp up style of the rumble");
                rumbleProfile.lowFrequencyInCurve = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, rumbleProfile.lowFrequencyInCurve);
            }

            if (rumbleProfile.lowFrequencyOutDuration > 0f)
            {
                attributes = new GUIContent("Out Curve", "The ramp down style of the rumble");
                rumbleProfile.lowFrequencyOutCurve = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, rumbleProfile.lowFrequencyOutCurve);
            }
        }

        InspectorUtilities.AddHeader("High Frequency");

        attributes = new GUIContent("High Frequency Rumble", "Whether to use the high frequency rumble");
        rumbleProfile.highFrequencyRumble = EditorGUILayout.Toggle(attributes, rumbleProfile.highFrequencyRumble);

        // Show other high frequency rumble settings
        if (rumbleProfile.highFrequencyRumble)
        {
            EditorGUILayout.Space(10f);
            
            attributes = new GUIContent("Strength", "How strong the high frequency rumble strength is");
            rumbleProfile.highFrequencyStrength = EditorGUILayout.Slider(attributes, rumbleProfile.highFrequencyStrength, 0f, 1f);

            EditorGUILayout.Space(10f);

            attributes = new GUIContent("In Duration", "The time it takes to transition to full rumble strength (in seconds)");
            rumbleProfile.highFrequencyInDuration = Mathf.Max(0f, EditorGUILayout.FloatField(attributes, rumbleProfile.highFrequencyInDuration));

            attributes = new GUIContent("Hold Duration", "The time the rumble remains at full strength (in seconds)");
            rumbleProfile.highFrequencyHoldDuration = Mathf.Max(0f, EditorGUILayout.FloatField(attributes, rumbleProfile.highFrequencyHoldDuration));

            attributes = new GUIContent("Out Duration", "The time it takes to transition from full rumble strength (in seconds)");
            rumbleProfile.highFrequencyOutDuration = Mathf.Max(0f, EditorGUILayout.FloatField(attributes, rumbleProfile.highFrequencyOutDuration));

            if (rumbleProfile.highFrequencyInDuration > 0f || rumbleProfile.highFrequencyOutDuration > 0f) EditorGUILayout.Space(10f);

            if (rumbleProfile.highFrequencyInDuration > 0f)
            {
                attributes = new GUIContent("In Curve", "The ramp up style of the rumble");
                rumbleProfile.highFrequencyInCurve = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, rumbleProfile.highFrequencyInCurve);
            }

            if (rumbleProfile.highFrequencyOutDuration > 0f)
            {
                attributes = new GUIContent("Out Curve", "The ramp down style of the rumble");
                rumbleProfile.highFrequencyOutCurve = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, rumbleProfile.highFrequencyOutCurve);
            }
        }
    }
}
