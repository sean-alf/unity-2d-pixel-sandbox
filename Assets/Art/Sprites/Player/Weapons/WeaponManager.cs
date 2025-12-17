using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private static readonly string TAG = "WeaponManager";
    private static readonly ILogger logger = Debug.unityLogger;

    private Weapon activeWeapon;

    public PlayerMovement movement;

    public List<Weapon> weapons;
    public bool attacking = false;

    void Awake()
    {
        movement.OnDirectionChange += OnDirectionChange;
        movement.OnAttackStart += OnAttackStart;
        movement.OnAttackEnd += OnAttackEnd;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Make sure all weapons are disabled to start
        foreach (var w in weapons)
        {
            w.gameObject.SetActive(false);
        }

        if (weapons.Count > 0)
        {
            // Make the first weapon active
            activeWeapon = weapons[0];
        }
        else
        {
            logger.LogError(TAG, "No weapons set!");
        }
    }

    private void OnDirectionChange(PlayerMovement.PlayerMovementData data)
    {
        
    }

    private void OnAttackStart()
    {
        logger.Log(TAG, "OnAttackStart");
        if (attacking) return;

        if (activeWeapon)
        {
            attacking = true;
            activeWeapon.Attack();
        }
    }

    private void OnAttackEnd()
    {
        logger.Log(TAG, "OnAttackEnd");
        if (activeWeapon)
        {
            activeWeapon.AttackEnd();
            attacking = false;
        }
    }
}
