using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player2Movement))]
public class Player2MovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);

        Rigidbody2D rb = ((Player2Movement)target).GetComponent<Rigidbody2D>();
        EditorGUILayout.LabelField("Rotation", rb.rotation.ToString("F2") + "°");

        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }
}
