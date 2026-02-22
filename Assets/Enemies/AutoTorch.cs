using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DirectionWatcher))]
[RequireComponent(typeof(ProjectileManager))]
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
    private Transform playerTransform;
    private bool shouldAttack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileManager = GetComponent<ProjectileManager>();

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
                        direction = transform.rotation * Vector2.up,
                    }
                );
            }
            else
            {
                shootDelaytimer = shootDelayDuration;
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

        if (playerDistance <= backupDistance)
        {
            MoveBackward();
        }
        else if (playerDistance <= stopDistance)
        {
            StopMoving();
        }
        else if (currentState == State.Attack)
        {
            MoveForward();
        }
    }

    public void MoveForward()
    {
        shouldAttack = false;
        leftWheelAnimator.Animate("Default");
        rightWheelAnimator.Animate("Default");
        Vector2 direction = Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.up;
        currentLinearVelocity = moveSpeed * direction;
    }

    public void MoveBackward()
    {
        shouldAttack = true;
        leftWheelAnimator.AnimateReverse("Default");
        rightWheelAnimator.AnimateReverse("Default");
        Vector2 direction = Quaternion.Euler(0f, 0f, rb.rotation) * Vector2.up;
        currentLinearVelocity = -moveSpeed * direction;
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
