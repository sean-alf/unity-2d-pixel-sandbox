using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ProjectileManager))]
[RequireComponent(typeof(HealthManager))]
public class PlayerController : MonoBehaviour, AutoMover.IAutoMoverTarget
{
    private static readonly float SPEED_CONSTANT = 20.0f;
    private static readonly int TO_EDGE_OF_EYE_PX = 4;

    [SerializeField]
    [Range(1, 10)]
    private int speed = 1;

    [Header("Debug")]
    [SerializeField]
    private bool enableLogs = false;

    private static readonly ILogger logger = Debug.unityLogger;

    private string Tag => $"Player2Movement:{name}";

    public float Speed => Time.fixedDeltaTime * speed * SPEED_CONSTANT;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput input;
    private ProjectileManager projectileManager;
    private HealthManager healthManager;
    private Vector2 currentDirection;
    private Vector2 lastNonIdleDirection = Vector2.up;
    private GameObject other;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        projectileManager = GetComponent<ProjectileManager>();
        healthManager = GetComponent<HealthManager>();

        EnableInput();

        projectileManager.SetShootingLayer(LayerNames.Projectile);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!currentDirection.IsIdle())
        {
            Vector2 movementOffset = Time.fixedDeltaTime * speed * SPEED_CONSTANT * currentDirection;
            rb.MovePosition(movementOffset + rb.position);
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        currentDirection = context.ReadValue<Vector2>().normalized;
        OnDirectionChanged(currentDirection);
    }

    private void OnDirectionChanged(Vector2 direction)
    {
        if (direction.IsIdle())
        {
            animator.Play(PlayerAnimatorStates.BaseLayer.PLAYER_DEFAULT);
        }
        else
        {
            // This is for determining which way the player is facing even when stopped
            lastNonIdleDirection = direction;
            rb.SetRotation(Quaternion.LookRotation(Vector3.forward, direction));
            animator.Play(PlayerAnimatorStates.BaseLayer.PLAYER_MOVING);
        }

        if (enableLogs)
        {
            logger.Log(Tag, $"Direction changed = {direction}");
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (other != null)
        {
            if (other.TryGetComponent(out AccessPanel p))
            {
                p.Activate();
            }
            else if (other.TryGetComponent(out HealthPickup h))
            {
                healthManager.Heal(h.HealAmount);
                Destroy(h.gameObject);
            }
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        projectileManager.Shoot(new ProjectileManager.StartingPointWithDirection
        {
            direction = lastNonIdleDirection,
            position = transform.position.Add(TO_EDGE_OF_EYE_PX * lastNonIdleDirection.normalized),
        });
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
        rb.linearVelocity = Vector2.zero;

        if (other.gameObject.TryGetComponent(out CollisionData data))
        {
            if (data.type == CollisionData.Type.Damage)
            {
                logger.LogWarning(Tag, $"Hit by {LayerMask.LayerToName(data.gameObject.layer)}");
                healthManager.DoDamage(data.strength);
                return;
            }
        }
        else if (this.other != other.gameObject)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Teleport t))
        {
            t.AnimateAndTeleportToNextScene(gameObject);
        }
    }

    private void OnDestroy()
    {
        DisableInput();
    }

    public void EnableInput()
    {
        InputSystem_Actions_Names.Player.Move(input).performed += OnMove;
        InputSystem_Actions_Names.Player.Move(input).canceled += OnMove;
        InputSystem_Actions_Names.Player.Interact(input).performed += OnInteract;
        InputSystem_Actions_Names.Player.Jump(input).performed += OnJump;
        InputSystem_Actions_Names.Player.Previous(input).performed += OnPrevious;
        InputSystem_Actions_Names.Player.Next(input).performed += OnNext;
    }

    public void DisableInput()
    {
        // Stop the movement
        currentDirection = Vector2.zero;

        InputSystem_Actions_Names.Player.Move(input).performed -= OnMove;
        InputSystem_Actions_Names.Player.Move(input).canceled -= OnMove;
        InputSystem_Actions_Names.Player.Interact(input).performed -= OnInteract;
        InputSystem_Actions_Names.Player.Jump(input).performed -= OnJump;
        InputSystem_Actions_Names.Player.Previous(input).performed -= OnPrevious;
        InputSystem_Actions_Names.Player.Next(input).performed -= OnNext;
    }

    void AutoMover.IAutoMoverTarget.OnDirectionChanged(Vector2 direction)
    {
        OnDirectionChanged(direction);
    }
}
