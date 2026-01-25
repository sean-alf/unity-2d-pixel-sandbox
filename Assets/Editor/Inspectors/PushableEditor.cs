using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pushable))]
public class PushableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var pushable = (Pushable)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Snap To Cell"))
        {
            var pos = pushable.transform.position;
            pushable.transform.position = pos.SnapXYToHalfInteger();
        }
    }
}
