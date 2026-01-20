using System.Collections.Generic;
using SuperTiled2Unity;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SuperTileWithCellPosition<E>
    where E : System.Enum
{
    public readonly SuperTile tile;
    public readonly Vector2 worldCenter;
    public readonly Vector3Int cellPosition;
    public readonly E type;

    public SuperTileWithCellPosition(SuperTile tile, Vector2 worldCenter, Vector3Int cellPosition, E type)
    {
        this.tile = tile;
        this.worldCenter = worldCenter;
        this.cellPosition = cellPosition;
        this.type = type;
    }

    public bool IsComparable(SuperTileWithCellPosition<E> other) => tile == other.tile &&
            worldCenter == other.worldCenter &&
            cellPosition == other.cellPosition &&
            EqualityComparer<E>.Default.Equals(type, other.type);

    public bool IsValid() => tile != null;
}

public static class TilemapExtensions
{
    private static readonly float TileAnchorOffset = 0.5f;

    public static SuperTileWithCellPosition<E> GetSuperTileFromWorldPosition<E>(this Tilemap t, Vector2 pos)
        where E : System.Enum
    {
        var p = t.WorldToCell(pos);
        var tile = t.GetTile<SuperTile>(p);
        tile.TryGetProperty("type", out CustomProperty prop);

        if (tile == null) return null;

        return new(
            tile: t.GetTile<SuperTile>(p),
            worldCenter: t.GetCellCenterWorld(p) + new Vector3(TileAnchorOffset, TileAnchorOffset, 0),
            cellPosition: p,
            type: prop.GetValueAsEnum<E>()
        );
    }
}
