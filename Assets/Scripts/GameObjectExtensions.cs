using UnityEngine;

public static class GameObjectExtensions
{
    public static string GetLayerName(this GameObject go) => LayerMask.LayerToName(go.layer);
    public static bool IsOnLayer(this GameObject go, string layerName) => go.GetLayerName() == layerName;
}