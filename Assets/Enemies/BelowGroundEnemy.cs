using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(EnemyDamageHandler))]
[RequireComponent(typeof(DirectionWatcher))]
[RequireComponent(typeof(DistanceWatcher))]
public class BelowGroundEnemy : MonoBehaviour
{
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite preparingToShootSprite;
    [SerializeField] private Sprite shootingSprite;
    [SerializeField] private float preparingToShootDuration = 0.25f;
    [SerializeField] private float afterShootDuration = 0.25f;
    [SerializeField] private float betweenShotsDelay = 2f;
    [SerializeField] private float preparingToShootRecoilDistance = 0.125f;
    [SerializeField] private Transform centerProjectileSpawnPoint;
    [SerializeField] private Transform negativeAngledProjectileSpawnPoint;
    [SerializeField] private Transform positiveAngledProjectileSpawnPoint;
    [SerializeField] private GameObject submergeEffectTemplate;

    [Space]
    [Header("Debug")]

    [SerializeField] private bool lockRotation;
    [SerializeField] private float rotation;
    [SerializeField] private bool isPlayerSighted = false;
    [SerializeField] private bool isSubmerged = true;
    [SerializeField] private LinearAnimator submergeEffectAnimator;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private new Collider2D collider;
    private LinearAnimator animator;
    private ProjectileManager projectileManager;
    private EnemyDamageHandler enemyDamageHandler;
    private DistanceWatcher distanceWatcher;
    private DirectionWatcher directionWatcher;

    // ──────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<LinearAnimator>();
        projectileManager = GetComponent<ProjectileManager>();
        enemyDamageHandler = GetComponent<EnemyDamageHandler>();
        distanceWatcher = GetComponent<DistanceWatcher>();
        directionWatcher = GetComponent<DirectionWatcher>();

        sr.sprite = null;
        collider.enabled = false;
        isSubmerged = true;

        var playerTransform = GameObject.Find("Player").transform;
        distanceWatcher.target = playerTransform;
        directionWatcher.target = playerTransform;

        submergeEffectAnimator = Instantiate(submergeEffectTemplate, transform.position, Quaternion.identity).GetComponent<LinearAnimator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Repeat());
    }

    // ──────────────────────────────────────────────────────────────
    // Public Control Methods
    // ──────────────────────────────────────────────────────────────

    public void UpdateTargetDirection(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(Vector2.down, direction);
        rotation = Mathf.Round(angle / 45f) * 45f;
        if (!lockRotation) rb.rotation = rotation;
    }

    public void SetPlayerIsSighted() => isPlayerSighted = true;

    public void SetPlayerIsNotSighted() => isPlayerSighted = false;

    // ──────────────────────────────────────────────────────────────
    // Control Methods
    // ──────────────────────────────────────────────────────────────

    private void Emerge()
    {
        lockRotation = true;
        isSubmerged = false; // This must be set here for the sake of the Repeat coroutine

        submergeEffectAnimator.Animate("Ripple", onFinished: () =>
        {
            submergeEffectAnimator.Animate("Splash");
            animator.AnimateReverse("Submerge", onFinished: () =>
            {
                collider.enabled = true;
                lockRotation = false;

                animator.Animate("Look", onFinished: () =>
                {
                    if (isPlayerSighted)
                    {
                        Shoot();
                    }
                    else
                    {
                        Submerge();
                    }
                });
            });
        });
    }

    private void Submerge()
    {
        collider.enabled = false;
        lockRotation = true;

        animator.Animate("Submerge", onFinished: () =>
        {
            sr.sprite = null;
            lockRotation = false;
            rb.rotation = rotation;
            isSubmerged = true;

            submergeEffectAnimator.Animate("Default");
        });
    }

    private void Shoot() => StartCoroutine(ShootCoroutine());

    // ──────────────────────────────────────────────────────────────
    // Coroutines
    // ──────────────────────────────────────────────────────────────

    private IEnumerator Repeat()
    {
        while (true)
        {
            if (isPlayerSighted)
            {
                Emerge();
                yield return new WaitUntil(() => isSubmerged);
                yield return SequencingUtilities.WaitForSecondsWhile(betweenShotsDelay, () => isPlayerSighted);
            }
            yield return new WaitUntil(() => isSubmerged);
            yield return new WaitUntil(() => isPlayerSighted);
        }
    }

    private IEnumerator ShootCoroutine()
    {
        lockRotation = true;

        var originalPosition = rb.position;

        sr.sprite = preparingToShootSprite;
        rb.position += (Vector2)transform.up * preparingToShootRecoilDistance;
        yield return new WaitForSeconds(preparingToShootDuration);
        sr.sprite = shootingSprite;
        rb.position = originalPosition;
        projectileManager.Shoot(new ProjectileManager.StartingPointWithDirection[]
        {
            new()
            {
                position = centerProjectileSpawnPoint.position,
                direction = -transform.up,
            },
            new()
            {
                position = positiveAngledProjectileSpawnPoint.position,
                direction = Quaternion.AngleAxis(45, Vector3.forward) * -transform.up,
            },
            new()
            {
                position = negativeAngledProjectileSpawnPoint.position,
                direction = Quaternion.AngleAxis(-45, Vector3.forward) * -transform.up,
            },
        });
        yield return new WaitForSeconds(afterShootDuration);
        sr.sprite = defaultSprite;
        yield return new WaitForSeconds(afterShootDuration);
        yield return new WaitWhile(() => enemyDamageHandler.IsInvincible);
        yield return new WaitWhile(() => enemyDamageHandler.IsDead);
        Submerge();
    }
}
