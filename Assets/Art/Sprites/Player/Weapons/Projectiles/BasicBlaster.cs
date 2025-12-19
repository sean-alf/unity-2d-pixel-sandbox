using System.Collections.Generic;
using UnityEngine;

public class BasicBlaster : Weapon
{
    public interface IBlasterAmmo
    {
        public int SpawnPositionOffset { get; }
    }

    private static readonly ILogger logger = Debug.unityLogger;

    public GameObject blasterPelletPrefab;
    public int concurrentBallisticCount;
    public float speed = 200;

    private List<BlasterPellet> recycleablePellets;
    private PlayerMovement movement;

    public override void Attack()
    {
        gameObject.SetActive(true);

        var p = recycleablePellets.Find(p => !p.InUse);

        if (p)
        {
            logger.Log(name, "Fire the pellet!");
            var d = movement.LastNonIdleDirection.ToSingleAxis(true);
            Vector2 offset = CalculateOffset(d, p);
            p.Use(transform.position.Add(offset), d, speed);
        }
        else
        {
            logger.Log(name, "no more pellets available");
        }
    }

    private Vector2 CalculateOffset(Vector2 direction, IBlasterAmmo ammo)
    {
        if (direction.IsLeft())
        {
            return new(-ammo.SpawnPositionOffset, 0);
        }
        else if (direction.IsRight())
        {
            return new(ammo.SpawnPositionOffset, 0);
        }
        else if (direction.IsUp())
        {
            return new(0, ammo.SpawnPositionOffset);
        }
        else if (direction.IsDown())
        {
            return new(0, -ammo.SpawnPositionOffset);
        }
        else
        {
            return new();
        }
    }

    public override void AttackEnd()
    {
        gameObject.SetActive(false);
    }

    void Awake()
    {
        if (blasterPelletPrefab == null)
        {
            logger.LogError(name, "blaster pellet prefab null!");
        }

        recycleablePellets ??= new(concurrentBallisticCount);

        if (movement == null)
        {
            movement = GetComponentInParent<PlayerMovement>();

            if (movement == null)
            {
                logger.LogError(name, "missing PlayerMovement in parent");
            }
        }
    }

    void Start()
    {
        for (int i = 0; i < recycleablePellets.Capacity; i++)
        {
            recycleablePellets.Add(Instantiate(blasterPelletPrefab.GetComponent<BlasterPellet>()));
        }

        gameObject.SetActive(false);
    }
}
