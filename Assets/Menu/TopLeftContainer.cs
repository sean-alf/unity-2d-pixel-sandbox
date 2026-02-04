using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemGroupIndex = ActionableItemGroupsManager.ItemGroupIndex;

public class TopLeftContainer : MonoBehaviour
{
    private readonly struct ImageGroup
    {
        public readonly Image icon;
        public readonly Image background;

        public ImageGroup(Image icon, Image background)
        {
            this.icon = icon;
            this.background = background;
        }
    }

    [SerializeField] private Sprite selectedBackgroundSprite;
    [SerializeField] private Sprite unselectedBackgroundSprite;
    [SerializeField] private Image meleeWeaponIconImage;
    [SerializeField] private Image meleeWeaponContainerImage;
    [SerializeField] private Image projectileIconImage;
    [SerializeField] private Image projectileContainerImage;
    [SerializeField] private Image toolIconImage;
    [SerializeField] private Image toolContainerImage;
    [SerializeField] private EnergyIndicator energyIndicator;

    private readonly Dictionary<ItemGroupIndex, ImageGroup> imageGroups = new();
    private ItemGroupIndex oldIndex = ItemGroupIndex.MeleeWeapons;

    private void Awake()
    {
        projectileIconImage.sprite = null;
        projectileIconImage.enabled = false;

        imageGroups.Add(ItemGroupIndex.MeleeWeapons, new(meleeWeaponIconImage, meleeWeaponContainerImage));
        imageGroups.Add(ItemGroupIndex.Projectiles, new(projectileIconImage, projectileContainerImage));
        imageGroups.Add(ItemGroupIndex.Tools, new(toolIconImage, toolContainerImage));

        MakeSelected(oldIndex);
    }

    public void OnMeleeWeaponChanged(MeleeWeaponSO meleeWeapon)
    {
        meleeWeaponIconImage.sprite = meleeWeapon.MenuIcon;
        meleeWeaponIconImage.enabled = meleeWeapon.MenuIcon != null;
    }

    public void OnProjectileChanged(ProjectileSO projectile)
    {
        projectileIconImage.sprite = projectile.MenuIcon;
        projectileIconImage.enabled = projectile.MenuIcon != null;
    }

    public void OnToolChanged() => throw new($"{GetType().Name}.OnToolChanged: Not Yet Implemeneted");

    public void OnItemGroupIndexChanged(ItemGroupIndex index)
    {
        MakeUnselected(oldIndex);
        MakeSelected(index);
        oldIndex = index;
    }

    private void MakeUnselected(ItemGroupIndex index) => imageGroups[index].background.sprite = unselectedBackgroundSprite;

    private void MakeSelected(ItemGroupIndex index) => imageGroups[index].background.sprite = selectedBackgroundSprite;
}
