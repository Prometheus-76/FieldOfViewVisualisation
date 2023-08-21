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

}
