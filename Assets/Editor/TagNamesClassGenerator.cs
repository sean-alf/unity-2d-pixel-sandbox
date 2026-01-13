#if UNITY_EDITOR

using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;

public static class TagNamesClassGenerator
{
    public static readonly string DIALOG_TITLE = "Generate TagNames Class";
    public static readonly string CONFIRM_TEXT = "Yes";
    public static readonly string DECLINE_TEXT = "No";

    [MenuItem("Assets/Generate TagNames Class")]
    public static void GenerateTagNamesClass()
    {
        string code = "public static class TagNames {\n";

        foreach (string t in InternalEditorUtility.tags)
        {
            code += $"   public static readonly string {t.Replace(" ", "")} = \"{t}\";\n";
        }

        code += "}\n";

        string directory = GetProjectWindowPath() ?? "Assets";
        string path = $"{directory}/TagNames.cs";

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
