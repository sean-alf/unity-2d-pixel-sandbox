using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ProjectileManager))]
public class PlayerMovement : MonoBehaviour, ProjectileManager.IDirectionProvider
{
    [SerializeField]
    [Range(10, 100)]
    private int speed = 10;

    [Header("Debug")]
    [SerializeField]
    private bool enableLogs = false;

    private static readonly ILogger logger = Debug.unityLogger;

    private string Tag => $"Player2Movement:{name}";

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput input;
    private ProjectileManager projectileManager;
    private Vector2 currentDirection;
    private Vector2 lastNonIdleDirection = Vector2.up;
    private GameObject other;

    public Vector2 LookDirection => lastNonIdleDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        projectileManager = GetComponent<ProjectileManager>();

        InputSystem_Actions_Names.Player.Move(input).performed += OnMove;
        InputSystem_Actions_Names.Player.Move(input).canceled += OnMove;
        InputSystem_Actions_Names.Player.Interact(input).performed += OnInteract;
        InputSystem_Actions_Names.Player.Jump(input).performed += OnJump;
        InputSystem_Actions_Names.Player.Previous(input).performed += OnPrevious;
        InputSystem_Actions_Names.Player.Next(input).performed += OnNext;
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

    private void OnMove(InputAction.CallbackContext context)
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
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (other != null && other.TryGetComponent(out AccessPanel p))
        {
            p.Activate();
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        projectileManager.Shoot();
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        projectileManager.SelectPrevious();
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        projectileManager.SelectNext();
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

    private void OnDestroy()
    {
        InputSystem_Actions_Names.Player.Move(input).performed -= OnMove;
        InputSystem_Actions_Names.Player.Move(input).canceled -= OnMove;
        InputSystem_Actions_Names.Player.Interact(input).performed -= OnInteract;
        InputSystem_Actions_Names.Player.Jump(input).performed -= OnJump;
        InputSystem_Actions_Names.Player.Previous(input).performed -= OnPrevious;
        InputSystem_Actions_Names.Player.Next(input).performed -= OnNext;
    }
}
