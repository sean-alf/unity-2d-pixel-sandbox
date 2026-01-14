using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<GameObject> OnInteract;

    public void Interact(GameObject interactor)
    {
        OnInteract?.Invoke(interactor);
    }
}
