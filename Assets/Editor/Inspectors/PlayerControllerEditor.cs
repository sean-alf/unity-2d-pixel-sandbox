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

            string buttonText = controller.IsInputActive ? "Disable" : "Enable";
            if (GUILayout.Button($"{buttonText} Input"))
            {
                if (controller.IsInputActive)
                {
                    controller.DisableInput();
                }
                else
                {
                    controller.EnableInput();
                }
            }
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
