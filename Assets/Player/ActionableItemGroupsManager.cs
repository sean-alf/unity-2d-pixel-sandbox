using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ProjectileManager))]
public class ActionableItemGroupsManager : MonoBehaviour
{
    public enum ItemGroupIndex
    {
        MeleeWeapons,
        Projectiles,
        Tools,
    }

    public interface IActionableItemGroup
    {
        /// <summary>
        /// Cycle to the next item, wrapping to the start.
        /// </summary>
        public void CycleToNextItem();
        /// <summary>
        /// Cycle to the previous item, wrapping to the end.
        /// </summary>
        public void CycleToPreviousItem();
    }

    public UnityEvent<ItemGroupIndex> onItemGroupIndexChanged;

    private ProjectileManager projectileManager;
    private ItemGroupIndex itemGroupIndex = ItemGroupIndex.MeleeWeapons;

    private readonly Dictionary<ItemGroupIndex, IActionableItemGroup> itemGroups = new();

    private void Awake()
    {
        projectileManager = GetComponent<ProjectileManager>();

        itemGroups.Add(ItemGroupIndex.MeleeWeapons, null);
        itemGroups.Add(ItemGroupIndex.Projectiles, projectileManager);
        itemGroups.Add(ItemGroupIndex.Tools, null);
    }

    public void CycleToNextGroup()
    {
        itemGroupIndex = itemGroupIndex.IncrementAndWrap();
        onItemGroupIndexChanged?.Invoke(itemGroupIndex);
    }

    public void CycleToPreviousGroup()
    {
        itemGroupIndex = itemGroupIndex.DecrementAndWrap();
        onItemGroupIndexChanged?.Invoke(itemGroupIndex);
    }

    public void CycleToNextItem()
    {
        var group = itemGroups[itemGroupIndex];

        if (group != null)
        {
            group.CycleToNextItem();
        }
        else
        {
            Debug.LogWarning($"{name} ({GetType().Name}): CycleToNextItem: no group set for {itemGroupIndex}!");
        }
    }

    public void CycleToPreviousItem()
    {
        var group = itemGroups[itemGroupIndex];

        if (group != null)
        {
            group.CycleToPreviousItem();
        }
        else
        {
            Debug.LogWarning($"{name} ({GetType().Name}): CycleToPreviousItem: no group set for {itemGroupIndex}!");
        }
    }
}
