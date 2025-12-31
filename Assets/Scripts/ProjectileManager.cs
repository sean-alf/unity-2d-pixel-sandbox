using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField]
    private GameObject projectileTemplate;

    private readonly List<BasicProjectile> projectiles = new();
    private readonly int maxProjectiles = 5;

    private void Awake()
    {
        for (int i = 0; i < maxProjectiles; i++)
        {
            if (Instantiate(projectileTemplate, Vector3.zero, Quaternion.identity).TryGetComponent(out BasicProjectile p))
            {
                p.gameObject.SetActive(false);
                projectiles.Add(p);
            }
            else
            {
                Debug.LogError("ProjectileManager: invalid projectileTemplate, no BasicProjectile attached!!!");
            }
        }
    }

    public void Shoot(Vector2 direction, Transform transform)
    {
        var p = projectiles.Find((p) => !p.InUse);

        if (p != null)
        {
            p.Use(direction, transform);
        }
        else
        {
            Debug.LogWarning("all projectiles in use!!");
        }
    }
}
