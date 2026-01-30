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
        if (layer.m_TiledName == "Reflecting Wall")
        {
            var reflectingWall = tilemap.transform.GetChild(0).gameObject.AddComponent<ReflectingWall>();
            var allCellPositions = tilemap.cellBounds.allPositionsWithin;
            allCellPositions.Reset();

            reflectingWall.tilemap = tilemap;
            reflectingWall.defaultTileColor = tilemap.GetColor(allCellPositions.Current);
        }
        if (layer.m_TiledName == "NPC Barrier" && tilemap.TryGetComponent(out TilemapRenderer r)) r.enabled = false;
    }
}
