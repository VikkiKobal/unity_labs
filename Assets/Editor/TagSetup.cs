using UnityEngine;
using UnityEditor;

/// <summary>
/// Автоматично додає потрібні теги при компіляції проєкту.
/// </summary>
[InitializeOnLoad]
public static class TagSetup
{
    static TagSetup()
    {
        AddTag("Car");
    }

    private static void AddTag(string tag)
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
            {
                Debug.Log($"[TagSetup] Тег '{tag}' вже існує.");
                return;
            }
        }

        int idx = tagsProp.arraySize;
        tagsProp.InsertArrayElementAtIndex(idx);
        tagsProp.GetArrayElementAtIndex(idx).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[TagSetup] Тег '{tag}' успішно додано!");
    }
}
