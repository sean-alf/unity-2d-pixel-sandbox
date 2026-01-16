using System.Collections.Generic;
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

        if (layer.m_TiledName == "Wall")
        {
            foreach (var collider in colliders) collider.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        if (layer.m_TiledName == "NPC Barrier" && tilemap.TryGetComponent(out TilemapRenderer r)) r.enabled = false;
    }

    private bool TryGetProperties(Component target, out SuperCustomProperties props, bool log)
    {
        if (target.TryGetComponent(out props))
        {
            Debug.Log($"{target.name}: Custom properties count is {props.m_Properties.Count}");

            if (props.m_Properties.Count == 0)
            {
                // Not worth trying to get properties if there are none...
                return false;
            }

            if (log)
            {
                foreach (var p in props.m_Properties)
                {
                    Debug.Log($"\tName: {p.m_Name}\n\tType: {p.m_Type}\n\tValue: {p.m_Value}\n");
                }
            }

            return true;
        }
        else
        {
            Debug.LogError($"No {nameof(SuperCustomProperties)} attached to {target.name}");
        }

        return false;
    }

    private void LogAllChildren(TmxAssetImportedArgs args)
    {
        var t = args.ImportedSuperMap.gameObject.transform;
        var children = GetAllDescendants(t);

        foreach (var child in children)
        {
            Debug.Log($"child found {child.name}");
        }
    }

    private List<GameObject> GetAllDescendants(Transform transform)
    {
        List<GameObject> children = new();
        CollectDescendants(children, transform);
        return children;
    }

    private void CollectDescendants(List<GameObject> children, Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            children.Add(t.gameObject);
            CollectDescendants(children, t);
        }
    }
}
