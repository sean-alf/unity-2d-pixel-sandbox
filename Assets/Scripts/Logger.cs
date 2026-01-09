using System;
using UnityEngine;

[Serializable]
public class Logger
{
    public Logging.Level logLevel = Logging.DEFAULT_LEVEL;

    public Logging.Tag logTag;

    /// <summary>
    /// Creates and internally stores the Logging.Tag for this Object.
    /// </summary>
    /// <param name="o"></param>
    /// <returns>The newly created Logging.Tag</returns>
    public Logging.Tag CreateTag(UnityEngine.Object o)
    {
        logTag = new(o);
        return logTag;
    }

    /// <summary>
    /// Sets log level if internally stored tag has been created.
    /// </summary>
    public void SetLogLevel()
    {
        if (logTag == null || logTag.Value == null || logTag.Value.Length == 0) return;
        Logging.SetLogLevel(logTag, logLevel);
    }

    public void V(object message) => Logging.LogVerbose(logTag, message);

    public void D(object message) => Logging.LogDebug(logTag, message);

    public void I(object message) => Logging.LogInfo(logTag, message);

    public void W(object message) => Logging.LogWarning(logTag, message);

    public void E(object message) => Logging.LogError(logTag, message);
}
