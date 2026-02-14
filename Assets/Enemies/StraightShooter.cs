using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(KnockbackReceiver))]
public class StraightShooter : MonoBehaviour
{
    private static readonly float FORWARD_PATH_DISTANCE = 1f / 16f;
    private static readonly float PERIPHERAL_PATH_DISTANCE = 1f / 4f;
    private static readonly ReadOnlyCollection<int> signs = Array.AsReadOnly(new[] { -1, 1 });
    private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);

    [SerializeField] private float speed = 1;
    [SerializeField] private float mainCastDistance = 10;
    [SerializeField] private float reverseCastDistance = 4;
    [SerializeField] private LayerMask castLayerMask;

    [Space]
    [Header("Debug")]
    [SerializeField] private float speedFactor = 1f;
    [SerializeField] private bool autoTurnCorner = false;
    [SerializeField] private bool isShootingAtPlayer = false;
    [SerializeField] private bool isGettingKnockedBack = false;
    [SerializeField] private LayerMask navigationLayerMask;
    [SerializeField] private Vector2 currentDirection;

    private Rigidbody2D rb;
    private new Collider2D collider;
    private LinearAnimator animator;
    private ProjectileManager projectileManager;
    private KnockbackReceiver knockbackReceiver;
    private Vector2 hitPosition;
    private Coroutine autoTurnCoroutine;
    private Coroutine autoShootCoroutine;

    [SerializeField]
    private CardinalDirection initialDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<LinearAnimator>();
        projectileManager = GetComponent<ProjectileManager>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();

        projectileManager.SetShootingLayer(LayerNames.EnemyProjectile);
        navigationLayerMask = Physics2D.GetLayerCollisionMask(gameObject.layer)
            & ~((1 << LayerMask.NameToLayer(LayerNames.Player)) | (1 << LayerMask.NameToLayer(LayerNames.Projectile)));
    }

    private void Start()
    {
        StartTimers();
        ChangeDirection(initialDirection.ToVector2());
    }

    private void OnValidate()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, initialDirection.ToVector2());
    }

    private void FixedUpdate()
    {
        if (!isGettingKnockedBack)
        {
            CheckForPlayerInSightOrBehind();
            HandlePathNavigation();
        }

        float scaledSpeed = speed * speedFactor;
        var scaledVelocity = scaledSpeed * currentDirection;
        rb.linearVelocity = knockbackReceiver.KnockbackVelocity + scaledVelocity;
    }

    private void CheckForPlayerInSightOrBehind()
    {
        if (isShootingAtPlayer) return;

        hitPosition = Vector2.zero;

        var hit = Physics2D.Raycast(transform.position, currentDirection, mainCastDistance, castLayerMask);
        var isPlayerInLineOfSight = hit.collider && hit.collider.gameObject.IsOnLayer(LayerNames.Player);

        // Capture position for Gizmos
        if (hit.collider)
        {
            hitPosition = hit.collider.transform.position;
        }

        if (isPlayerInLineOfSight)
        {
            isShootingAtPlayer = true;

            var savedDirection = currentDirection;

            StopTimers();
            Stop();
            StartCoroutine(SequencingUtilities.Delay(0.25f, onRun: () =>
            {
                Shoot();
                StartCoroutine(SequencingUtilities.Delay(0.25f, onRun: () =>
                {
                    isShootingAtPlayer = false;
                    StartTimers();
                    ChangeDirection(savedDirection);
                }));
            }));
        }
        else
        {
            CheckForPlayerCloseBehind();
        }
    }

    private void HandlePathNavigation()
    {
        if (isShootingAtPlayer) return;

        // Now check if self is blocked and needs to change directions
        if (IsPathBlocked(currentDirection, FORWARD_PATH_DISTANCE))
        {
            ChangeDirection(NewDirection());
        }
        else if (autoTurnCorner && LookForNextOpenCorner(out var newDirection))
        {
            autoTurnCorner = false;
            ChangeDirection(newDirection);
        }
    }

    public void EnemyDamageHandler_OnKnockbackStart()
    {
        isGettingKnockedBack = true;

        StopAllCoroutines();
        animator.Stop();
        isShootingAtPlayer = false;
        speedFactor = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void EnemyDamageHandler_OnKnockbackEnd()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        StartTimers();
        speedFactor = 1f;
        isGettingKnockedBack = false;
    }

    public void EnemyDamageHandler_OnDeathPreAnimate() => OnPreDeath();

    private void CheckForPlayerCloseBehind()
    {
        var reverse = -currentDirection;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, reverse, reverseCastDistance, castLayerMask);

        if (hit.collider && hit.collider.gameObject.IsOnLayer(LayerNames.Player))
        {
            StopTimers();
            ChangeDirection(reverse);
            StartTimers();
            return;
        }
    }

    private void ChangeDirection(Vector2 direction)
    {
        currentDirection = direction;

        if (currentDirection.IsIdle())
        {
            animator.Stop();
        }
        else
        {
            rb.SetRotation(Quaternion.LookRotation(Vector3.forward, currentDirection));
            animator.Animate("Default");
        }
    }

    private void StopTimers()
    {
        if (autoShootCoroutine != null)
        {
            StopCoroutine(autoShootCoroutine);
            autoShootCoroutine = null;
        }
        if (autoTurnCoroutine != null)
        {
            StopCoroutine(autoTurnCoroutine);
            autoTurnCoroutine = null;
        }
    }

    private void StartTimers()
    {
        autoShootCoroutine ??= StartCoroutine(ShootTimer());
        autoTurnCoroutine ??= StartCoroutine(AutoTurnCornersTimer());
    }

    private Vector2 NewDirection()
    {
        var randomSign = CollectionsExtensions.SelectRandom(signs);
        var perp = randomSign * Vector2.Perpendicular(currentDirection);
        var oppositePerp = -perp;
        var retreat = -currentDirection;

        // First check the randomly selected perpendicular direction
        // Return it if it's not blocked
        if (IsPathOpen(perp, PERIPHERAL_PATH_DISTANCE)) return perp;
        // Next, check the opposite perpendicular direction
        // Return it if it's not blocked
        if (IsPathOpen(oppositePerp, PERIPHERAL_PATH_DISTANCE)) return oppositePerp;
        // If neither of the perpendicular directions are open, then check to the rear
        // Return it if it's not blocked
        if (IsPathOpen(-currentDirection, FORWARD_PATH_DISTANCE)) return retreat;
        // At this point, if the forward direction is not blocked, return it
        if (IsPathOpen(currentDirection, FORWARD_PATH_DISTANCE)) return currentDirection;
        // Otherwise, just stop
        return Vector2.zero;
    }

    private bool LookForNextOpenCorner(out Vector2 newDirection)
    {
        var randomSign = CollectionsExtensions.SelectRandom(signs);
        var perp = randomSign * Vector2.Perpendicular(currentDirection);
        var oppositePerp = -perp;

        newDirection = perp;

        if (IsPathOpen(newDirection, PERIPHERAL_PATH_DISTANCE)) return true;

        newDirection = oppositePerp;

        if (IsPathOpen(newDirection, PERIPHERAL_PATH_DISTANCE)) return true;

        return false;
    }

    private bool IsPathOpen(Vector2 direction, float distance) => !IsPathBlocked(direction, distance);

    private bool IsPathBlocked(Vector2 direction, float distance)
    {
        var raycastPosition = transform.position.Add(collider.bounds.extents * direction);
        Vector2 size = new(collider.bounds.size.x - 1f / 16f, distance);
        var angle = Vector2.SignedAngle(Vector2.up, direction);
        var hits = Physics2D.BoxCastAll(raycastPosition, size, angle, direction, 0, navigationLayerMask);
        return gameObject.HasHits(hits);
    }

    private void Stop() => ChangeDirection(Vector2.zero);

    private void OnPreDeath()
    {
        Stop();
        StopAllCoroutines();
    }

    private void Shoot()
    {
        projectileManager.Shoot(new ProjectileManager.StartingPointWithDirection()
        {
            direction = transform.rotation * Vector2.up,
            position = transform.position
        });
    }

    IEnumerator AutoTurnCornersTimer()
    {
        while (true)
        {
            yield return new WaitWhile(() => autoTurnCorner);
            yield return new WaitForSeconds(Random.Range(5, 10));
            autoTurnCorner = true;
        }
    }

    IEnumerator ShootTimer()
    {
        Vector2 savedDirection;

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2, 5));

            // Halt movement
            savedDirection = currentDirection;
            Stop();

            yield return new WaitForSeconds(0.25f + (0.25f * Random.value));

            // Shoot 1 to 3 times
            for (int i = 0; i < Random.Range(1, 3); i++)
            {
                Shoot();
                yield return _waitForSeconds0_5;
            }

            yield return _waitForSeconds0_5;

            // Resume movement
            ChangeDirection(savedDirection);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellowGreen;
        Gizmos.DrawRay(transform.position, mainCastDistance * currentDirection);

        Gizmos.color = Color.chartreuse;
        Gizmos.DrawRay(transform.position, reverseCastDistance * -currentDirection);

        if (hitPosition != Vector2.zero)
        {
            Gizmos.color = Color.violetRed;
            Gizmos.DrawWireSphere(hitPosition, 1f);
        }
    }
}
