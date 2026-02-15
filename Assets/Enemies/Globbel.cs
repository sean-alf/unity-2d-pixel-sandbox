using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D.Animation;
using StartingPoint = ProjectileManager.StartingPointWithDirection;

[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(SpriteResolver))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(DistanceWatcher))]
public class Globbel : MonoBehaviour
{
    private static readonly int SIMULTANEOUS_SHOTS_COUNT = 4;

    private readonly StartingPoint[] startingPoints = Enumerable.Repeat(new StartingPoint(), SIMULTANEOUS_SHOTS_COUNT).ToArray();

    [SerializeField]
    private float rotationAngleStep = 45.0f;

    [SerializeField]
    private bool reverseShotRotation = false;

    [SerializeField]
    private float stepDurationSeconds = 2;

    [SerializeField]
    private float delayBetweenShotsSeconds = 0.0f;

    private SpriteRenderer sr;
    private ProjectileManager projectileManager;
    private WaitForSeconds stepWait;
    private WaitForSeconds shotWait;
    private float positionOffset;
    private Coroutine coroutine = null;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        projectileManager = GetComponent<ProjectileManager>();

        projectileManager.SetShootingLayer(LayerNames.EnemyProjectile);
        positionOffset = sr.bounds.extents.x;
        UpdateWaitDurations();
    }

    public void DistanceWatcher_OnActivate() => Activate();

    public void DistanceWatcher_OnDeactivate() => Deactivate();

    public void EnemyDamageHandler_OnDeathPreAnimate() => Deactivate();

    private void Activate()
    {
        Deactivate();
        coroutine = StartCoroutine(RotateAndShoot());
    }

    private void Deactivate()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
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
            transform.Rotate(0, 0, rotationAngleStep);
            UpdateStartingPoints(transform.eulerAngles.z * Mathf.Deg2Rad);

            yield return stepWait;

            if (Mathf.Approximately(delayBetweenShotsSeconds, 0.00f))
            {
                // Shoot simmultaneously
                projectileManager.Shoot(startingPoints);
            }
            else
            {
                if (reverseShotRotation)
                {
                    for (int i = startingPoints.Count() - 1; i >= 0; i--)
                    {
                        projectileManager.Shoot(startingPoints[i]);
                        yield return shotWait;
                    }
                }
                else
                {
                    for (int i = 0; i < startingPoints.Count(); i++)
                    {
                        projectileManager.Shoot(startingPoints[i]);
                        yield return shotWait;
                    }
                }
            }

            yield return stepWait;
        }
    }

    private void OnValidate()
    {
        UpdateWaitDurations();
    }

    private void UpdateWaitDurations()
    {
        stepWait = new(stepDurationSeconds / 2);
        shotWait = new(delayBetweenShotsSeconds);
    }
}
