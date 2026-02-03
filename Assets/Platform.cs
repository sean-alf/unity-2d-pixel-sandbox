using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class Platform : MonoBehaviour, ILoggerProvider, BetterInputManager.IInputChangeRequestor
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

    public Logger Logger => logger;

    public int Priority => inputMapSwitchingPriority;

    public string Name => $"{name} ({GetType().Name})";

    public BetterInputManager.InputType InputType => inputType;

    [SerializeField][Range(0, 100)] private int inputMapSwitchingPriority = 80;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool randomizeTurns = false;

    [Space]
    [Header("Debug")]

    [SerializeField] private Logger logger;
    [SerializeField] private bool isStopped = true;
    [SerializeField] private bool ignoreTerminals = false;
    [SerializeField] private bool autoAdjustSpeed = false;
    [SerializeField] private int autoSpeedAdjustTiming = 30;

    private Rigidbody2D rb;
    private Tilemap trackTilemap;

    private Vector2 currentDirection = Vector2.zero;
    private Vector3Int lastProcessedCellPosition;
    private TileAndDirection nextTarget;
    private PlayerController playerController;
    private Vector2 playerDirection;
    private BetterInputManager.InputType inputType = BetterInputManager.InputType.None;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trackTilemap = GameObject.Find("Above Ground").GetComponent<Tilemap>();

        if (trackTilemap == null)
        {
            logger.E("cannot find track Tilemap!");
            enabled = false;
        }
    }

    private void Start()
    {
        if (autoAdjustSpeed) StartCoroutine(TestAutoAdjustSpeed());
    }

    private void FixedUpdate()
    {
        if (isStopped) return;

        Vector2 oldPos = rb.position;

        if (nextTarget.IsValid && ShouldTrackToCenter(nextTarget.Tile.type))
        {
            logger.D($"next cell {nextTarget.Tile.cellPosition} → {nextTarget.Tile.type}");

            rb.MoveTowards(nextTarget.Tile.worldCenter, speed * Time.fixedDeltaTime);
            var distance = Vector2.Distance(nextTarget.Tile.worldCenter, rb.position);

            logger.V($"distance {distance}");

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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out AutoMover mover))
        {
            playerController = collision.GetComponent<PlayerController>();
            var inputManager = collision.GetComponent<BetterInputManager>();

            inputType = BetterInputManager.InputType.None;
            inputManager.AddInputChangeRequest(this);

            mover.MoveTo(rb.position, onDone: () =>
            {
                if (mover.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
                playerController.onDirectionChange.AddListener(PlayerDirectionChange);
                inputType = BetterInputManager.InputType.Aiming;
                inputManager.UpdateInputChangeRequest(this);
                collision.gameObject.transform.SetParent(transform);
                ResetAndStart();
            });
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController pc) && pc == playerController)
        {
            collision.gameObject.transform.SetParent(null);
            playerController = null;
        }
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

        logger.D($"entered cell {currentTile.cellPosition} → {currentTile.type}");

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
                            if (playerController)
                            {
                                playerController.onDirectionChange.RemoveListener(PlayerDirectionChange);

                                if (playerController.TryGetComponent(out BetterInputManager inputManager))
                                {
                                    inputManager.RemoveInputChangeRequest(this);
                                }

                                if (playerController.TryGetComponent(out Rigidbody2D rb))
                                {
                                    rb.bodyType = RigidbodyType2D.Dynamic;
                                    rb.WakeUp();
                                }
                            }
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

        logger.W("could not find any initial track direction!");
        Stop();
    }

    private void ChooseDirectionAtIntersection()
    {
        var candidates = new List<TileAndDirection>(3);

        TileAndDirection tileInDirection;
        // Player direction must be cardinal and it cannot be reverse
        bool playerHasDirection = playerDirection.IsCardinal() && playerDirection != -currentDirection;

        // First, prefer current player direction
        if (playerHasDirection)
        {
            tileInDirection = GetTileInDirection(playerDirection);
            if (tileInDirection.IsValid) candidates.Add(tileInDirection);
        }

        // Second, prefer continuing straight
        tileInDirection = GetTileInDirection(currentDirection);
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

        if (randomizeTurns && !playerHasDirection)
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

    public void PlayerDirectionChange(Vector2 direction) => playerDirection = direction;

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
