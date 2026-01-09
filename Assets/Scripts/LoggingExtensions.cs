using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Logging
{
    public static readonly Level DEFAULT_LEVEL = Level.INFO;

    public enum Level
    {
        VERBOSE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        NONE,
    }

    public class Tag
    {
        /// <summary>
        /// The full tag value.
        /// Use this for displaying tags in the console.
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// The object's instance name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The objects type name.
        /// </summary>
        public string TypeName { get; }
        public string GUID { get; }
        public long LocalID { get; }
        /// <summary>
        /// Returns true if an ID was found for the object.
        /// </summary>
        public bool IsIDValid { get; }

        public Tag(UnityEngine.Object o)
        {
            Name = o.name;
            TypeName = o.GetType().Name;
            Value = $"{Name} -> {TypeName}";
            IsIDValid = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out string guid, out long localID);
            GUID = guid;
            LocalID = localID;
        }

        public override string ToString() => Value;

        public bool IsValid() => IsIDValid && Value != null && Value.Length > 0;

        public override bool Equals(object obj)
        {
            if (obj is Tag other)
            {
                return GUID == other.GUID && LocalID == other.LocalID && Value == other.Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GUID, LocalID, Value);
        }
    };

    public static readonly Dictionary<Tag, Level> logLevels = new();

    public static void SetLogLevel(Tag tag, Level level)
    {
        if (!logLevels.ContainsKey(tag))
        {
            logLevels.Add(tag, level);
        }
        else
        {
            logLevels[tag] = level;
        }
    }

    public static void LogVerbose(Tag tag, object message)
    {
        if (IsValid(tag, Level.VERBOSE))
        {
            Debug.unityLogger.Log($"[V] {tag}", message);
        }
    }

    public static void LogDebug(Tag tag, object message)
    {
        if (IsValid(tag, Level.DEBUG))
        {
            Debug.unityLogger.Log(Blue($"[D] {tag}"), Blue(message));
        }
    }

    public static void LogInfo(Tag tag, object message)
    {
        if (IsValid(tag, Level.INFO))
        {
            Debug.unityLogger.Log(Green($"[I] {tag}"), Green(message));
        }
    }

    public static void LogWarning(Tag tag, object message)
    {
        if (IsValid(tag, Level.WARN))
        {
            Debug.unityLogger.LogWarning($"[W] {tag}", message);
        }
    }

    public static void LogError(Tag tag, object message)
    {
        if (IsValid(tag, Level.ERROR))
        {
            Debug.unityLogger.LogError($"[E] {tag}", message);
        }
    }

    /// <summary>
    /// Clears all tags, i.e., it clears the log levels dictionary.
    /// </summary>
    public static void ClearAllTags() => logLevels.Clear();

    private static bool IsValid(Tag t, Level l)
    {
        return t != null && t.Value != null && t.Value.Length > 0 && (!logLevels.ContainsKey(t) || logLevels[t] <= l);
    }

    private static string Green(object text) => $"<color=green>{text}</color>";
    private static string Blue(object text) => $"<color=cyan>{text}</color>";
}
