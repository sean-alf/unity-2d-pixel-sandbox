using System.Collections;
using UnityEngine;
using UnityEngine.U2D.Animation;
using StartingPoint = ProjectileManager.StartingPoint;

[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(SpriteResolver))]
[RequireComponent(typeof(SpriteRenderer))]
public class Globbel : MonoBehaviour
{
    private static readonly WaitForSeconds WAIT_FOR_HALF_SECOND = new(0.5f);
    private static readonly string CATEGORY = "Globbel";

    private readonly StartingPoint[] startingPoints = new StartingPoint[4];
    private readonly StartingPoint[] startingPointsAngled = new StartingPoint[4];

    private SpriteResolver sResolver;
    private SpriteRenderer sr;
    private ProjectileManager projectileManager;

    private int currentSpriteIndex = 0;
    private float positionOffset;
    private Coroutine coroutine = null;

    private void Awake()
    {
        sResolver = GetComponent<SpriteResolver>();
        sr = GetComponent<SpriteRenderer>();
        projectileManager = GetComponent<ProjectileManager>();
        projectileManager.SetShootingLayer(LayerNames.EnemyProjectile);

        positionOffset = sr.bounds.extents.x;

        for (int i = 0; i < 4; i++)
        {
            startingPoints[i] = CreateStartingPoint(i);
        }

        for (int i = 0; i < 4; i++)
        {
            startingPointsAngled[i] = CreateStartingPoint(i, Mathf.PI / 4);
        }
    }

    public void OnActivate()
    {
        coroutine = StartCoroutine(RotateAndShoot());
    }

    public void OnDeactivate()
    {
        if (coroutine == null) return;
        StopCoroutine(coroutine);
        coroutine = null;
    }

    public void ShootDefault()
    {
        projectileManager.Shoot(startingPoints);
    }

    public void ShootAngle()
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

    private IEnumerator RotateAndShoot()
    {
        while (true)
        {
            sResolver.SetCategoryAndLabel(CATEGORY, currentSpriteIndex.ToString());
            sResolver.ResolveSpriteToSpriteRenderer();

            yield return WAIT_FOR_HALF_SECOND;

            switch (currentSpriteIndex)
            {
                case 0:
                    ShootDefault();
                    break;
                case 1:
                    ShootAngle();
                    break;
            }

            yield return WAIT_FOR_HALF_SECOND;

            currentSpriteIndex = (currentSpriteIndex + 1) % 2;
        }
    }
}
