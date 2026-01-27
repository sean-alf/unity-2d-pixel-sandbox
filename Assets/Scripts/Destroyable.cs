using System;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    public Action onDestroyed;

    private void OnDestroy()
    {
        onDestroyed?.Invoke();
    }
}
