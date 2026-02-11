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
        Stairs,
    }

    public UnityEvent<Tilemap, Vector3Int> onTileChange;
    public UnityEvent<Tilemap, Vector3Int, TileType> onTileTypeChange;

    [Space]
    [Header("Debug")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Vector3Int currentTileCell;
    [SerializeField] private TileType currentType = TileType.None;

    private void Start() => FindTilemap();

    void Update() => ProcessTilemap(tilemap);

    private void ProcessTilemap(Tilemap tilemap)
    {
        // In case the current scene doesn't have the Above Ground tilemap/tile layer
        if (tilemap == null) return;

        var cell = tilemap.WorldToCell(transform.position);

        if (cell != currentTileCell)
        {
            currentTileCell = cell;
            var tile = tilemap.GetTile<SuperTile>(currentTileCell);
            var type = TileType.None;

            if (tile)
            {
                if (tile.TryGetProperty("tileType", out var prop))
                {
                    var typeString = prop.GetValueAsString();
                    type = Enum.Parse<TileType>(typeString);
                }

                onTileChange?.Invoke(tilemap, currentTileCell);
            }

            if (type != currentType) onTileTypeChange?.Invoke(tilemap, currentTileCell, type);
            currentType = type;
        }
    }

    private void FindTilemap()
    {
        GameObject.Find("Above Ground")
            .WhenNotNull(go => tilemap = go.GetComponent<Tilemap>())
            .WhenNull(() => Debug.LogWarning($"{name} ({GetType().Name}): tilemap AboveGround not found!"));
    }
}
