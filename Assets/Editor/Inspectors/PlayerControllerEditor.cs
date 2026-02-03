using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerController))]
public class PlayerMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var controller = (PlayerController)target;

        DrawDefaultInspector();

        if (controller.IsInputReady)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);

            BetterInputManager.InputType inputType = (BetterInputManager.InputType)EditorGUILayout.EnumPopup("Input Type", controller.CurrentInputType);

            if (inputType != controller.CurrentInputType) controller.UpdateInputType(inputType);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);

        Rigidbody2D rb = controller.GetComponent<Rigidbody2D>();
        EditorGUILayout.LabelField("Rotation", rb.rotation.ToString("F2") + "°");
        EditorGUILayout.LabelField("Input Active", controller.IsInputActive ? "True" : "False");

        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }
}
