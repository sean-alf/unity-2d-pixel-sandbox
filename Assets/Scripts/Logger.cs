using System;
using UnityEngine;

public interface ILogTagProvider
{
    public Logging.Tag LogTag
    {
        get;
    }
}

[Serializable]
public class Logger
{
    public Logging.Level logLevel;

    public Logging.Tag logTag;

    public void CreateTag(Component c)
    {
        logTag = c.CreateLogTag();
        Debug.Log($"Logger: tag created {logTag}");
    }

    public void SetLogLevel()
    {
        Logging.SetLogLevel(logTag, logLevel);
    }

    public void V(object message) => Logging.LogVerbose(logTag, message);

    public void D(object message) => Logging.LogDebug(logTag, message);

    public void I(object message) => Logging.LogInfo(logTag, message);

    public void W(object message) => Logging.LogWarning(logTag, message);

    public void E(object message) => Logging.LogError(logTag, message);
}
