using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(MeleeWeaponManager))]
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

    [SerializeField] private ItemGroupIndexChangeEvent itemGroupIndexChangeEvent;

    private MeleeWeaponManager meleeWeaponManager;
    private ProjectileManager projectileManager;
    private ItemGroupIndex itemGroupIndex = ItemGroupIndex.MeleeWeapons;

    private readonly Dictionary<ItemGroupIndex, IActionableItemGroup> itemGroups = new();

    private void Awake()
    {
        meleeWeaponManager = GetComponent<MeleeWeaponManager>();
        projectileManager = GetComponent<ProjectileManager>();

        itemGroups.Add(ItemGroupIndex.MeleeWeapons, meleeWeaponManager);
        itemGroups.Add(ItemGroupIndex.Projectiles, projectileManager);
        itemGroups.Add(ItemGroupIndex.Tools, null);
    }

    private void Start()
    {
        // Do this here so that we know anything listening will be ready
        itemGroupIndexChangeEvent.Raise(itemGroupIndex);
    }

    public void CycleToNextGroup()
    {
        itemGroupIndex = itemGroupIndex.IncrementAndWrap();
        itemGroupIndexChangeEvent.Raise(itemGroupIndex);
    }

    public void CycleToPreviousGroup()
    {
        itemGroupIndex = itemGroupIndex.DecrementAndWrap();
        itemGroupIndexChangeEvent.Raise(itemGroupIndex);
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
