using System;
using System.Collections.ObjectModel;
using UnityEngine;

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

    [SerializeField]
    private float speed = 1;

    private Rigidbody2D rb;
    private new Collider2D collider;
    private LinearAnimator animator;
    private ProjectileManager projectileManager;
    private Vector2 currentDirection;
    private Vector2 lookDirection = Vector2.up;

    private Vector2 raycastPosition;
    private Vector2 raycastDirection;
    private float raycastDistance;

    [SerializeField]
    private CardinalDirections initialDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<LinearAnimator>();
        projectileManager = GetComponent<ProjectileManager>();

        projectileManager.SetShootingLayer(LayerNames.EnemyProjectile);

        ChangeDirection(initialDirection.ToVector2());
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
        if (currentDirection == Vector2.zero)
        {
            animator.Stop();
        }
        else
        {
            if (IsPathBlocked(currentDirection, FORWARD_PATH_DISTANCE))
            {
                ChangeDirection(NewDirection());
            }
            animator.Animate("Default");
            rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * currentDirection);
        }
    }

    // IEnumerator MoveThenShoot(bool startWithRandomDir = true)
    // {
    //     if (startWithRandomDir) ChangeDirection(NewDirection());

    //     yield return new WaitForSeconds(3 + (2 * Random.value));

    //     currentDirection = Vector2.zero;

    //     yield return new WaitForSeconds(0.25f + (0.25f * Random.value));

    //     for (int i = 0; i < Random.Range(1, 3); i++)
    //     {
    //         projectileManager.Shoot(new ProjectileManager.StartingPointWithDirection()
    //         {
    //             direction = lookDirection,
    //             position = transform.position
    //         });
    //         yield return _waitForSeconds0_5;
    //     }

    //     yield return _waitForSeconds0_5;
    //     yield return StartCoroutine(MoveThenShoot());
    // }

    private void ChangeDirection(Vector2 direction)
    {
        currentDirection = direction;
        if (currentDirection != Vector2.zero) lookDirection = currentDirection;
        rb.SetRotation(Quaternion.LookRotation(Vector3.forward, currentDirection));
    }

    private Vector2 NewDirection()
    {
        var randomSign = CollectionsExtensions.SelectRandom(signs);
        var perp = randomSign * Vector2.Perpendicular(currentDirection);
        var oppositePerp = -perp;
        var retreat = -currentDirection;

        // First check the randomly selected perpendicular direction
        // Return it if it's not blocked
        if (!IsPathBlocked(perp, PERIPHERAL_PATH_DISTANCE)) return perp;
        // Next, check the opposite perpendicular direction
        // Return it if it's not blocked
        if (!IsPathBlocked(oppositePerp, PERIPHERAL_PATH_DISTANCE)) return oppositePerp;
        // If neither of the perpendicular directions are open, then check to the rear
        // Return it if it's not blocked
        if (!IsPathBlocked(-currentDirection, FORWARD_PATH_DISTANCE)) return retreat;
        // At this point, if the forward direction is not blocked, return it
        if (!IsPathBlocked(currentDirection, FORWARD_PATH_DISTANCE)) return currentDirection;
        // Otherwise, just stop
        return Vector2.zero;
    }

    private bool IsPathBlocked(Vector2 direction, float distance)
    {
        raycastDirection = direction;
        raycastPosition = transform.position.Add(collider.bounds.extents * raycastDirection);
        raycastDistance = distance;

        var hits = Physics2D.RaycastAll(raycastPosition, raycastDirection, raycastDistance, Physics2D.GetLayerCollisionMask(gameObject.layer));
        foreach (var hit in hits)
        {
            if (hit.collider.name == gameObject.name) continue;
            // Debug.Log($"StraightShooter: Raycast hit {hit.collider.name}");
        }

        return gameObject.HasHits(hits);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(raycastPosition, raycastPosition + (raycastDirection * raycastDistance));
    }
}
