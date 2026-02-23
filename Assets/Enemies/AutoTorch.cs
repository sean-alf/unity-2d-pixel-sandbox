using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DirectionWatcher))]
[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(Collider2D))]
public class AutoTorch : MonoBehaviour
{
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private LinearAnimator leftWheelAnimator;
    [SerializeField] private LinearAnimator rightWheelAnimator;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float idleAngle;
    [SerializeField] private float shootDelayDuration = 0.5f;
    [SerializeField][Tooltip("How close the player should be when this should stop")] private float stopDistance = 2f;
    [SerializeField][Tooltip("How close the player should be when this should back up (must be less than stopDistance)")] private float backupDistance = 1.5f;

    [Header("Obstruction Avoidance")]
    [SerializeField] private float boxcastDistance = 3f;
    [SerializeField] private float boxcastForwardOffset = 0.25f;
    [SerializeField] private float boxcastBoundsSizeScale = 1.2f;
    [SerializeField] private LayerMask boxcastLayerMask;

    [Space]
    [Header("Debug")]

    [SerializeField] private Vector2 currentLinearVelocity;
    [SerializeField] private float currentAngularVelocity;
    [SerializeField] private State currentState;
    [SerializeField] private float targetAngle;
    [SerializeField] private Vector2 targetDirection;
    [SerializeField] private float shootDelaytimer;

    private Rigidbody2D rb;
    private ProjectileManager projectileManager;
    private new Collider2D collider;
    private Transform playerTransform;
    private bool shouldAttack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileManager = GetComponent<ProjectileManager>();
        collider = GetComponent<Collider2D>();

        playerTransform = GameObject.Find("Player").transform;
        var distanceWatchers = GetComponents<DistanceWatcher>();
        var directionWatcher = GetComponent<DirectionWatcher>();

        foreach (var watcher in distanceWatchers)
        {
            watcher.target = playerTransform;
        }
        directionWatcher.target = playerTransform;
    }

    private void Update()
    {
        if (shouldAttack)
        {
            if (shootDelaytimer <= 0f)
            {
                projectileManager.Shoot(
                    new ProjectileManager.StartingPointWithDirection()
                    {
                        position = projectileSpawnPoint.position,
                        direction = transform.up,
                    }
                );
                shootDelaytimer = shootDelayDuration;
            }
            else
            {
                shootDelaytimer -= Time.deltaTime;
            }
        }
    }

    private void FixedUpdate()
    {

        rb.linearVelocity = currentLinearVelocity;

        if (Mathf.Abs(Mathf.DeltaAngle(rb.rotation, targetAngle)) < 1f)
        {
            StopTurning();
            rb.rotation = targetAngle;
        }
        else
        {
            rb.rotation = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
        }

        float playerDistance = Vector2.Distance(rb.position, playerTransform.position);

        if (currentState == State.Attack)
        {
            if (playerDistance <= backupDistance)
            {
                MoveBackward();
            }
            else if (playerDistance <= stopDistance)
            {
                StopMoving();
            }
            else
            {
                MoveForward();
            }
        }
    }

    public void MoveForward()
    {
        Vector2 direction = transform.up;

        if (CheckForObstruction(direction))
        {
            StopMoving();
            return;
        }

        shouldAttack = false;
        leftWheelAnimator.Animate("Default");
        rightWheelAnimator.Animate("Default");
        currentLinearVelocity = moveSpeed * direction;
    }

    public void MoveBackward()
    {
        Vector2 direction = -transform.up;

        if (CheckForObstruction(direction))
        {
            StopMoving();
            return;
        }

        shouldAttack = true;
        leftWheelAnimator.AnimateReverse("Default");
        rightWheelAnimator.AnimateReverse("Default");
        currentLinearVelocity = moveSpeed * direction;
    }

    public void TurnLeft()
    {
        leftWheelAnimator.AnimateReverse("Default");
        rightWheelAnimator.Animate("Default");
        currentAngularVelocity = turnSpeed;
    }

    public void TurnRight()
    {
        leftWheelAnimator.Animate("Default");
        rightWheelAnimator.AnimateReverse("Default");
        currentAngularVelocity = -turnSpeed;
    }

    public void StopTurning()
    {
        currentAngularVelocity = 0f;

        if (currentLinearVelocity == Vector2.zero)
        {
            leftWheelAnimator.Stop();
            rightWheelAnimator.Stop();
        }
    }

    public void StopMoving()
    {
        shouldAttack = currentState == State.Attack;
        currentLinearVelocity = Vector2.zero;

        if (currentAngularVelocity == 0f)
        {
            leftWheelAnimator.Stop();
            rightWheelAnimator.Stop();
        }
    }

    public void Stop()
    {
        StopTurning();
        StopMoving();
    }

    public void OnTargetDirectionChange(Vector2 newDirection)
    {
        targetDirection = newDirection;
        targetAngle = Vector2.SignedAngle(Vector2.up, targetDirection);
        targetAngle = Mathf.Round(targetAngle);

        var av = Mathf.DeltaAngle(rb.rotation, targetAngle);

        if (av < 0)
        {
            TurnRight();
        }
        else if (av > 0)
        {
            TurnLeft();
        }
    }

    public void TransitionToIdle() => SetState(State.Idle);

    public void TransitionToWatch() => SetState(State.Watch);

    public void TransitionToAttack() => SetState(State.Attack);

    /// <summary>
    /// Checks for obstructions in direction for boxCastDistance distance.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns>True if there is an obstruction</returns>
    private bool CheckForObstruction(Vector2 direction)
    {
        var adjustedSize = collider.bounds.size * boxcastBoundsSizeScale;
        var hits = Physics2D.BoxCastAll(
            origin: rb.position + (direction * boxcastForwardOffset),
            size: adjustedSize,
            angle: 0f,
            direction: direction.normalized,
            distance: boxcastDistance,
            layerMask: boxcastLayerMask
        );
        return gameObject.HasHits(hits);
    }

    private void SetState(State newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case State.Idle:
                SetIdle();
                break;
            case State.Watch:
                SetWatch();
                break;
            case State.Attack:
                SetAttack();
                break;
        }
    }

    private void SetIdle()
    {
        Stop();
        targetAngle = idleAngle;
    }

    private void SetWatch()
    {
        StopMoving();
    }

    private void SetAttack()
    {
        shootDelaytimer = 0f;
    }

    public enum State
    {
        Idle,
        Watch,
        Attack,
    }
}
