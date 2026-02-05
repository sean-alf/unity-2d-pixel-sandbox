using System;
using SuperTiled2Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class TileTracker : MonoBehaviour
{
    public enum TileType
    {
        None,
        DeepMud,
    }

    public UnityEvent<Vector3Int> onTileChange;
    public UnityEvent<Vector3Int, TileType> onTileTypeChange;

    private Tilemap groundMap;
    private Vector3Int currentTileCell;
    private TileType currentType = TileType.None;

    private void Start()
    {
        var go = GameObject.Find("Above Ground");

        if (go != null)
        {
            groundMap = go.GetComponent<Tilemap>();
        }
    }

    void Update()
    {
        if (groundMap == null) return;

        var cell = groundMap.WorldToCell(transform.position);

        if (cell != currentTileCell)
        {
            currentTileCell = cell;
            var tile = groundMap.GetTile<SuperTile>(currentTileCell);
            var type = TileType.None;

            if (tile)
            {
                if (tile.TryGetProperty("tileType", out var prop))
                {
                    var typeString = prop.GetValueAsString();
                    type = Enum.Parse<TileType>(typeString);
                }

                onTileChange?.Invoke(currentTileCell);
            }

            if (type != currentType) onTileTypeChange?.Invoke(currentTileCell, type);
            currentType = type;
        }
    }
}
