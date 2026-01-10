using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour, ILoggerProvider
{
    [SerializeField]
    [Tooltip("This is exposed only for debuging purposes. Do not update in the inspector!")]
    private List<SceneTransitionPoint> transitionPoints;

    [Space]
    [Header("Debug")]

    [SerializeField]
    private Logger logger;

    private static readonly int PERSISTENT_SCENE_INDEX = 0;
    private static readonly int STARTING_SCENE_INDEX = PERSISTENT_SCENE_INDEX + 1;

    private TransitionType cause = TransitionType.EXIT;
    // The index of the scene that starts the levels, and isn't persistent
    private int sceneIndex = STARTING_SCENE_INDEX;
    private int prevSceneIndex = STARTING_SCENE_INDEX;

    public Logger Logger => logger;

    void Start()
    {
        if (SceneManager.GetSceneByBuildIndex(sceneIndex).isLoaded)
        {
            // If the initial scene is already loaded (during testing of the scene)
            // simply find the entry point and enter
            FindTransitionPointsThenEnter();
        }
        else
        {
            // ... otherwise load the scene first
            LoadUpcomingScene();
        }
    }

    private void Switch()
    {
        prevSceneIndex = sceneIndex;

        if (cause == TransitionType.EXIT)
        {
            if (++sceneIndex == SceneManager.sceneCountInBuildSettings)
            {
                sceneIndex = SceneManager.sceneCountInBuildSettings - 1;
                logger.I($"No more scenes! Final scene index {sceneIndex}.");
                return;
            }
        }
        else
        {
            if (--sceneIndex < STARTING_SCENE_INDEX) sceneIndex = STARTING_SCENE_INDEX;
        }

        foreach (var e in transitionPoints)
        {
            e.OnExit -= OnExit;
        }

        LoadUpcomingScene();
    }

    private void OnExit(TransitionType type)
    {
        cause = type;
        Switch();
    }

    private void LoadUpcomingScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnloadExpiredScene();
    }

    private void UnloadExpiredScene()
    {
        if (sceneIndex != prevSceneIndex)
        {
            var operation = SceneManager.UnloadSceneAsync(prevSceneIndex);
            operation.completed += OnUnloadComplete;
        }
    }

    private void OnUnloadComplete(AsyncOperation operation)
    {
        operation.completed -= OnUnloadComplete;
        FindTransitionPointsThenEnter();
    }

    private void FindTransitionPointsThenEnter()
    {
        GetAllTransitionPoints();

        foreach (var e in transitionPoints)
        {
            if ((cause == TransitionType.EXIT && e.Type == TransitionType.ENTRY) || (cause == TransitionType.ENTRY && e.Type == TransitionType.EXIT))
            {
                e.Enter();
            }
        }
    }

    private void GetAllTransitionPoints()
    {
        transitionPoints.Clear();
        transitionPoints.AddRange(FindObjectsByType<SceneTransitionPoint>(FindObjectsSortMode.None));

        foreach (var e in transitionPoints)
        {
            e.OnExit += OnExit;
        }
    }
}
