using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D.Animation;
using StartingPoint = ProjectileManager.StartingPointWithDirection;

[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(SpriteResolver))]
[RequireComponent(typeof(SpriteRenderer))]
public class Globbel : MonoBehaviour
{
    private static readonly int SIMULTANEOUS_SHOTS_COUNT = 4;
    private static readonly WaitForSeconds WAIT_FOR_HALF_SECOND = new(0.5f);

    private readonly StartingPoint[] startingPoints = Enumerable.Repeat(new StartingPoint(), SIMULTANEOUS_SHOTS_COUNT).ToArray();

    private SpriteRenderer sr;
    private ProjectileManager projectileManager;

    private float positionOffset;
    private Coroutine coroutine = null;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        projectileManager = GetComponent<ProjectileManager>();
        projectileManager.SetShootingLayer(LayerNames.EnemyProjectile);

        positionOffset = sr.bounds.extents.x;
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

    private void UpdateStartingPoints(float angleOffsetRads = 0)
    {
        for (int i = 0; i < SIMULTANEOUS_SHOTS_COUNT; i++)
        {
            float angleRads = (i * Mathf.PI / 2) + angleOffsetRads;
            Vector2 direction = new(Mathf.Cos(angleRads), Mathf.Sin(angleRads));
            startingPoints[i].direction = direction;
            startingPoints[i].position = transform.position.Add(positionOffset * direction.normalized);
        }
    }

    private IEnumerator RotateAndShoot()
    {
        while (true)
        {
            transform.Rotate(0, 0, 45.0f);
            UpdateStartingPoints(transform.eulerAngles.z * Mathf.Deg2Rad);

            yield return WAIT_FOR_HALF_SECOND;

            projectileManager.Shoot(startingPoints);

            yield return WAIT_FOR_HALF_SECOND;
        }
    }
}
