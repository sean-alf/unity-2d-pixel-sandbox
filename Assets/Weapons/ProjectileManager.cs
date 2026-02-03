using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour, ILoggerProvider
{
    public struct StartingPointWithDirection
    {
        public Vector2 direction;
        public Vector3 position;
    }

    public Action<Transform> onProjectileInstantiated;

    [SerializeField] private List<ProjectileSO> projectiles;
    [SerializeField] private MenusAndDisplayManager madm;

    [Space]
    [Header("Debug")]

    [SerializeField] private float coolDownCounter = 0;
    [SerializeField] private Logger logger;

    private ProjectileSO selectedProjectile;
    private int currentIndex = 0;
    private int shootingLayer;
    private int activeProjectiles = 0;

    public Logger Logger => logger;
    /// <summary>
    /// The number of projectiles currently in use.
    /// </summary>
    public int ActiveProjectiles => activeProjectiles;

    // ──────────────────────────────────────────────────────────────
    // GameObject Lifecycle
    // ──────────────────────────────────────────────────────────────

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

    private void Update()
    {
        if (coolDownCounter == 0) return;

        if (coolDownCounter < 0)
        {
            coolDownCounter = 0;
        }
        else
        {
            coolDownCounter -= Time.deltaTime;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Public Control Methods
    // ──────────────────────────────────────────────────────────────

    public void Shoot(params StartingPointWithDirection[] startingPoints) => ShootDelayed(0f, startingPoints);

    public void ShootDelayed(float delayDuration, params StartingPointWithDirection[] startingPoints)
    {
        if (coolDownCounter > 0) return;

        foreach (var s in startingPoints)
        {
            selectedProjectile.Instantiate(p =>
            {
                coolDownCounter = p.CoolDownDuration;

                p.gameObject.SetActive(false);
                p.ClearVelocity();
                p.transform.position = s.position;
                p.gameObject.layer = shootingLayer;

                if (p.TryGetComponent(out Destroyable d))
                {
                    d.onDestroyed += OnProjectileDestroyed;
                }

                onProjectileInstantiated?.Invoke(p.transform);
                ++activeProjectiles;

                if (delayDuration > 0)
                {
                    StartCoroutine(SequencingUtilities.Delay(delayDuration, onRun: () => p.Use(s.direction, s.position)));
                }
                else
                {
                    p.Use(s.direction, s.position);
                }
            });
        }
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

    public void SetShootingLayer(string layer)
    {
        shootingLayer = LayerMask.NameToLayer(layer);
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

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

    // ──────────────────────────────────────────────────────────────
    // Callbacks
    // ──────────────────────────────────────────────────────────────

    private void OnProjectileDestroyed()
    {
        --activeProjectiles;

        // This means that the activeProjectiles count is off, or too many onDestroyed subscribed somehow
        if (activeProjectiles < 0) logger.E("OnProjectileDestroyed called when activeProjectiles == 0!");
    }
}
