using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ProjectileManager))]
public class WallCannon : MonoBehaviour
{
    [SerializeField] private float shotSpacingSeconds = 5f;
    [SerializeField] private float initialShotDelay = 0f;
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool limitShots = false;
    [SerializeField][Range(1, 100)] private int maxShotCount = 1;
    [SerializeField] private CardinalDirection direction;
    [SerializeField] private UnityEvent onReady;

    private ProjectileManager projectileManager;
    private WaitForSeconds initialWait;
    private WaitForSeconds wait;
    private Vector2 projectileSpawnPosition;
    private Vector2 projectileSpawnDirection;
    private Coroutine coroutine;

    private bool IsReady => !limitShots || projectileManager.ActiveProjectiles < maxShotCount;

    private void Awake()
    {
        projectileManager = GetComponent<ProjectileManager>();
        UpdateValues();
    }

    private void Start()
    {
        if (autoStart) StartAutoShooting();
    }

    private void OnValidate() => UpdateValues();

    public void StopAutoShooting() => coroutine.WhenNotNullClass(c => StopCoroutine(c));

    public void StartAutoShooting() => coroutine.WhenNullClass(() => coroutine = StartCoroutine(AutoShoot()));

    public void ShootOnce(float delayDuration = 0f)
    {
        if (!IsReady) return;

        projectileManager.ShootDelayed(delayDuration, new ProjectileManager.StartingPointWithDirection()
        {
            direction = projectileSpawnDirection,
            position = projectileSpawnPosition
        });

        // If we've just become not ready, start waiting for projectiles to be destroyed
        if (!IsReady)
        {
            StartCoroutine(InvokeOnReadyWhenReady());
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

    private IEnumerator AutoShoot()
    {
        yield return initialWait;

        while (true)
        {
            ShootOnce();
            yield return wait;
        }
    }

    private IEnumerator InvokeOnReadyWhenReady()
    {
        yield return new WaitUntil(() => IsReady);
        onReady?.Invoke();
    }
}
