using UnityEngine;

public class MenusAndDisplayManager : MonoBehaviour, ILoggerProvider
{
    [SerializeField]
    private TopLeftContainer topLeftContainer;

    [Header("Debug")]
    [Space]

    [SerializeField]
    private Logger logger;

    public Logger Logger => logger;

    private void OnEnable()
    {
        logger.CreateTag(this);
    }

    public TopLeftContainer GetTopLeftContainer() => topLeftContainer;
}
