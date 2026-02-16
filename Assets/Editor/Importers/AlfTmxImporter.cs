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

            if (tilemap.transform.childCount == 1) // If has the polygon collider GameObject
            {
                // Make sure it's on the correct layer
                var polygonCollider = tilemap.transform.GetChild(0).gameObject;
                polygonCollider.layer = tilemapSettings.layer;
                polygonCollider.name = $"{tilemap.name} Collider";
            }
        }
        else
        {
            Debug.LogWarning($"{GetType().Name}: tilemap settings null for tilemap {layer.m_TiledName}");
        }

        if (layer.m_TiledName == "Reflecting Wall") ConfigureReflectingWallTilemap(tilemap);
        if (layer.m_TiledName == "NPC Barrier") ConfigureNPCBarrierTilemap(renderer);
    }

    private void ConfigureReflectingWallTilemap(Tilemap tilemap)
    {
        var polygonColliderObject = tilemap.transform.GetChild(0).gameObject;
        var reflectingWall = polygonColliderObject.AddComponent<ReflectingWall>();
        var allCellPositions = tilemap.cellBounds.allPositionsWithin;

        reflectingWall.tilemap = tilemap;
        allCellPositions.Reset();
        reflectingWall.defaultTileColor = tilemap.GetColor(allCellPositions.Current);

        var path = "Assets/Effects/ReflectionAnimation.prefab";
        var anim = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (anim != null)
        {
            reflectingWall.reflectionAnimationTemplate = anim;
        }
        else
        {
            Debug.LogError($"{GetType().Name}: reflection animation template not found at path {path}!");
        }

    }

    private void ConfigureNPCBarrierTilemap(TilemapRenderer renderer)
    {
        // This tilemap should be invisible
        renderer.enabled = false;
    }

    private static TmxImporterSettings GetOrCreateSettings()
    {
        const string path = "Assets/Settings/TmxImporterSettings.asset";

        var asset = AssetDatabase.LoadAssetAtPath<TmxImporterSettings>(path)
            .WhenNullReturn(() =>
            {
                var asset = ScriptableObject.CreateInstance<TmxImporterSettings>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                return asset;
            });

        return asset;
    }
}
