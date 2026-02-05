using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BasicEvent", menuName = "Scriptable Objects/BasicEvent")]
public class BasicEvent : ScriptableObject
{
    [Header("Exposed to Inspector for debugging purposes")]
    public UnityEvent onRaised;

    public void Raise() => onRaised?.Invoke();
}
