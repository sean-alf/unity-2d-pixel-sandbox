using System;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, BetterInputManager.IInputChangeRequestor
{
    public interface IInteractor
    {
        public GameObject GameObject { get; }
        public BetterInputManager InputManager { get; }
    }

    public interface IOverride
    {
        public bool IsInteractable { get; }
    }

    [SerializeField][Range(0, 100)] int inputChangeRequestPriority;

    /// <summary>
    /// This will get invoked when the IsInteractable state has potentially changed.
    /// </summary>
    public Action<Interactable> onInteractableStateChange;

    [SerializeField]
    private UnityEvent<IInteractor> OnInteract;

    public int Priority => inputChangeRequestPriority;

    public string Name => $"{name} ({GetType().Name})";

    public BetterInputManager.InputType InputType => BetterInputManager.InputType.None;

    public void Interact(IInteractor interactor)
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
