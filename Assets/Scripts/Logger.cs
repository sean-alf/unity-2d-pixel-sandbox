using System;
using UnityEngine;

[Serializable]
public class Logger
{
    public Logging.Level logLevel = Logging.Level.DEFAULT;

    public Logging.Tag logTag;

    /// <summary>
    /// Creates and internally stores the Logging.Tag for this Component.
    /// </summary>
    /// <param name="c"></param>
    /// <returns>The newly created Logging.Tag</returns>
    public Logging.Tag CreateTag(Component c)
    {
        logTag = c.CreateLogTag();
        return logTag;
    }

    /// <summary>
    /// Creates and internally stores the Logging.Tag for this ScriptableObject.
    /// </summary>
    /// <param name="s"></param>
    /// <returns>The newly created Logging.Tag</returns>
    public Logging.Tag CreateTag(ScriptableObject s)
    {
        logTag = s.CreateLogTag();
        return logTag;
    }

    /// <summary>
    /// Sets log level if internally stored tag has been created.
    /// </summary>
    public void SetLogLevel()
    {
        if (logTag.Value == null || logTag.Value.Length == 0) return;
        Logging.SetLogLevel(logTag, logLevel);
    }

    public void V(object message) => Logging.LogVerbose(logTag, message);

    public void D(object message) => Logging.LogDebug(logTag, message);

    public void I(object message) => Logging.LogInfo(logTag, message);

    public void W(object message) => Logging.LogWarning(logTag, message);

    public void E(object message) => Logging.LogError(logTag, message);
}
