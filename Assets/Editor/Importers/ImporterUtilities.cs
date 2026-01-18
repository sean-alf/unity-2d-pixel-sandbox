using System.Collections.Generic;
using SuperTiled2Unity;
using SuperTiled2Unity.Editor;
using UnityEngine;

public static class ImporterUtilities
{
    public static bool TryGetSuperCustomProperties(this Component target, out SuperCustomProperties props, bool log)
    {
        if (target.TryGetComponent(out props))
        {
            if (log) Debug.Log($"{target.name}: Custom properties count is {props.m_Properties.Count}");

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

    public static void LogAllChildren(TmxAssetImportedArgs args)
    {
        var t = args.ImportedSuperMap.gameObject.transform;
        var children = GetAllDescendants(t);

        foreach (var child in children)
        {
            Debug.Log($"child found {child.name}");
        }
    }

    private static List<GameObject> GetAllDescendants(Transform transform)
    {
        List<GameObject> children = new();
        CollectDescendants(children, transform);
        return children;
    }

    private static void CollectDescendants(List<GameObject> children, Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            children.Add(t.gameObject);
            CollectDescendants(children, t);
        }
    }

}
