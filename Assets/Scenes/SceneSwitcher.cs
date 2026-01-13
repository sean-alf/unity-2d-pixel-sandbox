using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using IEnumerator = System.Collections.IEnumerator;

public enum SceneIndex
{
    Persistent = 0,
    FirstLevel = 1,
}

static class SceneSwitcherExtensions
{
    public static int ToInt(this SceneIndex s) => Convert.ToInt32(s);

    public static bool IsLoaded(this SceneIndex s) => SceneManager.GetSceneByBuildIndex(s.ToInt()).isLoaded;

    public static bool IsLevelSceneIndex(this int sceneBuildIndex) => sceneBuildIndex >= SceneIndex.FirstLevel.ToInt();

    public static bool IsFirstLevelScene(this Scene s) => s.buildIndex == SceneIndex.FirstLevel.ToInt();

    public static bool IsGreaterThanFirstLevelScene(this int sceneBuildIndex) => sceneBuildIndex > SceneIndex.FirstLevel.ToInt();
}

[RequireComponent(typeof(Synchronizer))]
public class SceneSwitcher : MonoBehaviour, ILoggerProvider
{
    [SerializeField]
    [Tooltip("This is exposed only for debuging purposes. Do not update in the inspector!")]
    private List<SceneTransitionPoint> transitionPoints;

    [Space]
    [Header("Debug")]

    [SerializeField]
    private Logger logger;

    private Synchronizer sync;
    private TransitionType fromTransitionType = TransitionType.EXIT;
    // The index of the scene that starts the levels, and isn't persistent
    private int prevSceneIndex = SceneIndex.FirstLevel.ToInt();
    private bool firstLoad = true;

    public Logger Logger => logger;

    private void Awake()
    {
        sync = GetComponent<Synchronizer>();
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

    void Start()
    {
        StartCoroutine(UnloadUnwantedLoadedScenes(() =>
        {
            // On Done

            // SceneManager.sceneLoaded gets called for scenes that are automatically loaded as well 
            // as ones that are programmatically loaded. 
            // This means that if the First Level scene is loaded in the Scene Editor, 
            // it will get loaded automatically and SceneManager.sceneLoaded will get called for it.
            // In other words, there's no need to try and manage it here.
            // Simply load it if it isn't already loaded.
            if (!SceneIndex.FirstLevel.IsLoaded())
            {
                Debug.Log($"First level is NOT loaded");
                // ... otherwise load the scene first
                LoadScene(SceneIndex.FirstLevel.ToInt());
            }
        }));
    }

    private void Switch()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (fromTransitionType == TransitionType.EXIT)
        {
            var wasClamped = sceneIndex.Increment(
                clamp: SceneManager.sceneCountInBuildSettings - 1,
                onClamped: () => logger.I($"No more scenes! Final scene index {sceneIndex}.")
            );

            if (wasClamped)
            {
                return;
            }
        }
        else
        {
            sceneIndex.Decrement(clamp: SceneIndex.FirstLevel.ToInt());
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
        if (!scene.buildIndex.IsLevelSceneIndex() || (firstLoad && !scene.IsFirstLevelScene())) return;

        Debug.Log($"OnSceneLoaded: scene build index {scene.buildIndex}, prev {prevSceneIndex}");

        SceneManager.SetActiveScene(scene);

        if (firstLoad && IsFirstLoad(scene))
        {
            firstLoad = false;
            FindTransitionPointsThenEnter();
        }
        else
        {
            SceneManager.UnloadSceneAsync(prevSceneIndex);
        }

        prevSceneIndex = scene.buildIndex;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (firstLoad) return;

        FindTransitionPointsThenEnter();
    }

    private void FindTransitionPointsThenEnter()
    {
        UpdateAllTransitionPoints();

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

    private IEnumerator UnloadUnwantedLoadedScenes(Action onDone)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneAt(i);

            // If it's not the first level (or below, i.e., persistent scene(s)), then unload it
            if (scene.buildIndex.IsGreaterThanFirstLevelScene())
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        onDone.Invoke();
    }

    private bool IsFirstLoad(Scene scene) => scene.buildIndex == prevSceneIndex;
}
