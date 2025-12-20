#if UNITY_EDITOR

using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public static class LayerCodeGenerator
{
    public static readonly string DIALOG_TITLE = "Generate LayerNames Class";
    public static readonly string CONFIRM_TEXT = "Yes";
    public static readonly string DECLINE_TEXT = "No";

    [MenuItem("Assets/Generate LayerNames Class")]
    public static void GenerateLayerNameClass()
    {
        string code = "public static class LayerNames {\n";

        foreach (string l in InternalEditorUtility.layers)
        {
            code += $"   public static readonly string {l.Replace(" ", "")} = \"{l}\";\n";
        }

        code += "}\n";

        string directory = GetProjectWindowPath() ?? "Assets";
        string path = $"{directory}/LayerNames.cs";

        bool result = EditorUtility.DisplayDialog(
            DIALOG_TITLE,
            $"Would you like to generate the file {path}?",
            CONFIRM_TEXT,
            DECLINE_TEXT
        );

        if (!result)
        {
            return;
        }

        if (File.Exists(path))
        {
            result = EditorUtility.DisplayDialog(
                DIALOG_TITLE,
                $"The file '{path}' already exists. Would you like to overwrite it?",
                CONFIRM_TEXT,
                DECLINE_TEXT
            );

            if (!result)
            {
                return;
            }
        }

        File.WriteAllText(path, code);
        AssetDatabase.Refresh();
    }

    static string GetProjectWindowPath()
    {
        var method = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
        return (string)method.Invoke(null, null);
    }
}

#endif
