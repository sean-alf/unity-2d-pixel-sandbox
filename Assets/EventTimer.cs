using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class EventTimer : MonoBehaviour
{
    [SerializeField] private float duration;

    public UnityEvent onTimerExpire;

    private WaitForSeconds wait;
    private Coroutine coroutine;

    private bool IsRunning => coroutine != null;

    private void Awake() => wait = new(duration);

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            wait = new(duration);
        }
        else
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                wait = new(duration);
            };
        }
    }
#endif

    public void StartTimer()
    {
        if (onTimerExpire == null)
        {
            Debug.LogError("EventTimer: Trying to start timer with no events!", this);
            return;
        }

        if (IsRunning) return;
        coroutine = StartCoroutine(Begin());
    }

    public void StopTimer()
    {
        if (!IsRunning) return;
        StopCoroutine(coroutine);
        coroutine = null;
    }

    public void ReloadTimer()
    {
        StopTimer();
        StartTimer();
    }

    private IEnumerator Begin()
    {
        yield return wait;
        onTimerExpire?.Invoke();
        coroutine = null;
    }
}
