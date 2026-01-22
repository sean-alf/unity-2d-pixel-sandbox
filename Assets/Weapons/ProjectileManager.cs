using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour, ILoggerProvider
{
    public struct StartingPointWithDirection
    {
        public Vector2 direction;
        public Vector3 position;
    }

    public struct StartingPointWithAngleDegrees
    {
        public float angleDegrees;
        public Vector3 position;
    }

    public struct StartingPointWithAngleRads
    {
        public float angleRads;
        public Vector3 position;
    }

    [SerializeField]
    private List<ProjectileSO> projectiles;

    [SerializeField]
    private MenusAndDisplayManager madm;

    [Space]
    [Header("Debug")]

    [SerializeField]
    private Logger logger;

    private ProjectileSO selectedProjectile;
    private int currentIndex = 0;
    private int shootingLayer;
    private int activeProjectiles = 0;

    public Logger Logger => logger;
    /// <summary>
    /// The number of projectiles currently in use.
    /// </summary>
    public int ActiveProjectiles => activeProjectiles;

    private void Awake()
    {
        if (projectiles == null || projectiles.Count == 0)
        {
            Debug.LogError("Projectile Manager: Projectiles not set!!");
            return;
        }

        // Automatically set the selected projectile to the first one in the list
        selectedProjectile = projectiles[currentIndex];
    }

    private void Start()
    {
        SetProjectiles();
    }

    public void Shoot(params StartingPointWithDirection[] startingPoints)
    {
        foreach (var s in startingPoints)
        {
            selectedProjectile.Instantiate(p =>
            {
                p.gameObject.layer = shootingLayer;
                p.onDestroyed += OnProjectileDestroyed;
                p.Use(s.direction, s.position);
                ++activeProjectiles;
            });
        }
        ;
    }

    public void Shoot(params StartingPointWithAngleDegrees[] startingPoints)
    {
        foreach (var s in startingPoints)
        {
            selectedProjectile.Instantiate(p =>
            {
                p.gameObject.layer = shootingLayer;
                p.onDestroyed += OnProjectileDestroyed;
                p.Use(s.angleDegrees, s.position);
                ++activeProjectiles;
            });
        }
        ;
    }

    public void Shoot(params StartingPointWithAngleRads[] startingPoints)
    {
        foreach (var s in startingPoints)
        {
            selectedProjectile.Instantiate(p =>
            {
                p.gameObject.layer = shootingLayer;
                p.onDestroyed += OnProjectileDestroyed;
                p.Use(s.angleRads, s.position);
                ++activeProjectiles;
            });
        }
        ;
    }

    public void SelectNext()
    {
        currentIndex = (currentIndex + 1) % projectiles.Count;
        selectedProjectile = projectiles[currentIndex];
        SetProjectiles();
    }

    public void SelectPrevious()
    {
        currentIndex = (currentIndex - 1 + projectiles.Count) % projectiles.Count;
        selectedProjectile = projectiles[currentIndex];
        SetProjectiles();
    }

    private void SetProjectiles()
    {
        if (shootingLayer == 0)
        {
            logger.E("shooting layer not set!!");
            return;
        }

        if (madm)
        {
            madm.GetTopLeftContainer().UpdatePrimaryWeaponIcon(selectedProjectile.MenuIcon);
        }
    }

    public void SetShootingLayer(string layer)
    {
        shootingLayer = LayerMask.NameToLayer(layer);
    }

    private void OnProjectileDestroyed()
    {
        --activeProjectiles;

        // This means that the activeProjectiles count is off, or too many onDestroyed subscribed somehow
        if (activeProjectiles < 0) logger.E("OnProjectileDestroyed called when activeProjectiles == 0!");
    }
}
