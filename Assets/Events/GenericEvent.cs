using UnityEngine;
using UnityEngine.Events;

// Leave here as template
// [CreateAssetMenu(fileName = "GenericEvent", menuName = "Scriptable Objects/Generic Event")]
public class GenericEvent<T> : ScriptableObject
{
    [Header("Exposed to Inspector for debugging purposes")]
    public UnityEvent<T> onRaised;

    public void Raise(T t) => onRaised?.Invoke(t);
}
