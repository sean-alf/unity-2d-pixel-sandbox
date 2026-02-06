using System;
using System.Collections.Generic;
using System.Linq;
using SuperTiled2Unity;
using SuperTiled2Unity.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[AutoCustomTmxImporter()]
public class AlfTmxImporter : CustomTmxImporter
{
    private TmxImporterSettings settings;

    public override void TmxAssetImported(TmxAssetImportedArgs args)
    {
        var superMap = args.ImportedSuperMap;

        // Ignore any rules maps
        if (superMap.name.Contains("rules", StringComparison.OrdinalIgnoreCase)) return;

        settings = GetOrCreateSettings();
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
        foreach (var tilemap in tilemaps)
        {
            SetupTilemap(tilemap);

            if (settings.tilemapSettingsList.FirstOrDefault(t => tilemap.name == t.name) == null)
            {
                settings.tilemapSettingsList.Insert(Array.IndexOf(tilemaps, tilemap), new()
                {
                    name = tilemap.name,
                });
            }
        }
    }

    private void SetupTilemap(Tilemap tilemap)
    {
        var layer = tilemap.GetComponent<SuperTileLayer>();
        var renderer = tilemap.GetComponent<TilemapRenderer>();
        var tilemapSettings = settings.tilemapSettingsList.FirstOrDefault(s => s.name == layer.m_TiledName);

        if (tilemapSettings != null)
        {
            tilemap.gameObject.layer = tilemapSettings.layer;
            renderer.sortingOrder = tilemapSettings.sortingOrder;
        }
        else
        {
            Debug.LogWarning($"{GetType().Name}: tilemap settings null for tilemap {layer.m_TiledName}");
        }

        if (layer.m_TiledName == "Reflecting Wall") ConfigureReflectingWallTilemap(tilemap);
        if (layer.m_TiledName == "Above Ground") ConfigureAboveGroundTilemap(tilemap);
        if (layer.m_TiledName == "NPC Barrier") ConfigureNPCBarrierTilemap(renderer);

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

    private void ConfigureNPCBarrierTilemap(TilemapRenderer renderer)
    {
        // This tilemap should be invisible
        renderer.enabled = false;
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

    private static TmxImporterSettings GetOrCreateSettings()
    {
        const string path = "Assets/Settings/TmxImporterSettings.asset";

        var asset = AssetDatabase.LoadAssetAtPath<TmxImporterSettings>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<TmxImporterSettings>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        return asset;
    }
}
