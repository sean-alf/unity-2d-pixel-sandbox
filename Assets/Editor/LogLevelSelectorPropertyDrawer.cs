using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Logger))]
public class LogLevelSelectorPropertyDrawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var p = property.serializedObject.targetObject as ILoggerProvider;
        var c = property.serializedObject.targetObject as Component;
        SerializedProperty logLevel = property.FindPropertyRelative("logLevel");
        Logging.Level currentLevel = (Logging.Level)logLevel.enumValueIndex;

        EditorGUI.BeginProperty(position, label, property);

        Logging.Level newLevel = (Logging.Level)EditorGUI.EnumPopup(position, new GUIContent("Log Level"), currentLevel);

        if (newLevel != currentLevel)
        {
            logLevel.enumValueIndex = (int)newLevel;
            property.serializedObject.ApplyModifiedProperties();

            if (p != null)
            {
                p.Logger.SetLogLevel();
            }
            else
            {
                Debug.LogError($"LogLeveleSelectorPropertyDrawer: component ({property.serializedObject.targetObject.GetType().Name}) does not implement ILogTagProvider!!");
            }
        }

        if (p != null)
        {
            // Second line: use PrefixLabel for proper alignment
            Rect secondLine = position;
            secondLine.y += EditorGUIUtility.singleLineHeight + 2;
            secondLine.height = EditorGUIUtility.singleLineHeight;

            // Generate control ID and split label/value
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Rect valueRect = EditorGUI.PrefixLabel(secondLine, controlID, new GUIContent("Log Tag"));

            // Draw the value in the correct area
            EditorGUI.LabelField(valueRect, p.Logger?.logTag?.ToString() ?? "Unset");
        }

        EditorGUI.EndProperty();
    }
}
