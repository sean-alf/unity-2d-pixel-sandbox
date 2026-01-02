using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ProjectileManager))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    [Range(10, 200)]
    private float speed = 10;

    [SerializeField]
    private MenusAndDisplayManager madm;

    [Header("Debug")]
    [SerializeField]
    private bool enableLogs = false;

    private static readonly ILogger logger = Debug.unityLogger;

    private string Tag => $"Player2Movement:{name}";

    private Rigidbody2D rb;
    private PlayerInput input;
    private Animator animator;
    private Vector2 currentDirection;
    private Vector2 lastNonIdleDirection;
    private bool actionButtonPressed = false;
    private GameObject other;
    private ProjectileManager projectileManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        projectileManager = GetComponent<ProjectileManager>();
    }

    private void OnEnable()
    {
        if (input == null)
        {
            input = GetComponent<PlayerInput>();
        }

        input.onActionTriggered += OnInput;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!currentDirection.IsIdle())
        {
            Vector2 movementOffset = Time.fixedDeltaTime * speed * currentDirection;
            rb.MovePosition(movementOffset + rb.position);
        }
    }

    private void OnInput(InputAction.CallbackContext context)
    {
        if (context.action.name == "Look" || context.phase == InputActionPhase.Started) return;

        switch (context.action.name)
        {
            case "Move":
                {
                    currentDirection = context.ReadValue<Vector2>().NormalizeAndRound();

                    if (currentDirection.IsIdle())
                    {
                        animator.Play(PlayerAnimatorStates.BaseLayer.PLAYER_DEFAULT);
                    }
                    else
                    {
                        // This is for determining which way the player is facing even when stopped
                        lastNonIdleDirection = currentDirection;
                        rb.SetRotation(Quaternion.LookRotation(Vector3.forward, currentDirection));
                        animator.Play(PlayerAnimatorStates.BaseLayer.PLAYER_MOVING);
                    }

                    if (enableLogs)
                    {
                        logger.Log(Tag, $"Move input = {currentDirection}");
                    }
                    break;
                }
            case "Jump":
                {
                    actionButtonPressed = context.ReadValueAsButton();

                    if (actionButtonPressed)
                    {
                        if (other != null && other.TryGetComponent(out AccessPanel p))
                        {
                            p.Activate();
                        }
                        else
                        {
                            projectileManager.Shoot(lastNonIdleDirection, transform);
                        }
                    }

                    if (enableLogs)
                    {
                        logger.Log(Tag, $"[PlayerMovement] Jump input = {actionButtonPressed}");
                    }
                    break;
                }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (this.other != other.gameObject)
        {
            this.other = other.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (this.other == other.gameObject)
        {
            this.other = null;
        }
    }

    private void OnDisable()
    {
        input.onActionTriggered -= OnInput;
    }

}
