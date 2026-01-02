using UnityEditor;
using UnityEngine;
using Level = Logging.Level;

[CustomPropertyDrawer(typeof(LogLevelSelector))]
public class LogLevelSelectorPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty logLevel = property.FindPropertyRelative("logLevel");
        Level currentLevel = (Level)logLevel.enumValueIndex;

        EditorGUI.BeginProperty(position, label, property);

        Level newLevel = (Level)EditorGUI.EnumPopup(position, new GUIContent("Log Level"), currentLevel);

        if (newLevel != currentLevel)
        {
            logLevel.enumValueIndex = (int)newLevel;

            if (property.serializedObject.targetObject is ILogTagProvider p)
            {
                Logging.SetLogLevel(p.LogTag, newLevel);
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError($"LogLeveleSelectorPropertyDrawer: component ({property.serializedObject.targetObject.GetType().Name}) does not implement ILogTagProvider!!");
            }
        }
    }
}
