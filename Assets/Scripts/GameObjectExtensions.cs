using UnityEngine;

public static class GameObjectExtensions
{
    public static string GetLayerName(this GameObject self) => LayerMask.LayerToName(self.layer);
    public static bool IsOnLayer(this GameObject self, string layerName) => self.GetLayerName() == layerName;
    public static bool IsSelf(this GameObject self, RaycastHit2D hit) => hit.collider != null && hit.collider.gameObject == self;
    public static bool HasHits(this GameObject self, RaycastHit2D[] hits)
    {
        foreach (var hit in hits)
        {
            if (!self.IsSelf(hit)) return true;
        }
        return false;
    }
}