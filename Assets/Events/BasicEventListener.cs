using UnityEngine;
using UnityEngine.Events;

public class BasicEventListener : MonoBehaviour
{
    [Header("Listen to this event")]
    [SerializeField] private BasicEvent eventSO;

    [Header("Actions when event is raised")]
    public UnityEvent onRaised;

    private void OnEnable() => eventSO.onRaised.AddListener(OnEventRaised);

    private void OnDisable() => eventSO.onRaised.RemoveListener(OnEventRaised);

    private void OnEventRaised() => onRaised?.Invoke();
}
