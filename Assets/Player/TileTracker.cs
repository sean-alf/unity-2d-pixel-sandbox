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

    private readonly Tilemap[] tilemaps = new Tilemap[1];
    private readonly string[] tilemapNames = new[]
    {
        "Above Ground",
    };
    private Vector3Int currentTileCell;
    private TileType currentType = TileType.None;

    private void Start() => FindTilemaps();

    void Update()
    {
        foreach (var tilemap in tilemaps) ProcessTilemap(tilemap);
    }

    private void ProcessTilemap(Tilemap tilemap)
    {
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

    private void FindTilemaps()
    {
        int index = 0;

        foreach (string tilemapName in tilemapNames) GameObject.Find(tilemapName)
            .WhenNotNull(go => tilemaps[index++] = go.GetComponent<Tilemap>())
            .WhenNull(() => Debug.LogError($"{name} ({GetType().Name}): tilemap {tilemapName} not found!"));
    }
}
