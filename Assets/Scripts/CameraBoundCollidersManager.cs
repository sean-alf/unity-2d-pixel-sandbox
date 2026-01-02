using UnityEngine;

public class CameraBoundCollidersManager : MonoBehaviour
{
    public void Reset()
    {
        foreach (var c in GetComponentsInChildren<CameraBoundCollider>())
        {
            c.Reset();
        }
    }
}
