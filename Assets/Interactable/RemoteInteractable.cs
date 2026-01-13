using System;
using UnityEngine;

/// <summary>
/// Objects that are interactable by state changes from other Objects or scripts.
/// </summary>
public class RemoteInteractable : MonoBehaviour
{
    public Action onInteract;

    /// <summary>
    /// Only "Interacts" if the passed in id matches this components gameObject.name.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Interact(string id)
    {
        bool match = id == gameObject.name;

        if (match)
        {
            onInteract?.Invoke();
        }

        return match;
    }
}
