using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CollisionData))]
[RequireComponent(typeof(ProjectileManager))]
public class GroundSmasher : MonoBehaviour
{
    [SerializeField] private float liftSpeed = 1f;
    [SerializeField] private float smashSpeed = 6f;
    [SerializeField] private float liftHeight = 1.5f;
    [SerializeField] private bool smashLeft = true;
    [SerializeField] private float afterLiftDuration = 0.5f;
    [SerializeField] private float betweenSmashDuration = 2f;

    [Header("Pushable")]
    [SerializeField] private bool isPushableWhenDead = false;
    [SerializeField] private CardinalDirection[] allowedDirections;
    [SerializeField] private int maxPushCount = 2;

    [Header("Face")]
    [SerializeField] private Sprite liftFaceSprite;
    [SerializeField] private Sprite smashFaceSprite;
    [SerializeField] private Sprite restFaceSprite;
    [SerializeField] private Sprite deadFaceSprite;

    [Header("Smashers")]
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;
    [SerializeField] private Sprite oneThirdDestroyedSprite;
    [SerializeField] private Sprite twoThirdsDestroyedSprite;

    [Space]
    [Header("Debug")]

    [SerializeField] private float originalY;
    [SerializeField] private float targetHeight;
    [SerializeField] private float timer;
    [SerializeField] private State currentState = State.Lift;

    private ProjectileManager projectileManager;
    private SpriteRenderer mainSR;
    private CollisionData collisionData;
    private Rigidbody2D leftRB;
    private Rigidbody2D rightRB;
    private Rigidbody2D targetRB;
    private SpriteRenderer leftSR;
    private SpriteRenderer rightSR;
    private EnemyDamageHandler leftDamageHandler;
    private EnemyDamageHandler rightDamageHandler;
    private EnemyDamageHandler targetDamageHandler;

    private void Awake()
    {
        projectileManager = GetComponent<ProjectileManager>();
        mainSR = GetComponent<SpriteRenderer>();
        collisionData = GetComponent<CollisionData>();

        leftRB = left.GetComponent<Rigidbody2D>();
        rightRB = right.GetComponent<Rigidbody2D>();
        leftSR = left.GetComponent<SpriteRenderer>();
        rightSR = right.GetComponent<SpriteRenderer>();
        leftDamageHandler = left.GetComponent<EnemyDamageHandler>();
        rightDamageHandler = right.GetComponent<EnemyDamageHandler>();

        originalY = left.transform.localPosition.y;
        targetHeight = originalY + liftHeight;
        SelectNextTarget();

        leftDamageHandler.onHealthChanged.AddListener(OnLeftHealthChanged);
        rightDamageHandler.onHealthChanged.AddListener(OnRightHealthChanged);

        UpdateState(currentState);
    }

    private void OnDestroy()
    {
        // Doing this just in case although I believe they automatically get unregistered when the object is destroyed
        leftDamageHandler.onHealthChanged.RemoveListener(OnLeftHealthChanged);
        rightDamageHandler.onHealthChanged.RemoveListener(OnRightHealthChanged);
        Destroy(collisionData);
        Destroy(projectileManager);
    }

    private void FixedUpdate()
    {
        if (currentState == State.Dead) return;

        if (left == null && right == null)
        {
            UpdateState(State.Dead);
            Destroy(this);
        }

        if (targetRB == null) SelectNextTarget();

        if (targetRB == null || currentState == State.Ignore) return;

        switch (currentState)
        {
            case State.Lift:
                Lift();
                break;
            case State.AfterLiftDelay:
                HandleDelay(State.Smash);
                break;
            case State.Smash:
                Smash();
                break;
            case State.AfterSmashDelay:
                HandleDelay(State.Lift);
                break;
        }
    }

    private void HandleDelay(State nextState)
    {
        if (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
        }
        else
        {
            UpdateState(nextState);
        }
    }

    private void Lift()
    {
        targetDamageHandler.deferDeath = true;
        targetRB.linearVelocity = new(0, liftSpeed);
        if (targetRB.transform.localPosition.y >= targetHeight) OnLiftFinished();
    }

    private void OnLiftFinished()
    {
        targetRB.linearVelocity = Vector2.zero;
        targetRB.transform.localPosition = new(targetRB.transform.localPosition.x, targetHeight);
        timer = afterLiftDuration;
        UpdateState(State.AfterLiftDelay);
    }

    private void Smash()
    {
        targetRB.linearVelocity = new(0, -smashSpeed);
        if (targetRB.transform.localPosition.y <= originalY) OnSmashFinished();
    }

    private void OnSmashFinished()
    {
        UpdateState(State.Ignore);
        targetRB.linearVelocity = Vector2.zero;
        targetRB.transform.localPosition = new(targetRB.transform.localPosition.x, originalY);
        targetDamageHandler.DieIfDead(onDone: () =>
        {
            targetDamageHandler.deferDeath = false;
            timer = betweenSmashDuration;

            if (!targetDamageHandler.IsDead)
            {
                projectileManager.Shoot(CalculateStartingPoints());
            }

            SelectNextTarget();
            UpdateState(State.AfterSmashDelay);
        });
    }

    private void UpdateState(State newState)
    {
        currentState = newState;
        mainSR.sprite = currentState switch
        {
            State.Lift => liftFaceSprite,
            State.Smash => smashFaceSprite,
            State.AfterLiftDelay => smashFaceSprite,
            State.Dead => deadFaceSprite,
            _ => restFaceSprite,
        };

        if (currentState == State.Dead && isPushableWhenDead)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            var pushable = gameObject.AddComponent<Pushable>();
            pushable.SetAllowedPushingDirections(allowedDirections);
            pushable.SetMaxPushCount(maxPushCount);
            pushable.setStaticAfterFinalPush = true;
        }
    }

    private ProjectileManager.StartingPointWithDirection[] CalculateStartingPoints()
    {
        var startingPoints = new ProjectileManager.StartingPointWithDirection[5];
        float startingAngle = targetRB == leftRB ? 45f : -45f;
        var position = targetRB.position;

        for (int i = 0; i < 5; i++)
        {
            float angle = startingAngle - (i * 45f);
            startingPoints[i] = new()
            {
                position = position,
                direction = Quaternion.Euler(0, 0, angle) * Vector2.right
            };
        }

        return startingPoints;
    }

    private void SelectNextTarget()
    {
        if (left == null)
        {
            targetRB = rightRB;
            targetDamageHandler = rightDamageHandler;
        }
        else if (right == null)
        {
            targetRB = leftRB;
            targetDamageHandler = leftDamageHandler;
        }
        else
        {
            targetRB = smashLeft ? leftRB : rightRB;
            targetDamageHandler = smashLeft ? leftDamageHandler : rightDamageHandler;
            smashLeft = !smashLeft;
        }
    }

    private void OnLeftHealthChanged(int currentHealth, int fullHealth) => OnHealthChanged(leftSR, currentHealth, fullHealth);
    private void OnRightHealthChanged(int currentHealth, int fullHealth) => OnHealthChanged(rightSR, currentHealth, fullHealth);

    public void OnHealthChanged(SpriteRenderer targetSR, int currentHealth, int fullHealth)
    {
        if ((float)currentHealth / fullHealth <= 0.4f) targetSR.sprite = twoThirdsDestroyedSprite;
        else if ((float)currentHealth / fullHealth <= 0.7f) targetSR.sprite = oneThirdDestroyedSprite;
    }

    public enum State
    {
        Lift,
        AfterLiftDelay,
        Smash,
        AfterSmashDelay,
        Ignore,
        Dead,
    }
}
