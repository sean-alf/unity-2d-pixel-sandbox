using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

using StartingPoint = ProjectileManager.StartingPoint;

[RequireComponent(typeof(ProjectileManager))]
public class EnemyShooter : MonoBehaviour
{
    private ProjectileManager projectileManager;
    private readonly StartingPoint[] startingPoints = new StartingPoint[4];
    private readonly StartingPoint[] startingPointsAngled = new StartingPoint[4];

    // TODO: this needs to be dynamic based on the sprite size and any other offset
    private readonly float positionOffset = 16;

    private void Awake()
    {
        projectileManager = GetComponent<ProjectileManager>();
        projectileManager.SetShootingLayer(LayerNames.EnemyProjectile);

        for (int i = 0; i < 4; i++)
        {
            startingPoints[i] = CreateStartingPoint(i);
        }

        for (int i = 0; i < 4; i++)
        {
            startingPointsAngled[i] = CreateStartingPoint(i, Mathf.PI / 4);
        }
    }

    public void Animator_ShootDefault()
    {
        projectileManager.Shoot(startingPoints);
    }

    public void Animator_ShootAngle()
    {
        projectileManager.Shoot(startingPointsAngled);
    }

    private StartingPoint CreateStartingPoint(int factor, float angleOffsetRads = 0)
    {
        float angleRads = (factor * Mathf.PI / 2) + angleOffsetRads;
        Vector2 direction = new(Mathf.Cos(angleRads), Mathf.Sin(angleRads));

        return new StartingPoint
        {
            direction = direction,
            position = transform.position.Add(positionOffset * direction.normalized),
        };
    }
}
