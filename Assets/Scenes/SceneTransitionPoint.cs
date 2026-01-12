using System;
using UnityEngine;

public enum TransitionType
{
    ENTRY,
    EXIT
}

public static class SceneTransitionPointExtensions
{
    public static bool ShouldEnter(this TransitionType t, TransitionType from) => t != from;
}

public interface ISceneTransitionPoint
{
    public TransitionType TransitionType { get; }
    public Action<TransitionType> OnExit { get; set; }

    public void Enter();
}

public class SceneTransitionPoint : MonoBehaviour, ILoggerProvider
{
    [SerializeField]
    private Logger logger;

    private ISceneTransitionPoint transitionPoint;

    public TransitionType Type => transitionPoint.TransitionType;

    public Action<TransitionType> OnExit
    {
        get => transitionPoint.OnExit;
        set => transitionPoint.OnExit = value;
    }

    public Logger Logger => logger;

    void Awake()
    {
        if (!TryGetComponent(out transitionPoint))
        {
            logger.E($"no {nameof(ISceneTransitionPoint)} attached!!");
        }
    }

    public void Enter()
    {
        transitionPoint.Enter();
    }
}
