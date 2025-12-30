using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class ChildOrderer : MonoBehaviour
{
    [SerializeField]
    private int sortingOrder;

    private void OnEnable()
    {
        SetSortingOrder();
    }

    private void OnValidate()
    {
        SetSortingOrder();
    }

    private void SetSortingOrder()
    {
        // Set sorting order for self
        var renderer = GetComponent<SpriteRenderer>();
        renderer.sortingOrder = sortingOrder;

        // Set sorting order for all children
        foreach (var r in GetComponentsInChildren<SpriteRenderer>())
        {
            r.sortingOrder = sortingOrder;
        }
    }
}
