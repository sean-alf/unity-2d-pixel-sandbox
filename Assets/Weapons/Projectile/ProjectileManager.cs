using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileManager : MonoBehaviour, ActionableItemGroupsManager.IActionableItemGroup, ILoggerProvider
{
    public struct StartingPointWithDirection
    {
        public Vector2 direction;
        public Vector3 position;
    }

    [Header("Only select one layer!!")]
    [SerializeField] private LayerMask shootingLayer;
    // Keep this for inter-scene communication
    public UnityEvent<ProjectileSO> onProjectileChanged;
    public UnityEvent<Transform> onProjectileInstantiated;

    [SerializeField] private List<ProjectileSO> projectiles;

    [Space]
    [Header("Debug")]
    [Space]

    [SerializeField] private float coolDownCounter = 0;
    [SerializeField] private ProjectileSO selectedProjectile;
    [SerializeField] private int shootingLayerIndex = 0;

    [Header("How many different types of projectiles the player has access to.")]
    [SerializeField] private int projectileCount = 0;
    [SerializeField] private int currentIndex = 0;

    [Header("How many projectiles of the currently selected type have been instantiated and have not been destroyed.")]
    [SerializeField] private int activeProjectiles = 0;
    [SerializeField] private Logger logger;

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
        projectileCount = projectiles.Count;
        CalculateShootingLayerIndex();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CalculateShootingLayerIndex();
        // Debug.Log($"{name} ({GetType().Name}): shooting layer {shootingLayer}, value {shootingLayer.value}, shooting layer index {shootingLayerIndex}");
    }
#endif

    private void Start()
    {
        // Do this here so that anything listening for projectile change will be ready
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
                if (p.MaxConcurrentProjectiles == activeProjectiles)
                {
                    // The maximum number of projectiles are already in the field
                    // Destroy this projectile and exit early
                    Destroy(p.gameObject);
                    return;
                }

                coolDownCounter = p.CoolDownDuration;

                p.gameObject.SetActive(false);
                p.ClearVelocity();
                p.transform.position = s.position;
                p.gameObject.layer = shootingLayerIndex;

                if (p.TryGetComponent(out TrailMaker tm))
                {
                    tm.inheritedSpeed = p.Speed;
                    tm.inheritedDirection = s.direction;
                }

                if (p.TryGetComponent(out Destroyable d))
                {
                    d.onDestroyed.AddListener(OnProjectileDestroyed);
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

    public void AddProjectile(ProjectileSO so)
    {
        projectiles.Add(so);
        projectileCount = projectiles.Count;

        if (projectileCount == 1) SetProjectiles();
    }

    // ──────────────────────────────────────────────────────────────
    // Interface Implementation Methods
    // ──────────────────────────────────────────────────────────────

    // ActionableItemGroupsManager.IActionableItemGroup
    public void CycleToNextItem()
    {
        if (projectileCount == 0) return;
        currentIndex = (currentIndex + 1) % projectileCount;
        SetProjectiles();
    }

    // ActionableItemGroupsManager.IActionableItemGroup
    public void CycleToPreviousItem()
    {
        if (projectileCount == 0) return;
        currentIndex = (currentIndex - 1 + projectileCount) % projectileCount;
        SetProjectiles();
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private void SetProjectiles()
    {
        if (projectileCount == 0) return;

        selectedProjectile = projectiles[currentIndex];

        if (shootingLayer.value == 0)
        {
            logger.E("shooting layer not set!!");
            return;
        }

        onProjectileChanged?.Invoke(selectedProjectile);
    }

    private void CalculateShootingLayerIndex()
    {
        int layerValue = shootingLayer.value;
        shootingLayerIndex = 0;
        while (layerValue > 1)
        {
            layerValue /= 2;
            ++shootingLayerIndex;
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
