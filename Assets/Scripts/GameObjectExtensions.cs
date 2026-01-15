using UnityEngine;

public static class GameObjectExtensions
{
    public static string GetLayerName(this GameObject self) => LayerMask.LayerToName(self.layer);
    public static bool IsOnLayer(this GameObject self, string layerName) => self.GetLayerName() == layerName;
    public static bool IsSelf(this GameObject self, RaycastHit2D hit) => hit.collider != null && hit.collider.gameObject == self;
    public static bool HasHits(this GameObject self, RaycastHit2D[] hits) => hits.Length > 1 || hits.Length == 1 && !self.IsSelf(hits[0]);
}