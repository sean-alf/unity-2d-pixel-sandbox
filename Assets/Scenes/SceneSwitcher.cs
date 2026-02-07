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
#if UNITY_EDITOR
    private bool isEditor = true;

    [SerializeField] private bool teleportInInitially = false;
#else
    private bool isEditor = false;
    private bool teleportInInitially = true;
#endif
    [SerializeField] private TargetFollowerCamera followerCamera;

    [Space]
    [Header("Debug")]

    [Tooltip("This is exposed only for debuging purposes. Do not update in the inspector!")]
    [SerializeField] private List<SceneTransitionPoint> transitionPoints;
    [SerializeField] private int targetBuildIndex = (int)SceneIndex.FirstLevel;
    // The index of the scene that starts the levels, and isn't persistent
    [SerializeField] private int prevSceneIndex = (int)SceneIndex.FirstLevel;
    [SerializeField] private int lastLevelSceneIndex;
    [SerializeField] private Logger logger;

    private Synchronizer sync;
    private TransitionType fromTransitionType = TransitionType.EXIT;


    public Logger Logger => logger;

    private void Awake()
    {
        sync = GetComponent<Synchronizer>();
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
        SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Debug.Log($"SceneSwitcher: OnSceneLoaded: scene build index {scene.buildIndex}, prev {prevSceneIndex}");

        if (scene.IsPersistentScene())
        {
            if (isEditor)
            {
                LoadInitialSceneForEditorBuild();
            }
            else
            {
                // Standalone build
                LoadScene((int)SceneIndex.FirstLevel);
            }
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

    private void LoadInitialSceneForEditorBuild()
    {
        var targetScene = SceneManager.GetActiveScene();

        // Debug.Log($"SceneSwitcher: OnSceneLoaded: active scene {targetScene.name}");

        // If the active scene is Persistent, then find the first scene that is not
        if (targetScene.IsPersistentScene())
        {
            // If a non-Persistent scene is found then use it, otherwise use the first level scene
            targetScene = TryGetFirstNonPersistentScene(out targetScene) ? targetScene : SceneManager.GetSceneByBuildIndex((int)SceneIndex.FirstLevel);
        }
        targetBuildIndex = targetScene.buildIndex;
        // Setting these equal signifies that this is the first loaded level
        prevSceneIndex = targetBuildIndex;

        // Debug.Log($"SceneSwitcher: OnSceneLoaded: target scene {targetScene.name}");
        // Debug.Log($"SceneSwitcher: OnSceneLoaded: target scene build index {targetBuildIndex}");

        StartCoroutine(UnloadAllScenesButTarget(targetBuildIndex, onDone: () =>
        {
            // Debug.Log($"SceneSwitcher: OnSceneLoaded: is target scene loaded {targetScene.isLoaded}");

            if (IsSceneInScenesList(targetScene))
            {
                FindTransitionPointsThenEnter(teleportInInitially);
            }
            else
            {
                Debug.Log($"Target scene is NOT loaded");
                // ... otherwise load the scene first
                LoadScene(targetBuildIndex);
            }
        }));
    }

    private bool IsSceneInScenesList(Scene scene)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var sceneToCheck = SceneManager.GetSceneAt(i);
            // Debug.Log($"SceneSwitcher: IsSceneInScenesList: scene {scene.name}, scene to check {sceneToCheck.name}");
            // Debug.Log($"SceneSwitcher: IsSceneInScenesList: scene {scene.buildIndex}, scene to check {sceneToCheck.buildIndex}");
            if (sceneToCheck.buildIndex == scene.buildIndex && sceneToCheck.name == scene.name) return true;
        }

        return false;
    }

    private bool TryGetFirstNonPersistentScene(out Scene scene)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s == null || s.IsPersistentScene()) continue;
            scene = s; // Choose the first scene that is not persistent or null
            return true;
        }

        scene = default;
        return false;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.buildIndex == prevSceneIndex) return;
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

    private IEnumerator UnloadAllScenesButTarget(int targetBuildIndex, Action onDone)
    {
        for (int i = lastLevelSceneIndex; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneByBuildIndex(i);

            if (scene.buildIndex.IsLevelSceneIndex() && scene.buildIndex != targetBuildIndex)
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        onDone.Invoke();
    }
}
