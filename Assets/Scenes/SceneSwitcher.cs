using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneIndex
{
    Persistent = 0,
    FirstLevel = 1,
}

static class SceneSwitcherExtensions
{
    public static bool IsLoaded(this SceneIndex s) => SceneManager.GetSceneByBuildIndex((int)s).isLoaded;

    public static bool IsPersistentScene(this Scene scene) => scene.buildIndex == (int)SceneIndex.Persistent;

    public static bool IsLevelSceneIndex(this int sceneBuildIndex) => sceneBuildIndex >= (int)SceneIndex.FirstLevel;

    public static bool IsFirstLevelScene(this Scene s) => s.buildIndex == (int)SceneIndex.FirstLevel;

    public static bool IsGreaterThanFirstLevelScene(this int sceneBuildIndex) => sceneBuildIndex > (int)SceneIndex.FirstLevel;
}

[RequireComponent(typeof(Synchronizer))]
public class SceneSwitcher : MonoBehaviour, ILoggerProvider
{
    [SerializeField] private bool startFromCurrentScene = true;
    [SerializeField] private bool teleportInInitially = false;

    [Space]
    [Header("Debug")]

    [Tooltip("This is exposed only for debuging purposes. Do not update in the inspector!")]
    [SerializeField] private List<SceneTransitionPoint> transitionPoints;
    [SerializeField] private int targetBuildIndex = (int)SceneIndex.FirstLevel;
    // The index of the scene that starts the levels, and isn't persistent
    [SerializeField] private int prevSceneIndex = (int)SceneIndex.FirstLevel;
    [SerializeField] private int lastLevelSceneIndex;
    [SerializeField] private bool firstLoad = true;
    [SerializeField] private Logger logger;

    private Synchronizer sync;
    private TransitionType fromTransitionType = TransitionType.EXIT;


    public Logger Logger => logger;

    private void Awake()
    {
        sync = GetComponent<Synchronizer>();

        if (startFromCurrentScene && !SceneManager.GetActiveScene().buildIndex.IsLevelSceneIndex())
        {
            // This could cause all level scenes to be unloaded
            Debug.LogError($"{name} ({GetType().Name}): startFromCurrentScene is enabled, but active scene is not a level scene!");
            startFromCurrentScene = false;
        }

        lastLevelSceneIndex = SceneManager.sceneCountInBuildSettings - 1;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Switch()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (fromTransitionType == TransitionType.EXIT)
        {
            var wasClamped = sceneIndex.Increment(
                clamp: lastLevelSceneIndex,
                onClamped: () => Debug.Log($"No more scenes! Final scene index {sceneIndex}.")
            );

            if (wasClamped)
            {
                return;
            }
        }
        else
        {
            sceneIndex.Decrement(clamp: (int)SceneIndex.FirstLevel);
        }

        LoadScene(sceneIndex);
    }

    private void OnExit(TransitionType type)
    {
        fromTransitionType = type;
        Switch();
    }

    private void LoadScene(int sceneBuildIndex)
    {
        transitionPoints.ForEach(p => p.OnExit -= OnExit);
        SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SceneSwitcher: OnSceneLoaded: scene build index {scene.buildIndex}, prev {prevSceneIndex}");

        if (scene.IsPersistentScene())
        {
            targetBuildIndex = startFromCurrentScene
                ? SceneManager.GetActiveScene().buildIndex
                : (int)SceneIndex.FirstLevel;
            prevSceneIndex = targetBuildIndex;

            StartCoroutine(UnloadUnwantedLoadedScenes(targetBuildIndex, onDone: () =>
            {
                var targetScene = SceneManager.GetSceneByBuildIndex(targetBuildIndex);
                if (targetScene.isLoaded)
                {
                    FindTransitionPointsThenEnter(teleportInInitially);
                }
                else
                {
                    Debug.Log($"Target scene is NOT loaded");
                    // ... otherwise load the scene first
                    LoadScene(targetBuildIndex);
                }

                firstLoad = false;
            }));
        }
        else
        {
            SceneManager.SetActiveScene(scene);
            if (scene.buildIndex == prevSceneIndex)
            {
                FindTransitionPointsThenEnter(teleportInInitially);
            }
            else
            {
                SceneManager.UnloadSceneAsync(prevSceneIndex);
            }
            prevSceneIndex = scene.buildIndex;
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (firstLoad) return;

        FindTransitionPointsThenEnter();
    }

    private void FindTransitionPointsThenEnter(bool teleport = true)
    {
        UpdateAllTransitionPoints();

        if (!teleport) return;

        transitionPoints.Find(p => p.Type.ShouldEnter(fromTransitionType))
            .IfNotNull(p =>
            {
                p.PrepareToEnter();
                sync.WaitForSync(() =>
                    {
                        // On Synchronized
                        p.Enter();
                    });
            })
            .IfNull(() => Debug.LogError("SceneSwitcher: no entry points found!!"));
    }

    private void UpdateAllTransitionPoints()
    {
        transitionPoints.Clear();
        transitionPoints.AddRange(FindObjectsByType<SceneTransitionPoint>(FindObjectsSortMode.None));
        transitionPoints.ForEach(p => p.OnExit += OnExit);
    }

    private IEnumerator UnloadUnwantedLoadedScenes(int targetBuildIndex, Action onDone)
    {
        for (int i = lastLevelSceneIndex; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneByBuildIndex(i);

            if (scene.isLoaded && scene.buildIndex.IsLevelSceneIndex() && scene.buildIndex != targetBuildIndex)
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        onDone.Invoke();
    }
}
