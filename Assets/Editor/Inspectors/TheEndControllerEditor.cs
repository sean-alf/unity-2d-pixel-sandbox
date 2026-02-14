using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TheEndController))]
public class TheEndControllerEditor : Editor
{
    private TheEndController controller;

    private void OnEnable()
    {
        controller = (TheEndController)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Play Entry Animation"))
        {
            controller.Enter();
        }

        if (GUILayout.Button("Hide"))
        {
            controller.Hide();
        }
    }
}
