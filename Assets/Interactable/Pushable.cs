using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Pushable : MonoBehaviour
{
    [SerializeField] private bool isPushable = true;
    [SerializeField] private float pushSpeed = 1f;
    [SerializeField][Range(0, 50)][Tooltip("0 is considered infinite")] private int maxPushCount = 1;
    [SerializeField]
    private CardinalDirection[] allowedPushingDirections =
    {
        CardinalDirection.Up,
        CardinalDirection.Right,
        CardinalDirection.Down,
        CardinalDirection.Left
    };

    [Space]
    [Header("Debug")]

    [SerializeField] private Vector2 initialPosition;
    [SerializeField] private Vector2 nextPosition;
    [SerializeField] private int currentPushCount = 0;
    [SerializeField] private Vector2 direction;
    [SerializeField] private bool isMoving;

    private Rigidbody2D rb;
    private new Collider2D collider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();

        rb.position = rb.position.SnapToHalf();
        nextPosition = rb.position;
        initialPosition = rb.position;
    }

    private void FixedUpdate()
    {
        if (!isPushable) return;

        if (Vector2.Distance(initialPosition, nextPosition) > 0.001f)
        {
            var direction = (nextPosition - rb.position).normalized;
            rb.MovePosition(rb.position + pushSpeed * Time.fixedDeltaTime * direction);

            if (Vector2.Distance(rb.position, nextPosition) <= 0.04f)
            {
                // Snap to the position
                rb.MovePosition(nextPosition.SnapToHalf());
                initialPosition = nextPosition;
                ++currentPushCount;
                isMoving = false;

                if (currentPushCount >= maxPushCount && maxPushCount > 0)
                {
                    isPushable = false;
                    enabled = false;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (!isPushable || isMoving) return;

        if (other.gameObject.TryGetComponent(out PlayerController _))
        {
            direction = other.relativeVelocity.normalized;

            if (!direction.IsCardinal() || allowedPushingDirections.Count(d => d.ToVector2() == direction) == 0) return;

            DetermineNextPosition(direction);

            if (nextPosition != initialPosition)
            {
                isMoving = true;
            }
        }
    }

    private void DetermineNextPosition(Vector2 direction)
    {
        if (direction.IsCardinal())
        {
            var targetPosition = (rb.position + direction).SnapToHalf();

            if (IsPathClear(targetPosition, direction))
            {
                nextPosition = targetPosition;
            }
        }
    }

    private bool IsPathClear(Vector2 targetPosition, Vector2 direction)
    {
        float distance = Vector2.Distance(rb.position, targetPosition);

        Vector2 origin = rb.position + direction * 0.9f;
        Vector2 castSize = collider.bounds.size * 0.9f;           // or * 0.9f if you prefer smaller

        var hits = Physics2D.BoxCastAll(
            origin,
            castSize,
            0,
            Vector2.zero,
            distance,
            Physics2D.GetLayerCollisionMask(gameObject.layer)
        );

        // ────────────────────────────────────────────────
        // Debug visualization
        Color color = hits.Length <= 1 ? Color.green : Color.red;
        Vector2 halfExtents = castSize * 0.5f;
        Vector2 right = Vector2.right * halfExtents.x;
        Vector2 up = Vector2.up * halfExtents.y;
        Vector2 left = -right;
        Vector2 down = -up;

        Debug.DrawLine(origin + right + up, origin + right + down, color, 1.2f);
        Debug.DrawLine(origin + right + down, origin + left + down, color, 1.2f);
        Debug.DrawLine(origin + left + down, origin + left + up, color, 1.2f);
        Debug.DrawLine(origin + left + up, origin + right + up, color, 1.2f);

        return !gameObject.HasHits(hits);
    }

    private void OnDrawGizmosSelected()
    {
        if (isMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(nextPosition, 0.4f);
            Gizmos.DrawLine(rb.position, nextPosition);
        }
    }
}
