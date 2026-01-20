using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class Platform : MonoBehaviour
{
    // This must stay in sync with Tiled
    enum TrackType
    {
        None,
        Terminal,
        Intersection,
        Broken,
    }

    private readonly struct TileAndDirection
    {
        public readonly SuperTileWithCellPosition<TrackType> Tile;
        public readonly Vector2 Direction;

        public TileAndDirection(SuperTileWithCellPosition<TrackType> tile, Vector2 direction)
        {
            Tile = tile;
            Direction = direction;
        }

        public bool IsValid => Tile != null;
    }

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool randomizeTurns = false;

    [Space]
    [Header("Debug")]
    [SerializeField] private bool isStopped = false;
    [SerializeField] private bool ignoreTerminals = false;
    [SerializeField] private bool autoAdjustSpeed = false;
    [SerializeField] private int autoSpeedAdjustTiming = 30;

    private Rigidbody2D rb;
    private Tilemap trackTilemap;

    private Vector2 currentDirection = Vector2.zero;
    private Vector3Int lastProcessedCellPosition;
    private TileAndDirection nextTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trackTilemap = GameObject.Find("Above Ground Trigger Layer").GetComponent<Tilemap>();

        if (trackTilemap == null)
        {
            Debug.LogError("Platform cannot find track Tilemap!", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if (autoAdjustSpeed) StartCoroutine(TestAutoAdjustSpeed());
        ResetAndStart();
    }

    private void FixedUpdate()
    {
        if (isStopped) return;

        Vector2 oldPos = rb.position;

        if (nextTarget.IsValid && ShouldTrackToCenter(nextTarget.Tile.type))
        {
            // Debug.Log($"Platform ({name}): next cell {nextTarget.Tile.cellPosition} → {nextTarget.Tile.type}");

            rb.MoveTowards(nextTarget.Tile.worldCenter, speed * Time.fixedDeltaTime);
            var distance = Vector2.Distance(nextTarget.Tile.worldCenter, rb.position);

            // Debug.Log($"Platform distance {distance}");
            if (distance <= 0.005f)
            {
                rb.MovePosition(nextTarget.Tile.worldCenter);
                UpdateNextTarget();
            }
        }
        else
        {
            if (nextTarget.IsValid) rb.MoveTowards(nextTarget.Tile.worldCenter, speed * Time.fixedDeltaTime);
            UpdateNextTarget();
        }

        rb.linearVelocity = rb.position - oldPos;
    }

    // ──────────────────────────────────────────────────────────────
    // Public control methods
    // ──────────────────────────────────────────────────────────────

    public void ResetAndStart()
    {
        nextTarget = default;
        lastProcessedCellPosition = Vector3Int.zero;
        isStopped = false;
        currentDirection = Vector2.zero;
    }

    public void Stop()
    {
        isStopped = true;
        currentDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    // ──────────────────────────────────────────────────────────────
    // Core track following logic
    // ──────────────────────────────────────────────────────────────

    private void UpdateNextTarget()
    {
        var currentTile = trackTilemap.GetSuperTileFromWorldPosition<TrackType>(rb.position);

        if (currentTile.cellPosition == lastProcessedCellPosition) return;

        lastProcessedCellPosition = currentTile.cellPosition;

        // Debug.Log($"Platform ({name}): entered cell {currentTile.cellPosition} → {currentTile.type}");

        switch (currentTile.type)
        {
            case TrackType.Terminal:
                {
                    if (nextTarget.IsValid)
                    {
                        if (ignoreTerminals)
                        {
                            TryReverseOrStop();
                        }
                        else
                        {
                            Stop();
                        }
                    }
                    else
                    {
                        TrySetInitialDirection();
                    }
                    break;
                }
            case TrackType.Broken:
                {
                    TryReverseOrStop();
                    break;
                }
            case TrackType.Intersection:
                {
                    ChooseDirectionAtIntersection();
                    break;
                }
            case TrackType.None:
                {
                    TryContinueOrReverse();
                    break;
                }
        }
    }

    private void TrySetInitialDirection()
    {
        Vector2[] cardinalDirections = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        foreach (var dir in cardinalDirections)
        {
            var candidate = GetTileInDirection(dir);
            if (candidate.IsValid)
            {
                currentDirection = dir;
                nextTarget = candidate;
                return;
            }
        }

        Debug.LogWarning("Platform could not find any initial track direction!", this);
        Stop();
    }

    private void ChooseDirectionAtIntersection()
    {
        var candidates = new List<TileAndDirection>(3);

        // Prefer continuing straight
        var tileInDirection = GetTileInDirection(currentDirection);
        if (tileInDirection.IsValid) candidates.Add(tileInDirection);

        // Left
        tileInDirection = GetTileInDirection(Vector2.Perpendicular(currentDirection));
        if (tileInDirection.IsValid) candidates.Add(tileInDirection);

        // Right
        tileInDirection = GetTileInDirection(-Vector2.Perpendicular(currentDirection));
        if (tileInDirection.IsValid) candidates.Add(tileInDirection);

        if (candidates.Count == 0)
        {
            TryReverseOrStop();
            return;
        }

        if (randomizeTurns)
        {
            nextTarget = candidates[Random.Range(0, candidates.Count)];
        }
        else
        {
            nextTarget = candidates[0];
        }

        currentDirection = nextTarget.Direction;
    }

    private void TryContinueOrReverse()
    {
        var forward = GetTileInDirection(currentDirection);
        if (forward.IsValid) nextTarget = forward;
        else TryReverseOrStop();
    }

    public void TryReverseOrStop()
    {
        var direction = -currentDirection;
        var reverse = GetTileInDirection(direction);

        if (reverse.IsValid)
        {
            nextTarget = reverse;
            currentDirection = direction;
        }
        else
        {
            Stop();
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private TileAndDirection GetTileInDirection(Vector2 direction) => new(
        tile: trackTilemap.GetSuperTileFromWorldPosition<TrackType>(rb.position + direction),
        direction
    );

    private static bool ShouldTrackToCenter(TrackType type) => type is TrackType.Terminal
        or TrackType.Intersection
        or TrackType.Broken;

    // ──────────────────────────────────────────────────────────────
    // Coroutines
    // ──────────────────────────────────────────────────────────────

    private IEnumerator TestAutoAdjustSpeed()
    {
        while (true)
        {
            speed = ((speed + 0.5f) % 25) + 1;
            yield return new WaitForSeconds(autoSpeedAdjustTiming);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Gizmos
    // ──────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!nextTarget.IsValid) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(nextTarget.Tile.worldCenter, 0.16f);
        Gizmos.color = new Color(0, 1, 1, 0.4f);
        Gizmos.DrawLine(rb.position, nextTarget.Tile.worldCenter);
    }
}
