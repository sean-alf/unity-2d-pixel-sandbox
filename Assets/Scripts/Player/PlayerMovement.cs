using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Flags]
    public enum State
    {
        PAUSED = 1 << 1,
    }

    public readonly struct PlayerMovementData
    {
        // This Vector2 is normalized and rounded
        public readonly Vector2 currentDirection;
        // This Vector2 is normalized and rounded
        public readonly Vector2 previousDirection;
        // This Vector2 is normalized and rounded
        public readonly Vector2 blockedDirection;
        // The current position of the player
        public readonly Vector2 position;
        // Any state that is currently set
        // This is a bit field, so use bitwise operations with PlayerMovement.State enum values
        public readonly State state;
        // The current speed at which the player is moving
        public readonly float speed;

        public PlayerMovementData(
            Vector2 currentDirection,
            Vector2 previousDirection,
            Vector2 blockedDirection,
            Vector2 position,
            State state,
            float speed)
        {
            this.currentDirection = currentDirection;
            this.previousDirection = previousDirection;
            this.blockedDirection = blockedDirection;
            this.position = position;
            this.state = state;
            this.speed = speed;
        }

        public bool IsIdle()
        {
            return this.currentDirection.IsIdle();
        }

        public bool IsPaused()
        {
            return (this.state & State.PAUSED) != 0;
        }

        public override string ToString()
        {
            return $@"PlayerMovementData
    current direction {this.currentDirection}
    previous direction {this.previousDirection}
    position {this.position}
    state {this.state}
    speed {this.speed}";
        }

    }

    public readonly struct PlayerButtonData
    {
        public readonly bool primaryActionActive;

        public PlayerButtonData(bool primaryActionActive)
        {
            this.primaryActionActive = primaryActionActive;
        }
    }

    private static readonly string TAG = "PlayerMovement";
    private static readonly ILogger logger = Debug.unityLogger;
    private static readonly Vector2 DEFAULT_DIRECTION = new(0, -1);

    [SerializeField]
    [Tooltip("The normal movement speed")]
    private float speed;

    [Header("Debug")]
    [SerializeField]
    private bool enableLogs = false;
    [SerializeField]
    [Tooltip("Enables logs called within highly repetitive Unity functions such as Update/FixedUpdate")]
    private bool enableVerboseLogs = false;

    public event Action<PlayerMovementData> OnDirectionChange;
    public event Action<PlayerButtonData> OnButtonChange;
    public event Action OnAttackStart;
    public event Action OnAttackEnd;

    /// <summary>
    /// Return current direction if it's not idle, if it is idle, return previous direction, if previous direction is 
    /// idle return the default direction of down (x=0, y=1).
    /// This Vector2 is normalized and rounded.
    /// </summary>
    public Vector2 LastNonIdleDirection => !lastNonIdleDirection.IsIdle() ? lastNonIdleDirection : DEFAULT_DIRECTION;

    private Rigidbody2D rb;
    private PixelMovement2D movement2D;
    private PlayerInput input;
    private Collision2D collision = null;
    private Vector2 currentDirection;
    private Vector2 previousDirection = new();
    private Vector2 lastNonIdleDirection = DEFAULT_DIRECTION;
    private Vector2 currentBlockedDirection = new();
    private Vector2 prevBlockedDirection = new();
    private State currentState = 0;
    private State prevState = 0;
    private bool actionButtonPressed;

    private void PauseMovement()
    {
        if (enableLogs)
        {
            logger.Log(TAG, $"PauseMovement");
        }
        currentState |= State.PAUSED;
    }

    private void ResumeMovement()
    {
        if (enableLogs)
        {
            logger.Log(TAG, $"ResumeMovement");
        }
        currentState &= ~State.PAUSED;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();

        input.onActionTriggered += OnInput;
        // Make sure the initial position is not fractional
        rb.position = new(Mathf.Round(rb.position.x), Mathf.Round(rb.position.x));
        movement2D = new(rb)
        {
            enableLogs = enableVerboseLogs
        };
    }

    void FixedUpdate()
    {
        if (collision != null && collision.contactCount > 0)
        {
            // Combine the directions of all of the collision contacts
            foreach (var c in collision.contacts)
            {
                if (enableVerboseLogs)
                {
                    logger.Log(TAG, $"contact normal {c.normal}");
                }

                var v = c.normal.Round();

                if (v.IsHorizontal())
                {
                    currentBlockedDirection.x = -v.x;
                }

                if (v.IsVertical())
                {
                    currentBlockedDirection.y = -v.y;
                }
            }

            if (enableVerboseLogs)
            {
                logger.Log(TAG, $"contact directions {currentBlockedDirection}");
            }
        }
        else
        {
            currentBlockedDirection.x = 0;
            currentBlockedDirection.y = 0;
        }

        if (enableVerboseLogs)
        {
            logger.Log(TAG, $"blocked direction {currentBlockedDirection}");
        }

        if (!IsPaused())
        {
            movement2D.Move(currentDirection, currentBlockedDirection, speed);
        }

        if (previousDirection != currentDirection ||
            prevState != currentState ||
            prevBlockedDirection != currentBlockedDirection)
        {
            OnDirectionChange?.Invoke(new PlayerMovementData(
                currentDirection,
                previousDirection,
                currentBlockedDirection,
                rb.position,
                currentState,
                speed
            ));
            previousDirection = currentDirection;
            prevState = currentState;
            prevBlockedDirection = currentBlockedDirection;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        this.collision = collision;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        this.collision = collision;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (this.collision == collision)
        {
            this.collision = null;
        }
    }

    private void OnInput(InputAction.CallbackContext context)
    {
        if (context.action.name == "Look") return;

        switch (context.action.name)
        {
            case "Move":
                {
                    currentDirection = context.ReadValue<Vector2>().NormalizeAndRound();

                    if (!currentDirection.IsIdle())
                    {
                        // This is for determining which way the player is facing even when stopped
                        lastNonIdleDirection = currentDirection;
                    }

                    if (enableLogs)
                    {
                        logger.Log(name, $"Move input = {currentDirection}");
                    }
                    break;
                }
            case "Jump":
                {
                    actionButtonPressed = context.ReadValueAsButton();

                    if (enableLogs)
                    {
                        logger.Log(TAG, $"[PlayerMovement] Jump input = {actionButtonPressed}");
                    }

                    OnButtonChange?.Invoke(new(actionButtonPressed));
                    break;
                }
        }
    }

    void OnDestroy()
    {
        input.onActionTriggered -= OnInput;
    }

    public void Animator_OnAttackStart()
    {
        PauseMovement();
        OnAttackStart?.Invoke();
    }

    public void Animator_OnAttackEnd()
    {
        OnAttackEnd?.Invoke();
        ResumeMovement();
    }

    private bool IsPaused()
    {
        return (currentState & State.PAUSED) != 0;
    }

    void OnDrawGizmos()
    {
        movement2D?.DrawGizmos();
    }

    void OnValidate()
    {
        if (movement2D != null)
        {
            movement2D.enableLogs = enableVerboseLogs;
        }
    }
}
