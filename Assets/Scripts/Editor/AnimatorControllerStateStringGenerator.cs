#if UNITY_EDITOR

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;

public static class AnimatorControllerStateStringGenerator
{
    [MenuItem("Assets/Generate State Strings")]
    [MenuItem("Tools/Animator Controller/Generate State Strings")]
    public static void Generate()
    {
        var controller = Selection.activeObject as AnimatorController;

        if (controller == null) return;

        string className = $"{controller.name.Replace(" ", "")}AnimatorStates";
        string code = $"public static class {className}\n{{\n";

        foreach (var layer in controller.layers)
        {
            code += $"\tpublic static class {layer.name.Replace(" ", "")}\n\t{{\n";

            foreach (var state in layer.stateMachine.states)
            {
                string name = state.state.name;
                string upperSnakeCaseName = Regex.Replace(name, @"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", "_").ToUpper();
                code += $"\t\tpublic static string {upperSnakeCaseName} = \"{name}\";\n";
            }

            code += "\t}\n";
        }

        code += "}\n";

        string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));

        if (directory == null || directory.Length == 0)
        {
            directory = "Assets";
        }

        string path = $"{directory}/{className}.cs";

        File.WriteAllText(path, code);
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Generate State Strings", true)]
    [MenuItem("Tools/Animator Controller/Generate State Strings", true)]
    static bool ValidateGenerate()
    {
        return Selection.activeObject is AnimatorController;
    }
}

#endif
