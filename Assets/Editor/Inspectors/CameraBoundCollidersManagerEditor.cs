using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraBoundCollidersManager))]
public class CameraBoundCollidersManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Reset"))
        {
            ((CameraBoundCollidersManager)target).ResetBounds();
        }
    }
}