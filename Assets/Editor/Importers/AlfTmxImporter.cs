using System;
using System.Linq;
using SuperTiled2Unity;
using SuperTiled2Unity.Editor;
using UnityEngine;
using UnityEngine.Tilemaps;

[AutoCustomTmxImporter()]
public class AlfTmxImporter : CustomTmxImporter
{
    public override void TmxAssetImported(TmxAssetImportedArgs args)
    {
        var superMap = args.ImportedSuperMap;
        SetupGrid(superMap.gameObject);
    }

    private void SetupGrid(GameObject target)
    {
        var grid = target.GetComponentInChildren<Grid>();

        if (grid)
        {
            var tilemaps = grid.GetComponentsInChildren<Tilemap>();

            if (tilemaps.Count() > 0)
            {
                SetupTilemaps(tilemaps);
            }
            else
            {
                Debug.LogError($"{grid.name}: No children with {nameof(Tilemap)} attached!");
            }
        }
        else
        {
            Debug.LogError($"No child with Grid found: {target.name}");
        }
    }

    private void SetupTilemaps(Tilemap[] tilemaps)
    {
        foreach (var t in tilemaps)
        {
            SetupTilemap(t);
        }
    }

    private void SetupTilemap(Tilemap tilemap)
    {
        var layer = tilemap.GetComponent<SuperTileLayer>();

        if (layer.m_TiledName == "Reflecting Wall") ConfigureReflectingWallTilemap(tilemap);
        if (layer.m_TiledName == "Above Ground") ConfigureAboveGroundTilemap(tilemap);
        if (layer.m_TiledName == "NPC Barrier" && tilemap.TryGetComponent(out TilemapRenderer r)) r.enabled = false;

        if (tilemap.transform.childCount == 1)
        {
            if (layer.m_TiledName == "Above Ground")
            {
                SetTileColliderType(tilemap, tile => tile.GetPropertyValueAsBool("solid"));
            }
            else
            {
                SetTileColliderType(tilemap);
            }
            UnityEngine.Object.DestroyImmediate(tilemap.transform.GetChild(0).gameObject);
            var rb = tilemap.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            tilemap.gameObject.AddComponent<CompositeCollider2D>();
            var collider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
        }
    }

    private void ConfigureReflectingWallTilemap(Tilemap tilemap)
    {
        var reflectingWall = tilemap.gameObject.AddComponent<ReflectingWall>();
        var allCellPositions = tilemap.cellBounds.allPositionsWithin;

        reflectingWall.tilemap = tilemap;
        allCellPositions.Reset();
        reflectingWall.defaultTileColor = tilemap.GetColor(allCellPositions.Current);
    }

    private void ConfigureAboveGroundTilemap(Tilemap tilemap)
    {
        var tilemapManager = tilemap.gameObject.AddComponent<AboveGroundTilemapManager>();
        tilemapManager.tilemap = tilemap;
    }

    private void SetTileColliderType(Tilemap tilemap, Func<SuperTile, bool> pred = null)
    {
        foreach (var cell in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile<SuperTile>(cell);
            if (tile == null || (pred != null && !pred(tile))) continue;
            tile.m_ColliderType = Tile.ColliderType.Sprite;
            tilemap.SetTile(cell, tile);
        }
    }
}
