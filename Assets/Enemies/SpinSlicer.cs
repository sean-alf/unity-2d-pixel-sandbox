using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpinSlicer : MonoBehaviour
{
    [SerializeField] private float idleRotationsPerSecond = 0.5f;
    [SerializeField] private float watchingRotationsPerSecond = 1f;
    [SerializeField] private float attackingRotationsPerSecond = 2f;

    [Header("Arc Attack")]
    [SerializeField] private float attackArcSpeed = 6f;
    [SerializeField] private float attackArcHeight = 1.2f;
    [SerializeField] private float attackDuration = 0.9f;
    [SerializeField] private float attackSpinupDuration = 2f;
    [SerializeField] private float attackCooldownPeriod = 2f;

    [Space]
    [Header("Debug")]

    [SerializeField] private Transform target;
    [SerializeField] private State currentState = State.Idle;
    [SerializeField] private State nextState = State.Idle;
    [SerializeField] private float currentAngularVelocity;
    [SerializeField] private Vector2 currentLinearVelocity;
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float arcProgress;
    [SerializeField] private float attackCooldownTimer;
    [SerializeField] private float attackSpinUpTimer;

    private Rigidbody2D rb;
    private KnockbackReceiver knockbackReceiver;

    // ──────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        knockbackReceiver = GetComponentInChildren<KnockbackReceiver>();

        var distanceWatchers = GetComponents<DistanceWatcher>();
        target = GameObject.Find("Player").transform;

        foreach (var watchers in distanceWatchers)
        {
            watchers.target = target;
        }
    }

    private void Start()
    {
        SetState(State.Idle);
    }

    private void FixedUpdate()
    {
        rb.angularVelocity = currentAngularVelocity;
        HandleAttackMovement();
        rb.linearVelocity = knockbackReceiver.KnockbackVelocity + currentLinearVelocity;
    }

    // ──────────────────────────────────────────────────────────────
    // Public Control Methods
    // ──────────────────────────────────────────────────────────────

    public void SetStateIdle() => UpdateState(State.Idle);

    public void SetStateWatching() => UpdateState(State.Watching);

    public void SetStateAttacking() => UpdateState(State.Attacking);

    public void SetStateDead() => SetState(State.Dead);

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private void UpdateState(State state)
    {
        nextState = state;
        if (currentState == state || currentState == State.Attacking) return;
        SetState(state);
    }

    private void InitAttack()
    {
        startPosition = (Vector2)transform.position;
        targetPosition = target.position;
        arcProgress = 0f;
    }

    private void HandleAttackMovement()
    {
        if (currentState != State.Attacking) return;

        if (attackSpinUpTimer > 0f)
        {
            if (InterruptIfNoLongerAttacking()) return;

            attackSpinUpTimer -= Time.fixedDeltaTime;
            var t = 1f - (attackSpinUpTimer / attackSpinupDuration);
            currentAngularVelocity = Mathf.Lerp(watchingRotationsPerSecond, attackingRotationsPerSecond, t) * 360f;

            if (attackSpinUpTimer <= 0)
            {
                currentAngularVelocity = attackingRotationsPerSecond * 360f;
            }

            return;
        }

        if (attackCooldownTimer >= 0f)
        {
            if (InterruptIfNoLongerAttacking()) return;

            attackCooldownTimer -= Time.fixedDeltaTime;
            float t = 1f - attackCooldownTimer / attackCooldownPeriod;
            float rotationsPerSecond;

            if (t <= 0.5f)
            {
                rotationsPerSecond = Mathf.Lerp(attackingRotationsPerSecond, watchingRotationsPerSecond, 2 * t);
            }
            else
            {
                rotationsPerSecond = Mathf.Lerp(watchingRotationsPerSecond, attackingRotationsPerSecond, 2 * (t - 0.5f));
            }

            currentAngularVelocity = rotationsPerSecond * 360f;
            InitAttack();
            return;
        }

        arcProgress += Time.fixedDeltaTime / attackDuration;

        if (arcProgress >= 1f)
        {
            currentLinearVelocity = Vector2.zero;
            if (nextState != State.Attacking) SetState(nextState);
            attackCooldownTimer = attackCooldownPeriod;
            return;
        }

        Vector2 targetPosThisFrame = ParabolicPosition(arcProgress);
        currentLinearVelocity = (targetPosThisFrame - (Vector2)transform.position) / Time.fixedDeltaTime;
        currentAngularVelocity = attackingRotationsPerSecond * 360f;
    }

    private bool InterruptIfNoLongerAttacking()
    {
        if (nextState != State.Attacking)
        {
            attackCooldownTimer = 0f;
            attackSpinUpTimer = 0f;
            SetState(nextState);
            return true;
        }
        return false;
    }

    private void SetState(State state)
    {
        currentState = state;

        switch (currentState)
        {
            case State.Idle:
                currentAngularVelocity = idleRotationsPerSecond * 360f;
                break;
            case State.Watching:
                currentAngularVelocity = watchingRotationsPerSecond * 360f;
                break;
            case State.Attacking:
                attackSpinUpTimer = attackSpinupDuration;
                InitAttack();
                break;
            case State.Dead:
                currentAngularVelocity = 0f;
                currentLinearVelocity = Vector2.zero;
                break;
        }
    }

    private Vector2 ParabolicPosition(float t)
    {
        Vector2 travelDirection = (targetPosition - startPosition).normalized;
        Vector2 arcDirection = -Vector2.Perpendicular(travelDirection);
        float heightOffset = attackArcHeight * (1f - (2f * t - 1f) * (2f * t - 1f));
        Vector2 straightPos = Vector2.Lerp(startPosition, targetPosition, t);
        return straightPos + arcDirection * heightOffset;
    }

    public enum State
    {
        Idle,
        Watching,
        Attacking,
        Dead
    }
}
