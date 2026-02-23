using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ProjectileManager))]
public class PlayerProjectileStash : MonoBehaviour
{
    [SerializeField] private List<PlayerProjectileSO> playerProjectiles;
    [SerializeField] private SpriteChangeEvent spriteChangeEvent;

    public UnityEvent<PlayerProjectileSO> onProjectileChanged;

    private ProjectileManager projectileManager;
    private PlayerProjectileSO selectedProjectile;

    private void Awake()
    {
        projectileManager = GetComponent<ProjectileManager>();
    }

    private void Start()
    {
        foreach (var p in playerProjectiles) projectileManager.AddProjectile(p.Projectile);
        SetProjectiles();
    }

    public void AddProjectile(PlayerProjectileSO playerProjectile)
    {
        playerProjectiles.Add(playerProjectile);
        projectileManager.AddProjectile(playerProjectile.Projectile);
    }

    public void ProjectileManager_OnProjectileChange(ProjectileSO projectile)
    {
        selectedProjectile = playerProjectiles.FirstOrDefault(p => p.Projectile == projectile);
        SetProjectiles();
    }

    private void SetProjectiles()
    {
        if (playerProjectiles.Count == 0 || selectedProjectile == null)
        {
            if (spriteChangeEvent) spriteChangeEvent.Raise(null);
            return;
        }

        if (spriteChangeEvent) spriteChangeEvent.Raise(selectedProjectile.LargeIcon);

        onProjectileChanged?.Invoke(selectedProjectile);
    }
}
