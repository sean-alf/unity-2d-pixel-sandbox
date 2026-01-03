using UnityEngine;

public class MenusAndDisplayManager : MonoBehaviour, ILogTagProvider
{
    [SerializeField]
    private TopLeftContainer topLeftContainer;

    [Header("Debug")]
    [Space]

    [SerializeField]
    private LogLevelSelector logLevelSelector;

    private Logging.Tag logTag;

    public Logging.Tag LogTag => logTag;

    private void OnEnable()
    {
        logTag = this.CreateLogTag();
        Logging.SetLogLevel(logTag, logLevelSelector.logLevel);
    }

    public TopLeftContainer GetTopLeftContainer() => topLeftContainer;
}
