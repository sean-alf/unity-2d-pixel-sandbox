#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimatorStateCodeGenerator
{
    [MenuItem("Assets/Generate Animator State Code")]
    static void Generate()
    {
        var controller = Selection.activeObject as AnimatorController;

        if (controller == null)
        {
            Debug.LogError("Invalid selection for generating animator state code");
            Debug.LogError("An Animator Controller must be selected in the Project view");
            return;
        }

        string className = controller.name.Replace(" ", "") + "AnimatorControllerStates";
        string code = $"using UnityEngine;\n\npublic static class {className} {{\n";

        foreach (var layer in controller.layers)
        {
            string layerName = layer.name.Replace(" ", "");

            code += $"   public static class {layerName} {{\n";

            foreach (var state in layer.stateMachine.states)
            {
                string stateName = state.state.name;
                code += $"      public static readonly int {stateName} = Animator.StringToHash(\"{stateName}\");\n";
            }

            code += "   }\n";
        }

        code += "}";

        string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(controller)) ?? "Assets";
        File.WriteAllText($"{path}/{className}.cs", code);
        AssetDatabase.Refresh();
    }
}

#endif