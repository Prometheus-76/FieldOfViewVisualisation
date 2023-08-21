using UnityEngine;
using UnityEditor;

[InitializeOnLoadAttribute]
public static class EditorHapticReset
{
    static EditorHapticReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        EditorApplication.pauseStateChanged += OnPauseStateChange;
    }

    private static void OnPlayModeStateChange(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            InputManager.ResetControllerRumble();
        }
    }

    private static void OnPauseStateChange(PauseState state)
    {
        if (state == PauseState.Paused)
        {
            InputManager.ResetControllerRumble();
        }
    }
}
