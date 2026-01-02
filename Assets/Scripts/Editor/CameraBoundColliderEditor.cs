using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraBoundCollider))]
public class CameraBoundColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Reset"))
        {
            ((CameraBoundCollider)target).Reset();
        }
    }
}
