using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(KnockbackReceiver))]
[RequireComponent(typeof(LinearAnimator))]
[RequireComponent(typeof(DistanceWatcher))]
[RequireComponent(typeof(DirectionWatcher))]
public class ShufflingEnemy : MonoBehaviour
{
    [SerializeField] private float stepSize = 2f / 32f;
    [SerializeField] private float delayBetweenSteps = 0.1f;
    [SerializeField] private float numberOfStepsPerGroup = 3;
    [SerializeField] private float delayBetweenStepGroups = 0.5f;

    [Space]
    [Header("Debug")]

    [SerializeField] private Vector2 currentVelocity;
    [SerializeField] private Vector2 targetDirection;
    [SerializeField] private bool isPlayerInSight = false;
    [SerializeField] private int stepCounter = 0;
    [SerializeField] private float betweenStepDelayCounter = 0;
    [SerializeField] private float stepGroupDelayCounter = 0;
    [SerializeField] private float stepTimeCounter = 0f;

    private Rigidbody2D rb;
    private KnockbackReceiver knockbackReceiver;
    private LinearAnimator animator;
    private DistanceWatcher distanceWatcher;
    private DirectionWatcher directionWatcher;

    private bool IsStepping => Vector2.Distance(currentVelocity, Vector2.zero) > 0.01f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
        animator = GetComponent<LinearAnimator>();
        distanceWatcher = GetComponent<DistanceWatcher>();
        directionWatcher = GetComponent<DirectionWatcher>();

        var playerTransform = GameObject.Find("Player").transform;
        distanceWatcher.target = playerTransform;
        directionWatcher.target = playerTransform;
    }

    private void FixedUpdate()
    {
        if (isPlayerInSight)
        {
            if (stepGroupDelayCounter > 0f)
            {
                stepGroupDelayCounter -= Time.fixedDeltaTime;
            }
            else if (betweenStepDelayCounter > 0f)
            {
                betweenStepDelayCounter -= Time.fixedDeltaTime;
            }
            else if (stepTimeCounter > 0f)
            {
                currentVelocity = targetDirection;
                stepTimeCounter -= Time.fixedDeltaTime;
            }
            else
            {
                if (stepCounter == numberOfStepsPerGroup)
                {
                    stepGroupDelayCounter = delayBetweenStepGroups;
                    stepCounter = 0;
                }
                else if (IsStepping)
                {
                    betweenStepDelayCounter = delayBetweenSteps;
                    ++stepCounter;
                }
                else
                {
                    stepTimeCounter = stepSize;
                }

                currentVelocity = Vector2.zero;
            }
        }

        rb.linearVelocity = knockbackReceiver.KnockbackVelocity + currentVelocity;
    }

    // ──────────────────────────────────────────────────────────────
    // Public Control Methods
    // ──────────────────────────────────────────────────────────────

    public void ActivateEnemy() => isPlayerInSight = true;

    // The following DeactivateEnemy method variations are there for use with UnityEvents
    // as well as calling directly from other scripts

    public void DeactivateEnemy() => DeactivateEnemy(false);

    public void DeactivateEnemyAndStopAnimation() => DeactivateEnemy(true);

    public void DeactivateEnemy(bool stopAnimation)
    {
        if (stopAnimation) animator.Stop();
        isPlayerInSight = false;
        currentVelocity = Vector2.zero;
        stepCounter = 0;
        betweenStepDelayCounter = 0;
        stepGroupDelayCounter = 0;
        stepTimeCounter = 0f;
    }

    public void NewTargetDirection(Vector2 newDirection) => targetDirection = newDirection;

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────
}
