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

            // Add RigidBody2D, which is a dependency of CompositeCollider2D
            // Set it's body type to Static since it's just a tilemap
            var rb = grid.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            // Now add the composite collider
            grid.gameObject.AddComponent<CompositeCollider2D>();
            grid.gameObject.layer = LayerMask.NameToLayer(LayerNames.Environment);
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
        var colliders = tilemap.GetComponentsInChildren<Collider2D>();

        if (layer.m_TiledName == "Wall") foreach (var collider in colliders) collider.compositeOperation = Collider2D.CompositeOperation.Merge;
        if (layer.m_TiledName == "NPC Barrier" && tilemap.TryGetComponent(out TilemapRenderer r)) r.enabled = false;
    }
}
