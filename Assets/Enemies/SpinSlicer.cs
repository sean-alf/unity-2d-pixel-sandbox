using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpinSlicer : MonoBehaviour
{
    [SerializeField] private float idleAngularVelocity = 180f;
    [SerializeField] private float watchAngularVelocity = 360f;
    [SerializeField] private float attackAngularVelocity = 1440f;
    [SerializeField] private float spinVelocityChangeDuration = 1f;

    [Header("Arc Attack")]
    [SerializeField] private float attackArcSpeed = 6f;
    [SerializeField] private float attackArcHeight = 1.2f;
    [SerializeField] private float attackDuration = 0.9f;
    [SerializeField] private float attackCooldownPeriod = 2f;

    [Space]
    [Header("Debug")]

    [SerializeField] private Transform target;
    [SerializeField] private State currentState = State.Idle;
    [SerializeField] private State nextState = State.Idle;
    [SerializeField] private SpinState currentSpinState = SpinState.Idle;
    [SerializeField] private float currentAngularVelocity;
    [SerializeField] private float startingAngularVelocity;
    [SerializeField] private float targetAngularVelocity;
    [SerializeField] private Vector2 currentLinearVelocity;
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float arcProgress;
    [SerializeField] private float spinVelocityChangeTimer;

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
        if (currentState != State.Dead)
        {
            HandleSpinMovement();
            HandleArcMovement();
        }
        rb.angularVelocity = currentAngularVelocity;
        rb.linearVelocity = knockbackReceiver.KnockbackVelocity + currentLinearVelocity;
    }

    // ──────────────────────────────────────────────────────────────
    // Public Control Methods
    // ──────────────────────────────────────────────────────────────

    public void SetStateIdle() => TransitionTo(State.Idle);

    public void SetStateWatching() => TransitionTo(State.Watching);

    public void SetStateAttacking() => TransitionTo(State.Attacking);

    public void SetStateDead() => SetState(State.Dead);

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    public void TransitionTo(State state)
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

    private void HandleSpinMovement()
    {
        // Debug.Log($"{name} ({GetType().Name}): HandleSpinMovement: currentState {currentState}");
        // Debug.Log($"{name} ({GetType().Name}): HandleSpinMovement: rb.angularVelocity {rb.angularVelocity}");
        // Debug.Log($"{name} ({GetType().Name}): HandleSpinMovement: targetAngularVelocity {targetAngularVelocity}");
        // Debug.Log($"{name} ({GetType().Name}): HandleSpinMovement: are equal {rb.angularVelocity == targetAngularVelocity}");

        if (spinVelocityChangeTimer <= 0f) return;
        if (currentAngularVelocity == targetAngularVelocity)
        {
            spinVelocityChangeTimer = 0f;
            return;
        }

        spinVelocityChangeTimer -= Time.fixedDeltaTime;
        var t = 1f - spinVelocityChangeTimer / spinVelocityChangeDuration;
        var newSpinVelocity = Mathf.Lerp(startingAngularVelocity, targetAngularVelocity, t);

        currentAngularVelocity = newSpinVelocity;

        if (spinVelocityChangeTimer <= 0f) OnTargetAngularVelocityReached();

        InterruptIfNoLongerAttacking();
    }

    private void OnTargetAngularVelocityReached()
    {
        currentAngularVelocity = targetAngularVelocity;

        switch (currentSpinState)
        {
            case SpinState.VelocityChange:
                currentSpinState = SpinState.Idle;
                break;
            case SpinState.AttackSpinup:
                InitAttack();
                currentSpinState = SpinState.Idle;
                break;
            case SpinState.AttackCooldown:
                var isAttackAngularVelocity = currentAngularVelocity.Approximately(attackAngularVelocity);

                // Debug.Log($"{name} ({GetType().Name}): OnTargetAngularVelocityReached: attackAngularVelocity {attackAngularVelocity}");
                // Debug.Log($"{name} ({GetType().Name}): OnTargetAngularVelocityReached: isAttackAngularVelocity {isAttackAngularVelocity}");

                if (isAttackAngularVelocity)
                {
                    currentSpinState = SpinState.Idle;
                    InitAttack();
                }
                else
                {
                    SetTargetAngularVelocity(attackAngularVelocity, SpinState.AttackCooldown);
                }
                break;
        }

        // Debug.Log($"{name} ({GetType().Name}): OnTargetAngularVelocityReached: currentSpinState {currentSpinState}");
    }

    private void HandleArcMovement()
    {
        if (currentState != State.Attacking || currentSpinState != SpinState.Idle || arcProgress >= 1f) return;

        arcProgress += Time.fixedDeltaTime / attackDuration;

        if (arcProgress >= 1f)
        {
            OnNewPositionReached();
        }
        else
        {
            Vector2 intermediateTargetPosition = ParabolicPosition(arcProgress);
            currentLinearVelocity = (intermediateTargetPosition - (Vector2)transform.position) / Time.fixedDeltaTime;
        }
    }

    private void OnNewPositionReached()
    {
        currentLinearVelocity = Vector2.zero;
        if (nextState != State.Attacking) SetState(nextState);
        SetTargetAngularVelocity(watchAngularVelocity, SpinState.AttackCooldown);
    }

    private void InterruptIfNoLongerAttacking()
    {
        if (currentState == State.Attacking && nextState != State.Attacking)
        {
            SetState(nextState);
        }
    }

    private void SetState(State state)
    {
        currentState = state;

        switch (currentState)
        {
            case State.Idle:
                SetTargetAngularVelocity(idleAngularVelocity, SpinState.VelocityChange);
                break;
            case State.Watching:
                SetTargetAngularVelocity(watchAngularVelocity, SpinState.VelocityChange);
                break;
            case State.Attacking:
                SetTargetAngularVelocity(attackAngularVelocity, SpinState.AttackSpinup);
                break;
            case State.Dead:
                currentAngularVelocity = 0f;
                currentLinearVelocity = Vector2.zero;
                spinVelocityChangeTimer = 0f;
                break;
        }
    }

    private void SetTargetAngularVelocity(float angularVelocity, SpinState newSpinState)
    {
        currentSpinState = newSpinState;
        startingAngularVelocity = currentAngularVelocity;
        targetAngularVelocity = angularVelocity;
        spinVelocityChangeTimer = spinVelocityChangeDuration;

        // Debug.Log($"{name} ({GetType().Name}): SetTargetAngularVelocity: startingAngularVelocity {startingAngularVelocity}");
        // Debug.Log($"{name} ({GetType().Name}): SetTargetAngularVelocity: targetAngularVelocity {targetAngularVelocity}");
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

    public enum SpinState
    {
        Idle,
        VelocityChange,
        AttackSpinup,
        AttackCooldown,
    }
}
