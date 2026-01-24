using System;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public interface IOverride
    {
        public bool IsInteractable { get; }
    }

    /// <summary>
    /// This will get invoked when the IsInteractable state has potentially changed.
    /// </summary>
    public Action<Interactable> onInteractableStateChange;

    [SerializeField]
    private UnityEvent<GameObject> OnInteract;

    public void Interact(GameObject interactor)
    {
        if (!TryGetComponent(out IOverride o) || o.IsInteractable) OnInteract?.Invoke(interactor);
    }

    /// <summary>
    /// This must only be called from the GameObject that this Interactable is attached to.
    /// </summary>
    public void NotifyStateChanged(GameObject caller)
    {
        if (caller != gameObject)
        {
            Debug.LogError($"Interactable ({gameObject}): NotifyStateChanged must only be called from same GameObject!");
            return;
        }
        onInteractableStateChange?.Invoke(this);
    }
}
