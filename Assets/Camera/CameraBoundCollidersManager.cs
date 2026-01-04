using UnityEngine;

public class CameraBoundCollidersManager : MonoBehaviour
{
    [SerializeField]
    [Range(0, 128)]
    private int pixelThickness = 32;

    [SerializeField]
    [Range(0, 128)]
    private int pixelOffset = 32;

    public void ResetBounds()
    {
        foreach (var c in GetComponentsInChildren<CameraBoundCollider>())
        {
            if (c.OverrideManager)
            {
                c.ResetBound();
            }
            else
            {
                c.ResetBound(pixelThickness, pixelOffset);
            }
        }
    }

    private void OnValidate()
    {
        ResetBounds();
    }
}
