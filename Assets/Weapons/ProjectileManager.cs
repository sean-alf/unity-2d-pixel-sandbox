using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
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

    private readonly List<BasicProjectile> projectilesList = new();

    private ProjectileSO selectedProjectile;
    private int currentIndex = 0;
    private int count;
    private int shootingLayer;

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

        shootingLayer = gameObject.layer;
    }

    private void Start()
    {
        SetProjectiles();
    }

    public void Shoot(params StartingPoint[] startingPoints)
    {
        foreach (var s in startingPoints)
        {
            var p = projectilesList.Find((p) => !p.InUse);

            if (p != null)
            {
                p.Use(s.direction, s.position);
            }
            else
            {
                Debug.LogWarning("all projectiles in use!!");
            }
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
        foreach (var p in projectilesList)
        {
            if (p != null)
            {
                Destroy(p.gameObject);
            }
        }

        projectilesList.Clear();

        for (int i = 0; i < selectedProjectile.MaxProjectiles; i++)
        {
            selectedProjectile.Instantiate(p =>
            {
                p.gameObject.layer = shootingLayer;
                projectilesList.Add(p);
            });
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
