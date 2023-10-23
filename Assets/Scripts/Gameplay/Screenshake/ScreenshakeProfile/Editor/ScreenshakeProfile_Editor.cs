using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScreenshakeProfile))]
public class ScreenshakeProfile_Editor : Editor
{
    private ScreenshakeHandle testEventHandle = null;

    public override void OnInspectorGUI()
    {
        // The instance we are modifying
        ScreenshakeProfile screenshakeProfile = target as ScreenshakeProfile;
        GUIContent attributes = new GUIContent();

        EditorGUI.BeginChangeCheck();

        // Basic configuration, should always be shown
        InspectorUtilities.AddHeader("Basic Configuration");
        attributes.text = "Shake Type";
        attributes.tooltip = "<b>Discrete</b> - Controlled internally within the ScreenshakeSystem over a fixed duration.\n\n<b>Continuous</b> - Controlled by another script in real-time using a handle object.";
        screenshakeProfile.shakeType = (ScreenshakeSystem.ShakeType)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.shakeType);

        if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Discrete)
        {
            attributes.text = "Shake Duration";
            attributes.tooltip = "How long this discrete shake event will last for.";
            float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.shakeDuration);
            screenshakeProfile.shakeDuration = Mathf.Max(floatFieldResult, 0.01f);
        }

        InspectorUtilities.DrawDivider(15f);

        DrawPositionSection(ref screenshakeProfile, ref attributes);

        InspectorUtilities.DrawDivider(15f);

        DrawRotationSection(ref screenshakeProfile, ref attributes);

        InspectorUtilities.DrawDivider(15f);

        DrawZoomSection(ref screenshakeProfile, ref attributes);

        // Check if we made changes to the instance, and if so, mark the object as needing to save
        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(screenshakeProfile);
    }

    private bool DrawPositionSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        EditorStyles.label.fontStyle = FontStyle.Bold;

        attributes.text = "Use Position";
        attributes.tooltip = "Whether this shake event should manipulate the position of the scene camera.";
        screenshakeProfile.usePosition = EditorGUILayout.Toggle(attributes, screenshakeProfile.usePosition);

        EditorStyles.label.fontStyle = FontStyle.Normal;

        if (screenshakeProfile.usePosition)
        {
            EditorGUILayout.Space(10f);

            if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Discrete)
            {
                DrawDiscretePositionSection(ref screenshakeProfile, ref attributes);
            }
            else if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Continuous)
            {
                DrawContinuousPositionSection(ref screenshakeProfile, ref attributes);
            }
        }

        return screenshakeProfile.usePosition;
    }

    private void DrawDiscretePositionSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Discrete Style";
        attributes.tooltip = "Whether the position is manipulated using noise, or with a directional animation.";
        screenshakeProfile.discretePositionStyle = (ScreenshakeSystem.DiscreteType)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discretePositionStyle);

        if (screenshakeProfile.discretePositionStyle == ScreenshakeSystem.DiscreteType.Noise)
        {
            DrawDiscretePositionNoiseSection(ref screenshakeProfile, ref attributes);
        }
        else if (screenshakeProfile.discretePositionStyle == ScreenshakeSystem.DiscreteType.Animation)
        {
            DrawDiscretePositionAnimationSection(ref screenshakeProfile, ref attributes);
        }
    }

    private void DrawDiscretePositionNoiseSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Noise Magnitude";
        attributes.tooltip = "How far the camera will be moved by this shake event (in world-units).";
        Vector2 vector2FieldResult = EditorGUILayout.Vector2Field(attributes, screenshakeProfile.discretePositionNoiseMagnitude);
        vector2FieldResult.x = Mathf.Max(vector2FieldResult.x, 0f);
        vector2FieldResult.y = Mathf.Max(vector2FieldResult.y, 0f);
        screenshakeProfile.discretePositionNoiseMagnitude = vector2FieldResult;

        attributes.text = "Noise Frequency";
        attributes.tooltip = "How fast the camera will be moved by this shake event.";
        vector2FieldResult = EditorGUILayout.Vector2Field(attributes, screenshakeProfile.discretePositionNoiseFrequency);
        vector2FieldResult.x = Mathf.Max(vector2FieldResult.x, 0.01f);
        vector2FieldResult.y = Mathf.Max(vector2FieldResult.y, 0.01f);
        screenshakeProfile.discretePositionNoiseFrequency = vector2FieldResult;

        EditorGUILayout.Space(10f);

        attributes.text = "Use Custom Curve";
        attributes.tooltip = "Whether to use a custom curve defined by splines (more expensive but more accurate) or a set of equations describing implicit curves.";
        screenshakeProfile.discretePositionNoiseUseCustomCurve = EditorGUILayout.Toggle(attributes, screenshakeProfile.discretePositionNoiseUseCustomCurve);

        EditorGUILayout.Space(10f);

        if (screenshakeProfile.discretePositionNoiseUseCustomCurve)
        {
            attributes.text = "Noise Curve";
            attributes.tooltip = "How intense the effect of this shake event is over time, scales using the magnitude value.";
            screenshakeProfile.discretePositionNoiseCustomCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discretePositionNoiseCustomCurve);
        }
        else
        {
            attributes.text = "Noise Curve In";
            attributes.tooltip = "How intense the effect of this shake event is over time, up the midpoint, scales using the magnitude value.";
            screenshakeProfile.discretePositionNoiseFixedCurveIn = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discretePositionNoiseFixedCurveIn);

            attributes.text = "Noise Curve Midpoint";
            attributes.tooltip = "Where the midpoint of this shake is.";
            InspectorUtilities.MinMaxLabelledSlider(attributes, ref screenshakeProfile.discretePositionNoiseFixedCurveMidpointMin, ref screenshakeProfile.discretePositionNoiseFixedCurveMidpointMax, 0f, 1f);

            attributes.text = "Noise Curve Out";
            attributes.tooltip = "How intense the effect of this shake event is over time, past the midpoint, scales using the magnitude value.";
            screenshakeProfile.discretePositionNoiseFixedCurveOut = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discretePositionNoiseFixedCurveOut);
        }
    }

    private void DrawDiscretePositionAnimationSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Animation Magnitude";
        attributes.tooltip = "How far the animation will push the camera in the desired direction with a value of 1.0 (in world-units).";
        float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discretePositionAnimationMagnitude);
        screenshakeProfile.discretePositionAnimationMagnitude = Mathf.Max(floatFieldResult, 0f);

        attributes.text = "Animation Direction";
        attributes.tooltip = "Which way the animation should push the camera, is normalized in code. (0, 0) will randomise the direction of the whole animation.";
        screenshakeProfile.discretePositionAnimationDirection = EditorGUILayout.Vector2Field(attributes, screenshakeProfile.discretePositionAnimationDirection);

        EditorGUILayout.Space(10f);

        attributes.text = "Use Custom Curve";
        attributes.tooltip = "Whether to use a custom curve defined by splines (more expensive but more accurate) or a set of equations describing implicit curves.";
        screenshakeProfile.discretePositionAnimationUseCustomCurve = EditorGUILayout.Toggle(attributes, screenshakeProfile.discretePositionAnimationUseCustomCurve);

        EditorGUILayout.Space(10f);

        if (screenshakeProfile.discretePositionAnimationUseCustomCurve)
        {
            attributes.text = "Animation Curve";
            attributes.tooltip = "How the position changes along the desired direction over time.";
            screenshakeProfile.discretePositionAnimationCustomCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discretePositionAnimationCustomCurve);
        }
        else
        {
            attributes.text = "Animation Curve In";
            attributes.tooltip = "How intense the effect of this shake event is over time, up the midpoint, scales using the magnitude value.";
            screenshakeProfile.discretePositionAnimationFixedCurveIn = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discretePositionAnimationFixedCurveIn);

            attributes.text = "Animation Curve Midpoint";
            attributes.tooltip = "Where the midpoint of this shake is.";
            InspectorUtilities.MinMaxLabelledSlider(attributes, ref screenshakeProfile.discretePositionAnimationFixedCurveMidpointMin, ref screenshakeProfile.discretePositionAnimationFixedCurveMidpointMax, 0f, 1f);

            attributes.text = "Animation Curve Out";
            attributes.tooltip = "How intense the effect of this shake event is over time, past the midpoint, scales using the magnitude value.";
            screenshakeProfile.discretePositionAnimationFixedCurveOut = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discretePositionAnimationFixedCurveOut);
        }
    }

    private void DrawContinuousPositionSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Noise Magnitude";
        attributes.tooltip = "How far the camera will be moved by this shake event (in world-units).";
        Vector2 vector2FieldResult = EditorGUILayout.Vector2Field(attributes, screenshakeProfile.continuousPositionNoiseMagnitude);
        vector2FieldResult.x = Mathf.Max(vector2FieldResult.x, 0f);
        vector2FieldResult.y = Mathf.Max(vector2FieldResult.y, 0f);
        screenshakeProfile.continuousPositionNoiseMagnitude = vector2FieldResult;

        attributes.text = "Noise Frequency";
        attributes.tooltip = "How fast the camera will be moved by this shake event.";
        vector2FieldResult = EditorGUILayout.Vector2Field(attributes, screenshakeProfile.continuousPositionNoiseFrequency);
        vector2FieldResult.x = Mathf.Max(vector2FieldResult.x, 0.01f);
        vector2FieldResult.y = Mathf.Max(vector2FieldResult.y, 0.01f);
        screenshakeProfile.continuousPositionNoiseFrequency = vector2FieldResult;
    }

    private bool DrawRotationSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        EditorStyles.label.fontStyle = FontStyle.Bold;

        attributes.text = "Use Rotation";
        attributes.tooltip = "Whether this shake event should manipulate the rotation of the scene camera.";
        screenshakeProfile.useRotation = EditorGUILayout.Toggle(attributes, screenshakeProfile.useRotation);

        EditorStyles.label.fontStyle = FontStyle.Normal;

        if (screenshakeProfile.useRotation)
        {
            EditorGUILayout.Space(10f);

            if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Discrete)
            {
                DrawDiscreteRotationSection(ref screenshakeProfile, ref attributes);
            }
            else if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Continuous)
            {
                DrawContinuousRotationSection(ref screenshakeProfile, ref attributes);
            }
        }

        return screenshakeProfile.useRotation;
    }

    private void DrawDiscreteRotationSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Noise Magnitude";
        attributes.tooltip = "How much the camera will be rotated by this shake event (in degrees).";
        float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteRotationNoiseMagnitude);
        screenshakeProfile.discreteRotationNoiseMagnitude = Mathf.Max(floatFieldResult, 0f);

        attributes.text = "Noise Frequency";
        attributes.tooltip = "How much the camera will be rotated by this shake event (in degrees).";
        floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteRotationNoiseFrequency);
        screenshakeProfile.discreteRotationNoiseFrequency = Mathf.Max(floatFieldResult, 0f);

        EditorGUILayout.Space(10f);

        attributes.text = "Use Custom Curve";
        attributes.tooltip = "Whether to use a custom curve defined by splines (more expensive but more accurate) or a set of equations describing implicit curves.";
        screenshakeProfile.discreteRotationNoiseUseCustomCurve = EditorGUILayout.Toggle(attributes, screenshakeProfile.discreteRotationNoiseUseCustomCurve);

        EditorGUILayout.Space(10f);

        if (screenshakeProfile.discreteRotationNoiseUseCustomCurve)
        {
            attributes.text = "Noise Curve";
            attributes.tooltip = "How intense the effect of this shake event is over time, scales using the magnitude value.";
            screenshakeProfile.discreteRotationNoiseCustomCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discreteRotationNoiseCustomCurve);
        }
        else
        {
            attributes.text = "Noise Curve In";
            attributes.tooltip = "How intense the effect of this shake event is over time, up the midpoint, scales using the magnitude value.";
            screenshakeProfile.discreteRotationNoiseFixedCurveIn = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteRotationNoiseFixedCurveIn);

            attributes.text = "Noise Curve Midpoint";
            attributes.tooltip = "Where the midpoint of this shake is.";
            InspectorUtilities.MinMaxLabelledSlider(attributes, ref screenshakeProfile.discreteRotationNoiseFixedCurveMidpointMin, ref screenshakeProfile.discreteRotationNoiseFixedCurveMidpointMax, 0f, 1f);

            attributes.text = "Noise Curve Out";
            attributes.tooltip = "How intense the effect of this shake event is over time, past the midpoint, scales using the magnitude value.";
            screenshakeProfile.discreteRotationNoiseFixedCurveOut = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteRotationNoiseFixedCurveOut);
        }
    }

    private void DrawContinuousRotationSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Noise Magnitude";
        attributes.tooltip = "How much the camera will be rotated by this shake event (in degrees).";
        float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.continuousRotationNoiseMagnitude);
        screenshakeProfile.continuousRotationNoiseMagnitude = Mathf.Max(floatFieldResult, 0f);

        attributes.text = "Noise Frequency";
        attributes.tooltip = "How much the camera will be rotated by this shake event (in degrees).";
        floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.continuousRotationNoiseFrequency);
        screenshakeProfile.continuousRotationNoiseFrequency = Mathf.Max(floatFieldResult, 0f);
    }

    private bool DrawZoomSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        EditorStyles.label.fontStyle = FontStyle.Bold;

        attributes.text = "Use Zoom";
        attributes.tooltip = "Whether this shake event should manipulate the zoom of the scene camera.";
        screenshakeProfile.useZoom = EditorGUILayout.Toggle(attributes, screenshakeProfile.useZoom);

        EditorStyles.label.fontStyle = FontStyle.Normal;

        if (screenshakeProfile.useZoom)
        {
            EditorGUILayout.Space(10f);

            if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Discrete)
            {
                DrawDiscreteZoomSection(ref screenshakeProfile, ref attributes);
            }
            else if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Continuous)
            {
                DrawContinuousZoomSection(ref screenshakeProfile, ref attributes);
            }
        }

        return screenshakeProfile.useZoom;
    }

    private void DrawDiscreteZoomSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Discrete Style";
        attributes.tooltip = "Whether the zoom is manipulated using noise, or with an animation.";
        screenshakeProfile.discreteZoomStyle = (ScreenshakeSystem.DiscreteType)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteZoomStyle);

        if (screenshakeProfile.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Noise)
        {
            DrawDiscreteZoomNoiseSection(ref screenshakeProfile, ref attributes);
        }
        else if (screenshakeProfile.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Animation)
        {
            DrawDiscreteZoomAnimationSection(ref screenshakeProfile, ref attributes);
        }
    }

    private void DrawDiscreteZoomNoiseSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Noise In Magnitude";
        attributes.tooltip = "How much the camera will zoom in from this shake event (in orthographic size).";
        float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteZoomNoiseInMagnitude);
        screenshakeProfile.discreteZoomNoiseInMagnitude = Mathf.Max(floatFieldResult, 0f);

        attributes.text = "Noise Out Magnitude";
        attributes.tooltip = "How much the camera will zoom out from this shake event (in orthographic size).";
        floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteZoomNoiseOutMagnitude);
        screenshakeProfile.discreteZoomNoiseOutMagnitude = Mathf.Max(floatFieldResult, 0f);

        attributes.text = "Noise Frequency";
        attributes.tooltip = "How fast the camera zoom will be changed by this shake event.";
        floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteZoomNoiseFrequency);
        screenshakeProfile.discreteZoomNoiseFrequency = Mathf.Max(floatFieldResult, 0f);

        EditorGUILayout.Space(10f);

        attributes.text = "Use Custom Curve";
        attributes.tooltip = "Whether to use a custom curve defined by splines (more expensive but more accurate) or a set of equations describing implicit curves.";
        screenshakeProfile.discreteZoomNoiseUseCustomCurve = EditorGUILayout.Toggle(attributes, screenshakeProfile.discreteZoomNoiseUseCustomCurve);

        EditorGUILayout.Space(10f);

        if (screenshakeProfile.discreteZoomNoiseUseCustomCurve)
        {
            attributes.text = "Noise Curve";
            attributes.tooltip = "How intense the effect of this shake event is over time, scales using the magnitude value.";
            screenshakeProfile.discreteZoomNoiseCustomCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discreteZoomNoiseCustomCurve);
        }
        else
        {
            attributes.text = "Noise Curve In";
            attributes.tooltip = "How intense the effect of this shake event is over time, up the midpoint, scales using the magnitude value.";
            screenshakeProfile.discreteZoomNoiseFixedCurveIn = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteZoomNoiseFixedCurveIn);

            attributes.text = "Noise Curve Midpoint";
            attributes.tooltip = "Where the midpoint of this shake is.";
            InspectorUtilities.MinMaxLabelledSlider(attributes, ref screenshakeProfile.discreteZoomNoiseFixedCurveMidpointMin, ref screenshakeProfile.discreteZoomNoiseFixedCurveMidpointMax, 0f, 1f);

            attributes.text = "Noise Curve Out";
            attributes.tooltip = "How intense the effect of this shake event is over time, past the midpoint, scales using the magnitude value.";
            screenshakeProfile.discreteZoomNoiseFixedCurveOut = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteZoomNoiseFixedCurveOut);
        }
    }

    private void DrawDiscreteZoomAnimationSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Animation Magnitude";
        attributes.tooltip = "How much the animation will adjust the camera zoom with a value of 1.0 (in orthographic size).";
        float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteZoomAnimationMagnitude);
        screenshakeProfile.discreteZoomAnimationMagnitude = Mathf.Max(floatFieldResult, 0f);

        attributes.text = "Animation Direction";
        attributes.tooltip = "Which way the camera will zoom with increasing values of this animation.";
        screenshakeProfile.discreteZoomAnimationDirection = (ScreenshakeSystem.ZoomDirection)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteZoomAnimationDirection);

        EditorGUILayout.Space(10f);

        attributes.text = "Use Custom Curve";
        attributes.tooltip = "Whether to use a custom curve defined by splines (more expensive but more accurate) or a set of equations describing implicit curves.";
        screenshakeProfile.discreteZoomAnimationUseCustomCurve = EditorGUILayout.Toggle(attributes, screenshakeProfile.discreteZoomAnimationUseCustomCurve);

        EditorGUILayout.Space(10f);

        if (screenshakeProfile.discreteZoomAnimationUseCustomCurve)
        {
            attributes.text = "Animation Curve";
            attributes.tooltip = "How the camera zoom changes over time.";
            screenshakeProfile.discreteZoomAnimationCustomCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discreteZoomAnimationCustomCurve);
        }
        else
        {
            attributes.text = "Animation Curve In";
            attributes.tooltip = "How intense the effect of this shake event is over time, up the midpoint, scales using the magnitude value.";
            screenshakeProfile.discreteZoomAnimationFixedCurveIn = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteZoomAnimationFixedCurveIn);

            attributes.text = "Animation Curve Midpoint";
            attributes.tooltip = "Where the midpoint of this shake is.";
            InspectorUtilities.MinMaxLabelledSlider(attributes, ref screenshakeProfile.discreteZoomAnimationFixedCurveMidpointMin, ref screenshakeProfile.discreteZoomAnimationFixedCurveMidpointMax, 0f, 1f);

            attributes.text = "Animation Curve Out";
            attributes.tooltip = "How intense the effect of this shake event is over time, past the midpoint, scales using the magnitude value.";
            screenshakeProfile.discreteZoomAnimationFixedCurveOut = (Curves.CurveStyle)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteZoomAnimationFixedCurveOut);
        }
    }

    private void DrawContinuousZoomSection(ref ScreenshakeProfile screenshakeProfile, ref GUIContent attributes)
    {
        attributes.text = "Noise In Magnitude";
        attributes.tooltip = "How much the camera will zoom in from this shake event (in orthographic size).";
        float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.continuousZoomNoiseInMagnitude);
        screenshakeProfile.continuousZoomNoiseInMagnitude = Mathf.Max(floatFieldResult, 0f);

        attributes.text = "Noise Out Magnitude";
        attributes.tooltip = "How much the camera will zoom out from this shake event (in orthographic size).";
        floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.continuousZoomNoiseOutMagnitude);
        screenshakeProfile.continuousZoomNoiseOutMagnitude = Mathf.Max(floatFieldResult, 0f);

        attributes.text = "Noise Frequency";
        attributes.tooltip = "How fast the camera zoom will be changed by this shake event.";
        floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.continuousZoomNoiseFrequency);
        screenshakeProfile.continuousZoomNoiseFrequency = Mathf.Max(floatFieldResult, 0f);
    }
}
