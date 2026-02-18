using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DirectionWatcher))]
public class SpinSlicer : MonoBehaviour
{
    [Header("Spin Velocity")]
    [SerializeField] private float idleSpinVelocity = 180f;
    [SerializeField] private float watchSpinVelocity = 360f;
    [SerializeField] private float attackSpinVelocity = 1440f;
    [SerializeField] private float spinVelocityChangeDuration = 0.5f;

    [Header("Center Eye")]
    [SerializeField] private Transform centerEye;
    [SerializeField] private Sprite lookStraightSprite;
    [SerializeField] private Sprite lookUpSprite;
    [SerializeField] private Sprite lookLeftSprite;
    [SerializeField] private Sprite lookDownSprite;
    [SerializeField] private Sprite lookRightSprite;
    [SerializeField] private Sprite lookUpLeftSprite;
    [SerializeField] private Sprite lookDownLeftSprite;
    [SerializeField] private Sprite lookUpRightSprite;
    [SerializeField] private Sprite lookDownRightSprite;

    [Header("Arc Attack")]
    [SerializeField] private float attackArcHeight = 1.2f;
    [SerializeField] private float attackDuration = 0.9f;

    [Space]
    [Header("Debug")]

    [HideInInspector]
    [SerializeField] private Transform target;
    [HideInInspector]
    [SerializeField] private SpriteRenderer centerEyeSR;
    [HideInInspector]
    [SerializeField] private State currentState = State.Idle;
    [HideInInspector]
    [SerializeField] private State nextState = State.Idle;
    [HideInInspector]
    [SerializeField] private SpinState currentSpinState = SpinState.Idle;
    [HideInInspector]
    [SerializeField] private float currentAngularVelocity;
    [HideInInspector]
    [SerializeField] private float startingAngularVelocity;
    [HideInInspector]
    [SerializeField] private float targetAngularVelocity;
    [HideInInspector]
    [SerializeField] private Vector2 currentLinearVelocity;
    [HideInInspector]
    [SerializeField] private Vector2 startPosition;
    [HideInInspector]
    [SerializeField] private Vector2 targetPosition;
    [HideInInspector]
    [SerializeField] private float arcProgress;
    [HideInInspector]
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

        var directionWatcher = GetComponent<DirectionWatcher>();
        var distanceWatchers = GetComponents<DistanceWatcher>();
        target = GameObject.Find("Player").transform;

        directionWatcher.target = target;
        foreach (var watchers in distanceWatchers)
        {
            watchers.target = target;
        }

        centerEyeSR = centerEye.GetComponent<SpriteRenderer>();
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

    private void LateUpdate()
    {
        // Prevent the center eye from rotating with the parent GameObject
        centerEye.rotation = Quaternion.identity;
    }

    // ──────────────────────────────────────────────────────────────
    // Public Control Methods
    // ──────────────────────────────────────────────────────────────

    public void SetStateIdle() => TransitionTo(State.Idle);

    public void SetStateWatching() => TransitionTo(State.Watching);

    public void SetStateAttacking() => TransitionTo(State.Attacking);

    public void SetStateDead() => SetState(State.Dead);

    public void OnNewTargetDirection(Vector2 direction)
    {
        if (direction.IsIdle())
        {
            centerEyeSR.sprite = lookStraightSprite;
            return;
        }

        float angle = Vector2.SignedAngle(Vector2.down, direction);
        float rotation = Mathf.Round(angle / 45f) * 45f;

        centerEyeSR.sprite = rotation switch
        {
            -180 => lookUpSprite,
            -135 => lookUpLeftSprite,
            -90 => lookLeftSprite,
            -45 => lookDownLeftSprite,
            0 => lookDownSprite,
            45 => lookDownRightSprite,
            90 => lookRightSprite,
            135 => lookUpRightSprite,
            180 => lookUpSprite,
            _ => lookStraightSprite,
        };
    }

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
                var isAttackAngularVelocity = currentAngularVelocity.Approximately(attackSpinVelocity);

                // Debug.Log($"{name} ({GetType().Name}): OnTargetAngularVelocityReached: attackAngularVelocity {attackAngularVelocity}");
                // Debug.Log($"{name} ({GetType().Name}): OnTargetAngularVelocityReached: isAttackAngularVelocity {isAttackAngularVelocity}");

                if (isAttackAngularVelocity)
                {
                    currentSpinState = SpinState.Idle;
                    InitAttack();
                }
                else
                {
                    SetTargetAngularVelocity(attackSpinVelocity, SpinState.AttackCooldown);
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
        SetTargetAngularVelocity(watchSpinVelocity, SpinState.AttackCooldown);
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
                SetTargetAngularVelocity(idleSpinVelocity, SpinState.VelocityChange);
                break;
            case State.Watching:
                SetTargetAngularVelocity(watchSpinVelocity, SpinState.VelocityChange);
                break;
            case State.Attacking:
                SetTargetAngularVelocity(attackSpinVelocity, SpinState.AttackSpinup);
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
