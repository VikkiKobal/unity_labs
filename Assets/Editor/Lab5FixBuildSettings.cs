using UnityEditor;
using UnityEngine;

public class Lab5FixBuildSettings
{
    [MenuItem("Tools/Lab 5/Fix Build Settings (lab_2 only)")]
    static void Fix()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/lab_2.unity", true)
        };
        Debug.Log("[Lab5] Build Settings: only lab_2.unity remains.");
    }
}
