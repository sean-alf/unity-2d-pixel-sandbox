using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerController))]
public class PlayerMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);

        Rigidbody2D rb = ((PlayerController)target).GetComponent<Rigidbody2D>();
        EditorGUILayout.LabelField("Rotation", rb.rotation.ToString("F2") + "°");

        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }
}
