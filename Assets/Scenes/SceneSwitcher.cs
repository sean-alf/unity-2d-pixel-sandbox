using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using IEnumerator = System.Collections.IEnumerator;

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
    private static readonly int FIRST_LEVEL_SCENE_INDEX = PERSISTENT_SCENE_INDEX + 1;

    private TransitionType cause = TransitionType.EXIT;
    // The index of the scene that starts the levels, and isn't persistent
    private int sceneIndex = FIRST_LEVEL_SCENE_INDEX;
    private int prevSceneIndex = FIRST_LEVEL_SCENE_INDEX;
    private bool firstLoad = true;

    public Logger Logger => logger;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void Start()
    {
        StartCoroutine(UnloadUnwantedLoadedScenes(() =>
        {
            // On Done
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
        }));
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
            if (--sceneIndex < FIRST_LEVEL_SCENE_INDEX) sceneIndex = FIRST_LEVEL_SCENE_INDEX;
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
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"loaded scene {scene.name}");

        if (scene.buildIndex == FIRST_LEVEL_SCENE_INDEX && firstLoad)
        {
            firstLoad = false;
            FindTransitionPointsThenEnter();
        }
        else if (sceneIndex != prevSceneIndex)
        {
            SceneManager.UnloadSceneAsync(prevSceneIndex);
        }

        if (scene.buildIndex >= FIRST_LEVEL_SCENE_INDEX)
        {
            SceneManager.SetActiveScene(scene);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
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

    private IEnumerator UnloadUnwantedLoadedScenes(Action onDone)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneAt(i);

            // If it's not the first level (or below, i.e., persistent scene(s)), then unload it
            if (scene.buildIndex > FIRST_LEVEL_SCENE_INDEX)
            {
                Debug.Log($"unloading scene {scene.name}");
                logger.I($"unloading scene {scene.name}");
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        onDone.Invoke();
    }
}
