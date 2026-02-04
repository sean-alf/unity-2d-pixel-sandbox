using UnityEngine;
using UnityEngine.Events;

public class GenericEventListener<T> : MonoBehaviour
{
    [Header("Listen to this event")]
    [SerializeField] private GenericEvent<T> eventSO;

    [Header("Actions when event is raised")]
    public UnityEvent<T> onRaised;

    private void OnEnable() => eventSO.onRaised.AddListener(OnEventRaised);

    private void OnDisable() => eventSO.onRaised.RemoveListener(OnEventRaised);

    private void OnEventRaised(T t) => onRaised?.Invoke(t);
}
