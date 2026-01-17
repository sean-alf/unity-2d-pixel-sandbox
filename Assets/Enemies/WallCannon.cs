using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ProjectileManager))]
public class WallCannon : MonoBehaviour
{
    [SerializeField]
    private float shotSpacingSeconds = 5f;

    [SerializeField]
    private float initialShotDelay = 0f;

    [SerializeField]
    private bool autoStart = true;

    [SerializeField]
    private CardinalDirection direction;

    private ProjectileManager projectileManager;
    private WaitForSeconds initialWait;
    private WaitForSeconds wait;
    private Vector2 projectileSpawnPosition;
    private Vector2 projectileSpawnDirection;
    private Coroutine coroutine;

    private void Awake()
    {
        projectileManager = GetComponent<ProjectileManager>();

        projectileManager.SetShootingLayer(LayerNames.EnemyProjectile);

        UpdateValues();
    }

    private void Start()
    {
        if (autoStart) StartAutoShooting();
    }

    private void OnValidate() => UpdateValues();

    public void StopAutoShooting() => coroutine.WhenNotNullClass(c => StopCoroutine(c));

    public void StartAutoShooting() => coroutine.WhenNullClass(() => coroutine = StartCoroutine(AutoShoot()));

    public void ShootOnce() => projectileManager.ShootPreserveRotation(new ProjectileManager.StartingPointWithDirection()
    {
        direction = projectileSpawnDirection,
        position = projectileSpawnPosition
    });

    private IEnumerator AutoShoot()
    {
        yield return initialWait;

        while (true)
        {
            ShootOnce();
            yield return wait;
        }
    }

    private void UpdateValues()
    {
        initialWait = new(initialShotDelay);
        wait = new(shotSpacingSeconds);
        transform.rotation = direction.ToRotation();
        projectileSpawnDirection = transform.rotation * Vector2.up;
        projectileSpawnPosition = transform.GetChild(0).transform.position;
    }
}
