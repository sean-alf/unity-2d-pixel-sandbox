using System;
using UnityEngine;
using UnityEngine.Events;

public class Destroyable : MonoBehaviour
{
    public UnityEvent onDestroyed;

    public void DestroySelf() => Destroy(gameObject);

    private void OnDestroy()
    {
        onDestroyed?.Invoke();
    }
}
