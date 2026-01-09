using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField]
    [Tooltip("This is exposed only for debuging purposes. Do not update in the inspector!")]
    private List<SceneTransitionPoint> transitionPoints;

    private TransitionType cause = TransitionType.EXIT;
    private int sceneIndex = 0;
    private int prevSceneIndex = 0;

    void Start()
    {
        // Scene's already loaded here, so the player just need to make an entrance
        FindTransitionPointsThenEnter();
    }

    private void GetAllEntryAndExitPoints()
    {
        transitionPoints.Clear();
        transitionPoints.AddRange(FindObjectsByType<SceneTransitionPoint>(FindObjectsSortMode.None));

        foreach (var e in transitionPoints)
        {
            e.OnExit += OnExit;
        }
    }

    private void OnExit(TransitionType type)
    {
        cause = type;
        Switch();
    }

    private void Switch()
    {
        prevSceneIndex = sceneIndex;

        if (cause == TransitionType.EXIT)
        {
            if (++sceneIndex == SceneManager.sceneCountInBuildSettings) sceneIndex = SceneManager.sceneCountInBuildSettings - 1;
        }
        else
        {
            if (--sceneIndex < 0) sceneIndex = 0;
        }

        foreach (var e in transitionPoints)
        {
            e.OnExit -= OnExit;
        }

        LoadUpcomingScene();
    }

    private void LoadUpcomingScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
    }

    private void UnloadExpiredScene()
    {
        if (sceneIndex != prevSceneIndex)
        {
            var operation = SceneManager.UnloadSceneAsync(prevSceneIndex);
            operation.completed += OnUnloadComplete;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnloadExpiredScene();
    }

    private void FindTransitionPointsThenEnter()
    {
        GetAllEntryAndExitPoints();

        foreach (var e in transitionPoints)
        {
            if ((cause == TransitionType.EXIT && e.Type == TransitionType.ENTRY) || (cause == TransitionType.ENTRY && e.Type == TransitionType.EXIT))
            {
                e.Enter();
            }
        }
    }

    private void OnUnloadComplete(AsyncOperation operation)
    {
        operation.completed -= OnUnloadComplete;
        FindTransitionPointsThenEnter();
    }
}
