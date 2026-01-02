using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(IDirectionProvider))]
public class ProjectileManager : MonoBehaviour
{
    public interface IDirectionProvider
    {
        public Vector2 LookDirection { get; }
    }

    [SerializeField]
    private List<ProjectileSO> projectiles;

    [SerializeField]
    private MenusAndDisplayManager madm;

    private readonly List<BasicProjectile> projectilesList = new();

    private IDirectionProvider directionProvider;
    private ProjectileSO selectedProjectile;
    private int currentIndex = 0;
    private int count;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();

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

    public void Shoot()
    {
        var p = projectilesList.Find((p) => !p.InUse);

        if (p != null)
        {
            p.Use(directionProvider.LookDirection, transform);
        }
        else
        {
            Debug.LogWarning("all projectiles in use!!");
        }
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
            selectedProjectile.Instantiate(p => projectilesList.Add(p));
        }

        madm.UpdatePrimaryWeaponIcon(selectedProjectile.MenuIcon);
    }
}
