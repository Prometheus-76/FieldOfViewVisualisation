using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScreenshakeProfile))]
public class ScreenshakeProfile_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        // The instance we are modifying
        ScreenshakeProfile screenshakeProfile = target as ScreenshakeProfile;
        GUIContent attributes = new GUIContent();

        // Basic configuration, should always be shown
        InspectorUtilities.AddHeader("Basic Configuration");
        attributes.text = "Shake Type";
        attributes.tooltip = "<b>Continuous</b> - Controlled by another script in real-time using a handle object.\n\n<b>Discrete</b> - Controlled internally within the ScreenshakeSystem over a fixed duration.";
        screenshakeProfile.shakeType = (ScreenshakeSystem.ShakeType)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.shakeType);

        if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Discrete)
        {
            attributes.text = "Shake Duration";
            attributes.tooltip = "How long this discrete shake event will last for.";
            float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.shakeDuration);
            screenshakeProfile.shakeDuration = Mathf.Max(floatFieldResult, 0.01f);
        }

        EditorGUILayout.Space(10f);

        bool positionDrawn = DrawPositionSection(ref screenshakeProfile, ref attributes);
        if (positionDrawn) EditorGUILayout.Space(10f);

        bool rotationDrawn = DrawRotationSection(ref screenshakeProfile, ref attributes);
        if (rotationDrawn) EditorGUILayout.Space(10f);
        
        bool zoomDrawn = DrawZoomSection(ref screenshakeProfile, ref attributes);
        if (zoomDrawn) EditorGUILayout.Space(10f);
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
            if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Continuous)
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
            else if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Discrete)
            {
                attributes.text = "Discrete Style";
                attributes.tooltip = "Whether the position is manipulated using noise, or with a directional animation.";
                screenshakeProfile.discretePositionStyle = (ScreenshakeSystem.DiscreteType)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discretePositionStyle);

                if (screenshakeProfile.discretePositionStyle == ScreenshakeSystem.DiscreteType.Noise)
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

                    attributes.text = "Noise Curve";
                    attributes.tooltip = "How intense the effect of this shake event is over time, scales using the magnitude value.";
                    screenshakeProfile.discretePositionNoiseCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discretePositionNoiseCurve);
                }
                else if (screenshakeProfile.discretePositionStyle == ScreenshakeSystem.DiscreteType.Animation)
                {
                    attributes.text = "Animation Magnitude";
                    attributes.tooltip = "How far the animation will push the camera in the desired direction with a value of 1.0 (in world-units).";
                    float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discretePositionAnimationMagnitude);
                    screenshakeProfile.discretePositionAnimationMagnitude = Mathf.Max(floatFieldResult, 0f);

                    attributes.text = "Animation Direction";
                    attributes.tooltip = "Which way the animation should push the camera, is normalized in code. (0, 0) will randomise the direction of the whole animation.";
                    screenshakeProfile.discretePositionAnimationDirection = EditorGUILayout.Vector2Field(attributes, screenshakeProfile.discretePositionAnimationDirection);

                    attributes.text = "Animation Curve";
                    attributes.tooltip = "How the position changes along the desired direction over time.";
                    screenshakeProfile.discretePositionAnimationCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discretePositionAnimationCurve);
                }
            }
        }

        return screenshakeProfile.usePosition;
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
            if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Continuous)
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
            else if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Discrete)
            {
                attributes.text = "Noise Magnitude";
                attributes.tooltip = "How much the camera will be rotated by this shake event (in degrees).";
                float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteRotationNoiseMagnitude);
                screenshakeProfile.discreteRotationNoiseMagnitude = Mathf.Max(floatFieldResult, 0f);

                attributes.text = "Noise Frequency";
                attributes.tooltip = "How much the camera will be rotated by this shake event (in degrees).";
                floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteRotationNoiseFrequency);
                screenshakeProfile.discreteRotationNoiseFrequency = Mathf.Max(floatFieldResult, 0f);

                attributes.text = "Noise Curve";
                attributes.tooltip = "How intense the effect of this shake event is over time, scales using the magnitude value.";
                screenshakeProfile.discreteRotationNoiseCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discreteRotationNoiseCurve);
            }
        }

        return screenshakeProfile.useRotation;
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
            if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Continuous)
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
            else if (screenshakeProfile.shakeType == ScreenshakeSystem.ShakeType.Discrete)
            {
                attributes.text = "Discrete Style";
                attributes.tooltip = "Whether the zoom is manipulated using noise, or with an animation.";
                screenshakeProfile.discreteZoomStyle = (ScreenshakeSystem.DiscreteType)EditorGUILayout.EnumPopup(attributes, screenshakeProfile.discreteZoomStyle);

                if (screenshakeProfile.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Noise)
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

                    attributes.text = "Noise Curve";
                    attributes.tooltip = "How intense the effect of this shake event is over time, scales using the magnitude value.";
                    screenshakeProfile.discreteZoomNoiseCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discreteZoomNoiseCurve);
                }
                else if (screenshakeProfile.discreteZoomStyle == ScreenshakeSystem.DiscreteType.Animation)
                {
                    attributes.text = "Animation Magnitude";
                    attributes.tooltip = "How much the animation will adjust the camera zoom with a value of 1.0 (in orthographic size).";
                    float floatFieldResult = EditorGUILayout.FloatField(attributes, screenshakeProfile.discreteZoomAnimationMagnitude);
                    screenshakeProfile.discreteZoomAnimationMagnitude = Mathf.Max(floatFieldResult, 0f);

                    attributes.text = "Noise Curve";
                    attributes.tooltip = "How the camera zoom changes over time.";
                    screenshakeProfile.discreteZoomAnimationCurve = EditorGUILayout.CurveField(attributes, screenshakeProfile.discreteZoomAnimationCurve);
                }
            }
        }

        return screenshakeProfile.useZoom;
    }
}
