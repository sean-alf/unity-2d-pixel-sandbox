using System.Collections.Generic;
using UnityEngine;

public class SpriteRendererVisibilityManager : MonoBehaviour
{
    [Header("Debug")]

    [SerializeField] private List<SpriteRenderer> renderers = new();

    private void Awake()
    {
        FindSpriteRenderersRecursive(gameObject);
    }

    public void SetVisibility(bool visible)
    {
        foreach (var sr in renderers)
        {
            sr.color = sr.color.WithAlpha(visible ? 1f : 0f);
        }
    }

    private void FindSpriteRenderersRecursive(GameObject go)
    {
        var srs = go.GetComponentsInChildren<SpriteRenderer>();

        if (srs.Length == 0) return;

        foreach (var sr in srs)
        {
            if (!renderers.Contains(sr))
            {
                renderers.Add(sr);
                FindSpriteRenderersRecursive(sr.gameObject);
            }
        }
    }
}
