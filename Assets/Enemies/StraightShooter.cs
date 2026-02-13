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

    private Rigidbody2D rb;
    private new Collider2D collider;
    private LinearAnimator animator;
    private ProjectileManager projectileManager;
    private Vector2 currentDirection;
    private Vector2 hitPosition;
    private int navigationLayerMask;
    private Coroutine autoTurnCoroutine;
    private Coroutine autoShootCoroutine;
    private bool autoTurnCorner = false;
    private bool isShootingAtPlayer = false;

    [SerializeField]
    private CardinalDirection initialDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<LinearAnimator>();
        projectileManager = GetComponent<ProjectileManager>();

        projectileManager.SetShootingLayer(LayerNames.EnemyProjectile);
        navigationLayerMask = Physics2D.GetLayerCollisionMask(gameObject.layer)
            & ~((1 << LayerMask.NameToLayer(LayerNames.Player)) | (1 << LayerMask.NameToLayer(LayerNames.Projectile)));

        ChangeDirection(initialDirection.ToVector2());
    }

    private void Start()
    {
        StartTimers();
    }

    private void OnValidate()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, initialDirection.ToVector2());
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (isShootingAtPlayer) return;

        if (currentDirection == Vector2.zero)
        {
            animator.Stop();
        }
        else
        {
            hitPosition = Vector2.zero;

            // Check if player is in line-of-sight
            // Shoot if true
            var hit = Physics2D.Raycast(transform.position, currentDirection, mainCastDistance, castLayerMask);
            if (hit.collider)
            {
                hitPosition = hit.collider.transform.position;
            }

            if (hit.collider && hit.collider.gameObject.IsOnLayer(LayerNames.Player))
            {
                isShootingAtPlayer = true;
                StopTimers();
                StartCoroutine(SequencingUtilities.Delay(0.25f, onRun: () =>
                {
                    Shoot();
                    StartCoroutine(SequencingUtilities.Delay(0.25f, onRun: () =>
                    {
                        isShootingAtPlayer = false;
                        StartTimers();
                    }));
                }));
            }
            else
            {
                CheckForPlayerBehind();
            }

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

            animator.Animate("Default");
            rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * currentDirection);
        }
    }

    public void EnemyDamageHandler_OnDeathPreAnimate() => OnPreDeath();

    private void CheckForPlayerBehind()
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
        rb.SetRotation(Quaternion.LookRotation(Vector3.forward, currentDirection));
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

    private void Stop() => currentDirection = Vector2.zero;

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
            currentDirection = savedDirection;
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
