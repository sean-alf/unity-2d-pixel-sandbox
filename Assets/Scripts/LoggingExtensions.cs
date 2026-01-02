using System;
using System.Collections.Generic;
using UnityEngine;

public static class Logging
{
    public enum Level
    {
        VERBOSE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        NONE,
    }

    public readonly struct Tag
    {
        public string Value { get; }

        public Tag(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString() => Value;
    };

    public static readonly Dictionary<Tag, Level> logLevels = new();

    public static Tag CreateLogTag(this Component c) => new($"{c.GetType().Name} ({c.name})");

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
        if (IsValidLevel(tag, Level.VERBOSE))
        {
            Debug.unityLogger.Log($"[V] {tag}", message);
        }
    }

    public static void LogDebug(Tag tag, object message)
    {
        if (IsValidLevel(tag, Level.DEBUG))
        {
            Debug.unityLogger.Log(Blue($"[D] {tag}"), Blue(message));
        }
    }

    public static void LogInfo(Tag tag, object message)
    {
        if (IsValidLevel(tag, Level.INFO))
        {
            Debug.unityLogger.Log(Green($"[I] {tag}"), Green(message));
        }
    }

    public static void LogWarning(Tag tag, object message)
    {
        if (IsValidLevel(tag, Level.WARN))
        {
            Debug.unityLogger.LogWarning($"[W] {tag}", message);
        }
    }

    public static void LogError(Tag tag, object message)
    {
        if (IsValidLevel(tag, Level.ERROR))
        {
            Debug.unityLogger.LogError($"[E] {tag}", message);
        }
    }

    private static bool IsValidLevel(Tag t, Level l)
    {
        return !logLevels.ContainsKey(t) || logLevels[t] <= l;
    }

    private static string Green(object text) => $"<color=green>{text}</color>";
    private static string Blue(object text) => $"<color=cyan>{text}</color>";
}
