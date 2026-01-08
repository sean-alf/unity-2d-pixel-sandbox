#if UNITY_EDITOR

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.InputSystem;

public static class InputActionMapStringsGenerator
{

    [MenuItem("Assets/Input Action Asset/Generate Input Action Map Strings")]
    [MenuItem("Tools/Input Action Asset/Generate Input Action Map Strings")]
    public static void Generate()
    {
        var asset = Selection.activeObject as InputActionAsset;

        if (asset == null) return;

        string className = $"{asset.name.Replace(" ", "")}_Names";
        string code = $"using UnityEngine.InputSystem;\n\n";
        code += $"public static class {className}\n{{\n";

        foreach (var map in asset.actionMaps)
        {
            code += $"\tpublic static class {map.name}\n\t{{\n";

            code += $"\t\tpublic static readonly string MAP_NAME = \"{map.name}\";\n\n";

            foreach (var action in map.actions)
            {
                var actionName = action.name.Replace(" ", "_");
                var actionNameConstant = Regex.Replace(actionName, "([a-z])([A-Z])", "$1_$2").ToUpper();

                code += $"\t\tpublic static readonly string {actionNameConstant} = \"{action.name}\";\n\n";
                code += $"\t\tpublic static InputAction {actionName}(PlayerInput input)\n\t\t{{\n";
                code += $"\t\t\treturn input.actions.FindActionMap(MAP_NAME).FindAction({actionNameConstant});\n";
                code += "\t\t}\n\n";
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

    [MenuItem("Assets/Input Action Asset/Generate Input Action Map Strings", true)]
    [MenuItem("Tools/Input Action Asset/Generate Input Action Map Strings", true)]
    static bool ValidateGenerate()
    {
        return Selection.activeObject is InputActionAsset;
    }
}

#endif
