using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeWeaponManager : MonoBehaviour, ActionableItemGroupsManager.IActionableItemGroup
{
    [SerializeField] private List<MeleeWeaponSO> meleeWeapons;
    [SerializeField] private SpriteChangeEvent spriteChangeEvent;

    private readonly List<MeleeWeaponAndIcon> meleeWeaponAndIcons = new();
    private MeleeWeaponAndIcon currentWeaponAndIcon;
    private int weaponsCount;
    private int currentIndex = 0;
    private bool isAttacking = false;

    private void Awake()
    {
        weaponsCount = meleeWeapons.Count;

        // Instantiate the existing weapons, if any
        foreach (var weaponSO in meleeWeapons)
        {
            var weapon = Instantiate(weaponSO.WeaponTemplate).GetComponent<IMeleeWeapon>();
            meleeWeaponAndIcons.Add(new(weapon, so: weaponSO));
        }

        SetWeapon();
    }

    public void Attack(IMeleeWeaponWielder wielder)
    {
        if (isAttacking) return;
        isAttacking = true;
        currentWeaponAndIcon.weapon.Attack(wielder);
    }

    // ActionableItemGroupsManager.IActionableItemGroup
    public void CycleToNextItem()
    {
        currentIndex = (currentIndex + 1) % weaponsCount;
        SetWeapon();
    }

    // ActionableItemGroupsManager.IActionableItemGroup
    public void CycleToPreviousItem()
    {
        currentIndex = (currentIndex - 1 + weaponsCount) % weaponsCount;
        SetWeapon();
    }

    private void SetWeapon()
    {
        if (currentWeaponAndIcon.IsValid)
        {
            currentWeaponAndIcon.weapon.UnregisterOnAttackFinished(OnAttackFinished);
        }

        currentWeaponAndIcon = meleeWeaponAndIcons[currentIndex];

        if (currentWeaponAndIcon.IsValid)
        {
            currentWeaponAndIcon.weapon.RegisterOnAttackFinished(OnAttackFinished);
            spriteChangeEvent.Raise(currentWeaponAndIcon.so.MenuIcon);
        }
        else
        {
            Debug.LogError($"{name} ({GetType().Name}): weapon is invalid!");
        }
    }

    private void OnAttackFinished() => isAttacking = false;

    private readonly struct MeleeWeaponAndIcon
    {
        public readonly IMeleeWeapon weapon;
        public readonly MeleeWeaponSO so;

        public MeleeWeaponAndIcon(IMeleeWeapon weapon, MeleeWeaponSO so)
        {
            this.weapon = weapon;
            this.so = so;
        }

        public bool IsValid => weapon != null && so != null;
    }
}
