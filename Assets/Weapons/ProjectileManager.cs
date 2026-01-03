using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour, ILogTagProvider
{
    public struct StartingPoint
    {
        public Vector2 direction;
        public Vector3 position;
    }

    [SerializeField]
    private List<ProjectileSO> projectiles;

    [SerializeField]
    private MenusAndDisplayManager madm;

    private ProjectileSO selectedProjectile;
    private int currentIndex = 0;
    private int count;
    private int shootingLayer;
    private Logging.Tag logTag;

    public Logging.Tag LogTag => throw new System.NotImplementedException();

    private void OnEnable()
    {
        logTag = this.CreateLogTag();
    }

    private void Awake()
    {
        if (projectiles == null || projectiles.Count == 0)
        {
            Debug.LogError("Projectile Manager: Projectiles not set!!");
            return;
        }

        count = projectiles.Count;

        // Automatically set the selected projectile to the first one in the list
        selectedProjectile = projectiles[currentIndex];
    }

    private void Start()
    {
        SetProjectiles();
    }

    public void Shoot(params StartingPoint[] startingPoints)
    {
        foreach (var s in startingPoints)
        {
            selectedProjectile.Instantiate(p =>
            {
                p.gameObject.layer = shootingLayer;
                p.Use(s.direction, s.position);
            });
        }
        ;
    }

    public void SelectNext()
    {
        currentIndex = (currentIndex + 1) % count;
        selectedProjectile = projectiles[currentIndex];
        SetProjectiles();
    }

    public void SelectPrevious()
    {
        currentIndex = (currentIndex - 1 + count) % count;
        selectedProjectile = projectiles[currentIndex];
        SetProjectiles();
    }

    private void SetProjectiles()
    {
        if (shootingLayer == 0)
        {
            Logging.LogError(logTag, "shooting layer not set!!");
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
}
